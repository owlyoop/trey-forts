using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public struct MyPlayerCommands : IPlayerCommands
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool Fire;

        public void ReadFromBuffer(SerializationBuffer buf)
        {
            MoveAxisForward = buf.ReadFloat();
            MoveAxisRight = buf.ReadFloat();
            CameraRotation = buf.ReadQuaternion();
            JumpDown = buf.ReadBool();
            Fire = buf.ReadBool();
        }

        public void WriteToBuffer(SerializationBuffer buf, bool mustBeFullState)
        {
            buf.WriteFloat(MoveAxisForward);
            buf.WriteFloat(MoveAxisRight);
            buf.WriteQuaternion(CameraRotation);
            buf.WriteBool(JumpDown);
            buf.WriteBool(Fire);
        }
    }
}