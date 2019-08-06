using KinematicCharacterController.Examples;
using System;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public class MyPlayerController : MonoBehaviour, IPlayerController<MyPlayerCommands>
    {
        public OrbitCamera OrbitCamera;
        public MyCharacterController Character;
        public float MouseSensitivity = 0.01f;
        public bool UsePrediction = false;

        [NonSerialized]
        public MyPlayerCommands CurrentCommands;

        private bool _isLocal = true;
        private int _id = -1;
        private int _connectionId = -1;

        private bool _hasProcessedTransientCommands = false;
        private MyPlayerCommands _transientCommands = new MyPlayerCommands();

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private void LateUpdate()
        {
            if (GetIsLocal())
            {
                HandleCameraInput();
                HandleTransientInputs();

                if(Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void HandleCameraInput()
        {
            if (OrbitCamera)
            {
                // Create the look input vector for the camera
                float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
                float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
                Vector3 lookInputVector = new Vector3(mouseLookAxisRight * MouseSensitivity, mouseLookAxisUp * MouseSensitivity, 0f);

                // Input for zooming the camera (disabled in WebGL because it can cause problems)
                float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

                // Apply inputs to the camera
                OrbitCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

                // Handle toggling zoom level
                if (Input.GetMouseButtonDown(1))
                {
                    OrbitCamera.TargetDistance = (OrbitCamera.TargetDistance == 0f) ? OrbitCamera.DefaultDistance : 0f;
                }
            }
        }

        private void HandleTransientInputs()
        {
            if (OrbitCamera && Character)
            {
                float tmpVInput = Input.GetAxisRaw(VerticalInput);
                if (tmpVInput != 0f)
                {
                    _transientCommands.MoveAxisForward = tmpVInput;
                }

                float tmpHInput = Input.GetAxisRaw(HorizontalInput);
                if (tmpHInput != 0f)
                {
                    _transientCommands.MoveAxisRight = tmpHInput;
                }

                bool tmpJumpInput = Input.GetKeyDown(KeyCode.Space);
                if (tmpJumpInput != false)
                {
                    _transientCommands.JumpDown = tmpJumpInput;
                }

                bool tmpFireInput = Input.GetMouseButtonDown(0);
                if (tmpFireInput != false)
                {
                    _transientCommands.Fire = tmpFireInput;
                }

                _transientCommands.CameraRotation = OrbitCamera.Transform.rotation;

                _hasProcessedTransientCommands = true;
            }
        }

        public void UpdateLocalCommands(float deltaTime)
        {
            if (!_hasProcessedTransientCommands)
            {
                HandleTransientInputs();
            }

            SetCommands(_transientCommands);

            _transientCommands = new MyPlayerCommands();
            _hasProcessedTransientCommands = false;
        }

        public void ApplyCommands(bool allowEvents)
        {
            if (Character)
            {
                Character.SetCommands(ref CurrentCommands);

                // Handle gun
                if (allowEvents)
                {
                    if (CurrentCommands.Fire)
                    {
                        Character.LazerGun.Fire(CurrentCommands.CameraRotation);
                    }
                }
            }
        }

        public MyPlayerCommands GetCommands()
        {
            return CurrentCommands;
        }

        public void SetCommands(MyPlayerCommands playerCommands)
        {
            CurrentCommands = playerCommands;
        }

        public MyPlayerCommands GetNewCommands(MyPlayerCommands previousCommands)
        {
            if (UsePrediction)
            {
                return previousCommands;
            }
            else
            {
                MyPlayerCommands newCommands = new MyPlayerCommands();
                newCommands.CameraRotation = previousCommands.CameraRotation;
                return newCommands;
            }
        }

        public int GetId()
        {
            return _id;
        }

        public void SetId(int id)
        {
            _id = id;
        }

        public bool GetIsLocal()
        {
            return _isLocal;
        }

        public void SetIsLocal(bool local)
        {
            _isLocal = local;
        }

        public int GetConnectionId()
        {
            return _connectionId;
        }

        public void SetConnectionId(int id)
        {
            _connectionId = id;
        }
    }
}