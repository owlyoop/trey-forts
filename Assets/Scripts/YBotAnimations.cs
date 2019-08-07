using KinematicCharacterController;
using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBotAnimations : MonoBehaviour {

	public GameObject ragdollPrefab;

	public PlayerInput playerInputs;

	public Animator animator;

	public float InputForward;
	public float InputRight;

	public bool KickInput;
	public bool JumpInput;
	public bool CrouchInput;


	public MyCamera cam;
	public Transform chest;
	public Vector3 Offset;

	KinematicCharacterMotor playerKCM;

	void Start ()
	{
		animator = this.gameObject.GetComponent<Animator>();
		playerKCM = playerInputs.GetComponent<MyCharacterController>().Motor;
	}
	
	void Update ()
	{

		KickInput = playerInputs.playerInputs.MeleeKick;
		JumpInput = playerInputs.playerInputs.Jump;
		CrouchInput = playerInputs.playerInputs.Crouch;

		InputForward = Input.GetAxis("Vertical");
		InputRight = Input.GetAxis("Horizontal");

		animator.SetFloat("InputForward", InputForward);
		animator.SetFloat("InputRight", InputRight);
		animator.SetBool("IsGrounded", playerKCM.GroundingStatus.FoundAnyGround);
		animator.SetBool("Kick Pressed", KickInput);
		animator.SetBool("Jump Pressed", JumpInput);
		animator.SetBool("Crouch Held", CrouchInput);
		
	}

	private void LateUpdate()
	{
		
		//chest.LookAt(cam.LookAtPosition);
		//chest.rotation = chest.rotation * Quaternion.Euler(Offset);
	}

	void KickAnimation()
	{
		
	}
}
