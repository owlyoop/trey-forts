using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using Photon.Pun;

namespace KinematicCharacterController.Owly
{
	public enum CharacterState
	{
		Default,
		Dead,
		Bunnyhop,
		Spectator,
		Divekick
	}

	public enum OrientationMethod
	{
		TowardsCamera,
		TowardsMovement,
	}

	public struct PlayerCharacterInputs
	{
		public float MoveAxisForward;
		public float MoveAxisRight;
		public Quaternion CameraRotation;
		public bool JumpDown;
		public bool JumpHeld;
	}

	public struct AICharacterInputs
	{
		public Vector3 MoveVector;
		public Vector3 LookVector;
	}

	public class MyCharacterController : BaseCharacterController
	{
		public PlayerStats player;
		Camera cam;
		[Header("Stable Movement")]
		public float MaxStableMoveSpeed = 10f;
		public float StableMovementSharpness = 15;
		public float OrientationSharpness = 10;
		public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

		[Header("Air Movement")]
		public float MaxAirMoveSpeed = 10f;
		public float AirAccelerationSpeed = 5f;
		public float AirDeaccelerationSpeed = 2f;
		public float Drag = 0.1f;
		public float DivekickSpeed = 11f;


		[Header("Bunnyhop")]
		public float BhopAirAccelSpeed = 1f;
		public float BhopAirDeaccelSpeed = 1f;
		public float airControl = 0.3f;               // How precise air control is
		public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
		public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
		public float BhopDrag = 0.04f;
		public float BhopFrictionReduction = 20f;

		[Header("Spectator Noclip")]
		public float NoClipMoveSpeed = 10f;
		public float NoClipSharpness = 15;

		[Header("Jumping")]
		public bool AllowJumpingWhenSliding = false;
		public float JumpSpeed = 10f;
		public float JumpPreGroundingGraceTime = 0f;
		public float JumpPostGroundingGraceTime = 0f;

		[Header("Misc")]
		public List<Collider> IgnoredColliders = new List<Collider>();
		public bool OrientTowardsGravity = false;
		public Vector3 Gravity = new Vector3(0, -30f, 0);
		public Transform MeshRoot;
		public Transform CameraFollowPoint;

		public CharacterState CurrentCharacterState { get; private set; }

		private Collider[] _probedColliders = new Collider[8];
		private Vector3 _moveInputVector;
		private Vector3 _lookInputVector;
		private bool _jumpRequested = false;
		private bool _jumpConsumed = false;
		private bool _jumpedThisFrame = false;
		private bool _jumpInputIsHeld = false;
		private float _timeSinceJumpRequested = Mathf.Infinity;
		private float _timeSinceLastAbleToJump = 0f;
		private Vector3 _internalVelocityAdd = Vector3.zero;
		private bool _shouldBeCrouching = false;
		private bool _crouchInputIsHeld = false;
		private bool _isCrouching = false;
		private Vector3 _divekickDirection;

		private Vector3 lastInnerNormal = Vector3.zero;
		private Vector3 lastOuterNormal = Vector3.zero;

		public float currentSpeed;
		public float dot;

        public LayerMask collidableLayers;

		private void Start()
		{
			if (!photonView.IsMine)
				return;
			// Handle initial state
			TransitionToState(CharacterState.Default);
			cam = GetComponent<PlayerInput>().cam;

            Motor.CollidableLayers = collidableLayers;
		}

		private void Update()
		{
			if (!photonView.IsMine)
				return;
		}

		/// <summary>
		/// Handles movement state transitions and enter/exit callbacks
		/// </summary>
		public void TransitionToState(CharacterState newState)
		{
			CharacterState tmpInitialState = CurrentCharacterState;
			OnStateExit(tmpInitialState, newState);
			CurrentCharacterState = newState;
			OnStateEnter(newState, tmpInitialState);
		}

		/// <summary>
		/// Event when entering a state
		/// </summary>
		public void OnStateEnter(CharacterState state, CharacterState fromState)
		{
			switch (state)
			{
				case CharacterState.Default:
					{
						break;
					}
				case CharacterState.Spectator:
					{
						Motor.SetCapsuleCollisionsActivation(false);
						Motor.SetMovementCollisionsSolvingActivation(false);
						Motor.SetGroundSolvingActivation(false);
						break;
					}
				case CharacterState.Divekick:
					{
                        _internalVelocityAdd = -(Motor.Velocity * 0.5f);
						player.wepSlots.DecactivateAllWeapons();
						_divekickDirection = cam.transform.forward;
						_divekickDirection = new Vector3(_divekickDirection.x, 0, _divekickDirection.z);
                        AddVelocity(_divekickDirection.normalized * DivekickSpeed);
                        player.divekickHitbox.ActivateDivekick();
                        StartCoroutine(DivekickExpireTimer());
						break;
					}

			}
		}

		/// <summary>
		/// Event when exiting a state
		/// </summary>
		public void OnStateExit(CharacterState state, CharacterState toState)
		{
			switch (state)
			{
				case CharacterState.Default:
					{
						break;
					}
				case CharacterState.Spectator:
					{
						Motor.SetCapsuleCollisionsActivation(true);
						Motor.SetMovementCollisionsSolvingActivation(true);
						Motor.SetGroundSolvingActivation(true);
						break;
					}
				case CharacterState.Divekick:
					{
                        if (toState != CharacterState.Spectator || toState != CharacterState.Dead)
                        {
                            player.wepSlots.SwitchActiveWeaponSlot(player.wepSlots.activeWeaponIndex);
                        }
                        StopCoroutine(DivekickExpireTimer());
                        player.divekickHitbox.DeactivateDivekick();
                        break;
					}
			}
		}

		/// <summary>
		/// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
		/// </summary>
		public void SetInputs(ref PlayerCharacterInputs inputs)
		{
			_jumpInputIsHeld = inputs.JumpHeld;
			// Clamp input
			Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

			// Calculate camera direction and rotation on the character plane
			Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
			if (cameraPlanarDirection.sqrMagnitude == 0f)
			{
				cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
			}
			Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

			switch (CurrentCharacterState)
			{
				case CharacterState.Bunnyhop:
				case CharacterState.Default:
					{
						// Move and look inputs
						_moveInputVector = cameraPlanarRotation * moveInputVector;

						switch (OrientationMethod)
						{
							case OrientationMethod.TowardsCamera:
								_lookInputVector = cameraPlanarDirection;
								break;
							case OrientationMethod.TowardsMovement:
								_lookInputVector = _moveInputVector.normalized;
								break;
						}

						// Jumping input
						if (inputs.JumpDown)
						{
							_timeSinceJumpRequested = 0f;
							_jumpRequested = true;
						}

						break;
					}

				case CharacterState.Spectator:
					{
						_moveInputVector = inputs.CameraRotation * moveInputVector;
						_lookInputVector = cameraPlanarDirection;
						break;
					}
			}
		}

		/// <summary>
		/// This is called every frame by the AI script in order to tell the character what its inputs are
		/// </summary>
		public void SetInputs(ref AICharacterInputs inputs)
		{
			_moveInputVector = inputs.MoveVector;
			_lookInputVector = inputs.LookVector;
			
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called before the character begins its movement update
		/// </summary>
		public override void BeforeCharacterUpdate(float deltaTime)
		{
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its rotation should be right now. 
		/// This is the ONLY place where you should set the character's rotation
		/// </summary>
		public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Spectator:
				case CharacterState.Bunnyhop:
				case CharacterState.Default:
					{
						if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
						{
							// Smoothly interpolate from current to target look direction
							Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

							// Set the current rotation (which will be used by the KinematicCharacterMotor)
							currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
						}
						if (OrientTowardsGravity)
						{
							// Rotate from current up to invert gravity
							currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
						}
						break;
					}
			}
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its velocity should be right now. 
		/// This is the ONLY place where you can set the character's velocity
		/// </summary>
		public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
					{
						Vector3 targetMovementVelocity = Vector3.zero;

						// Ground movement
						if (Motor.GroundingStatus.IsStableOnGround)
						{
							Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
							if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
							{
								// Take the normal from where we're coming from
								Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
								if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
								{
									effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
								}
								else
								{
									effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
								}
							}

							// Reorient velocity on slope
							currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocity.magnitude;

							// Calculate target velocity
							Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
							Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
							targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

							// Smooth movement Velocity
							currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
						}
						// Air movement
						else
						{
							// Add move input
							if (_moveInputVector.sqrMagnitude > 0f)
							{
								targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

								// Prevent climbing on un-stable slopes with air movement
								if (Motor.GroundingStatus.FoundAnyGround)
								{
									Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
									targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
								}

								Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
								currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
							}

							// Gravity
							currentVelocity += Gravity * deltaTime;

							// Drag
							currentVelocity *= (1f / (1f + (Drag * deltaTime)));
						}

						// Handle jumping
						_jumpedThisFrame = false;
						_timeSinceJumpRequested += deltaTime;
						if (_jumpRequested)
						{
							// See if we actually are allowed to jump
							if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
							{
								// Calculate jump direction before ungrounding
								Vector3 jumpDirection = Motor.CharacterUp;
								if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
								{
									jumpDirection = Motor.GroundingStatus.GroundNormal;
								}

								// Makes the character skip ground probing/snapping on its next update. 
								// If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
								Motor.ForceUnground();

								// Add to the return velocity and reset jump state
								currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
								_jumpRequested = false;
								_jumpConsumed = true;
								_jumpedThisFrame = true;
							}
						}

						// Take into account additive velocity
						if (_internalVelocityAdd.sqrMagnitude > 0f)
						{
							currentVelocity += _internalVelocityAdd;
							_internalVelocityAdd = Vector3.zero;
						}
						break;
					}

				case CharacterState.Dead:
					{
						currentVelocity = Vector3.zero;
						_internalVelocityAdd = Vector3.zero;
						break;
					}

				case CharacterState.Bunnyhop:
					{
						Vector3 targetMovementVelocity = Vector3.zero;
						
						// Ground movement
						if (Motor.GroundingStatus.IsStableOnGround)
						{
							Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
							if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
							{
								// Take the normal from where we're coming from
								Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
								if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
								{
									effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
								}
								else
								{
									effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
								}
							}

							// Reorient velocity on slope
							currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocity.magnitude;

							// Calculate target velocity
							Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
							Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
							targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

							if (_jumpRequested)
							{
								float friction;
								if (currentVelocity.magnitude > 18f)
								{
									friction = 18f;
								}
								else friction = currentVelocity.magnitude;
								//currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp((-StableMovementSharpness / (BhopFrictionReduction - friction)) * deltaTime));
							}
							else
							{
								// Smooth movement Velocity
								currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
							}
							
							
						}
						// Air movement
						else
						{
							// Add move input
							if (_moveInputVector.sqrMagnitude > 0f)
							{
								//targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;
								Vector3 wishDir;

								float accel;

								float forward = Input.GetAxisRaw("Vertical");
								float horiz = Input.GetAxisRaw("Horizontal");

								wishDir = new Vector3(horiz, 0, forward);
								wishDir = transform.TransformDirection(wishDir);

                                
                                float wishSpeed = wishDir.magnitude;
								wishSpeed *= MaxStableMoveSpeed;
								wishDir.Normalize();

								//wishSpeed *= 0.12f;

								//cpm
								float wishSpeed2 = wishSpeed;
								if (Vector3.Dot(currentVelocity, wishDir) < 0)
								{
									accel = BhopAirAccelSpeed;
								}
								else
								{
									accel = BhopAirDeaccelSpeed;
								}

								if (horiz != 0)
								{
									if (wishSpeed > sideStrafeSpeed)
										wishSpeed = sideStrafeSpeed;
									accel = sideStrafeAcceleration;
									
								}
								currentVelocity = Accelerate(wishDir, wishSpeed, accel, currentVelocity);
								

								if (airControl > 0)
								{
									currentVelocity = AirControl(wishDir, wishSpeed2, currentVelocity);
								}

								// Prevent climbing on un-stable slopes with air movement
								if (Motor.GroundingStatus.FoundAnyGround)
								{
									Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
									targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
								}

								Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
								
							}

							// Gravity
							currentVelocity += Gravity * deltaTime;

							// Drag
							currentVelocity *= (1f / (1f + (BhopDrag * deltaTime)));
							
						}

						// Handle jumping
						_jumpedThisFrame = false;
						_timeSinceJumpRequested += deltaTime;
						if (_jumpRequested)
						{
							// See if we actually are allowed to jump
							if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
							{
								// Calculate jump direction before ungrounding
								Vector3 jumpDirection = Motor.CharacterUp;
								if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
								{
									jumpDirection = Motor.GroundingStatus.GroundNormal;
								}

								// Makes the character skip ground probing/snapping on its next update. 
								// If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
								Motor.ForceUnground();

								// Add to the return velocity and reset jump state
								currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
								_jumpRequested = false;
								_jumpConsumed = true;
								_jumpedThisFrame = true;
								player.StatusEffectManager.OnPlayerJump();
							}
						}

						// Take into account additive velocity
						if (_internalVelocityAdd.sqrMagnitude > 0f)
						{
							currentVelocity += _internalVelocityAdd;
							_internalVelocityAdd = Vector3.zero;
						}

						currentSpeed = currentVelocity.magnitude;
						break;
					}

				case CharacterState.Spectator:
					{
						float verticalInput = 0f + (_jumpInputIsHeld ? 1f : 0f);// + (_crouchInputIsHeld ? -1f : 0f);

						// Smoothly interpolate to target velocity
						Vector3 targetMovementVelocity = (_moveInputVector + (Motor.CharacterUp * verticalInput)).normalized * NoClipMoveSpeed;
						currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-NoClipSharpness * deltaTime));
						break;
					}

				case CharacterState.Divekick:
					{
						if (Motor.GroundingStatus.IsStableOnGround)
						{
                              if (player.currentHealth > 0)
					              TransitionToState(player.playerClass.defaultState);
						}
						else
						{
                            Vector3 targetMovementVelocity = Vector3.zero;

                            // Add move input
                            if (_moveInputVector.sqrMagnitude > 0f)
                            {
                                targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;
                                Vector3 wishDir;

                                float accel;

                                float forward = Input.GetAxisRaw("Vertical");
                                float horiz = Input.GetAxisRaw("Horizontal");

                                wishDir = new Vector3(horiz, 0, forward);
                                wishDir = transform.TransformDirection(wishDir);

                                float wishSpeed = wishDir.magnitude;
                                wishSpeed *= MaxStableMoveSpeed;
                                wishDir.Normalize();

                                wishSpeed *= 0.12f;

                                //cpm
                                float wishSpeed2 = wishSpeed;
                                if (Vector3.Dot(currentVelocity, wishDir) < 0)
                                {
                                    accel = BhopAirAccelSpeed;
                                }
                                else
                                {
                                    accel = BhopAirDeaccelSpeed;
                                }

                                if (horiz != 0)
                                {
                                    if (wishSpeed > sideStrafeSpeed)
                                        wishSpeed = sideStrafeSpeed;
                                    accel = sideStrafeAcceleration;

                                }
                                currentVelocity = Accelerate(wishDir, wishSpeed, accel, currentVelocity);


                                if (airControl > 0)
                                {
                                    currentVelocity = AirControl(wishDir, wishSpeed2, currentVelocity);
                                }
                            }

                            // Gravity
                            currentVelocity += (Gravity/2f) * deltaTime;


                            // Take into account additive velocity
                            if (_internalVelocityAdd.sqrMagnitude > 0f)
                            {
                                currentVelocity += _internalVelocityAdd;
                                _internalVelocityAdd = Vector3.zero;
                            }
                        }
						break;
					}
			}
		}

		Vector3 Accelerate(Vector3 wishdir, float wishspeed, float accel, Vector3 currentVel)
		{
			float addSpeed;
			float accelSpeed;
			float currentSpeed;

			currentSpeed = Vector3.Dot(currentVel, wishdir);
			addSpeed = wishspeed - currentSpeed;
			if (addSpeed <= 0)
			{
				return currentVel;
			}
				

			accelSpeed = accel * Time.deltaTime * wishspeed;
			if (accelSpeed > addSpeed)
				accelSpeed = addSpeed;



			currentVel.x += accelSpeed * wishdir.x;
			currentVel.z += accelSpeed * wishdir.z;

			return currentVel;
		}

		Vector3 AirControl(Vector3 targetMovementVelocity, float wishSpeed, Vector3 currentVelocity)
		{
			if (Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
				return currentVelocity;
			float zspeed = currentVelocity.y;
			currentVelocity.y = 0;

			float speed = currentVelocity.magnitude;
			currentVelocity.Normalize();

			dot = Vector3.Dot(currentVelocity, targetMovementVelocity);
			float k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			if (dot > 0)
			{
				currentVelocity.x = currentVelocity.x * speed + targetMovementVelocity.x * k;
				currentVelocity.y = currentVelocity.y * speed + targetMovementVelocity.y * k;
				currentVelocity.z = currentVelocity.z * speed + targetMovementVelocity.z * k;

				currentVelocity.Normalize();
			}

			currentVelocity.x *= speed;
			currentVelocity.y = zspeed;
			currentVelocity.z *= speed;

			return currentVelocity;
		}

        IEnumerator DivekickExpireTimer()
        {
            yield return new WaitForSeconds(2f);
            if (CurrentCharacterState == CharacterState.Divekick)
            {
                if (player.currentHealth > 0)
                    TransitionToState(player.playerClass.defaultState);
            }
        }

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called after the character has finished its movement update
		/// </summary>
		public override void AfterCharacterUpdate(float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Spectator:
				case CharacterState.Bunnyhop:
				case CharacterState.Default:
					{
						// Handle jump-related values
						{
							// Handle jumping pre-ground grace period
							if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
							{
								_jumpRequested = false;
							}

							if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
							{
								// If we're on a ground surface, reset jumping values
								if (!_jumpedThisFrame)
								{
									_jumpConsumed = false;
								}
								_timeSinceLastAbleToJump = 0f;
							}
							else
							{
								// Keep track of time since we were last able to jump (for grace period)
								_timeSinceLastAbleToJump += deltaTime;
							}
						}

						// Handle uncrouching
						if (_isCrouching && !_shouldBeCrouching)
						{
							// Do an overlap test with the character's standing height to see if there are any obstructions
							Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
							if (Motor.CharacterOverlap(
								Motor.TransientPosition,
								Motor.TransientRotation,
								_probedColliders,
								Motor.CollidableLayers,
								QueryTriggerInteraction.Ignore) > 0)
							{
								// If obstructions, just stick to crouching dimensions
								Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
							}
							else
							{
								// If no obstructions, uncrouch
								MeshRoot.localScale = new Vector3(1f, 1f, 1f);
								_isCrouching = false;
							}
						}
						break;
					}
			}
		}

		public override void PostGroundingUpdate(float deltaTime)
		{
			// Handle landing and leaving ground
			if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
			{
				OnLanded();
			}
			else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
			{
				OnLeaveStableGround();
			}
		}

		public override bool IsColliderValidForCollisions(Collider coll)
		{
			if (IgnoredColliders.Count == 0)
			{
				return true;
			}

			if (IgnoredColliders.Contains(coll))
			{
				return false;
			}
			return true;
		}

		public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
		}

		public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
		}

		public void AddVelocity(Vector3 velocity)
		{
			switch (CurrentCharacterState)
			{

				case CharacterState.Bunnyhop:
				case CharacterState.Default:
                case CharacterState.Divekick:
					{
						_internalVelocityAdd += velocity;
						break;
					}
			}
		}

		public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
		{
		}

		protected void OnLanded()
		{
		}

		protected void OnLeaveStableGround()
		{
		}
	}
}