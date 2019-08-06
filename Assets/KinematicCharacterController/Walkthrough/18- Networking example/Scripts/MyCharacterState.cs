using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public struct MyCharacterState
    {
        public int CharacterId;

        public KinematicCharacterMotorState MotorState;
        public bool JumpConsumed;
        public float TimeSinceJumpRequested;
        public float TimeSinceLastAbleToJump;
        public bool IsStunned;
        public float StunTimeRemaining;
    }
}