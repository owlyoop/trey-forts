using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public struct MyWorldSnapshot : IWorldSnapshot<MyWorldSnapshot>
    {
        public KinematicCharacterSubsystemState KinematicCharacterSubsystemState;
        
        public void ReadFromBuffer(SerializationBuffer buf)
        {
            KinematicCharacterSubsystemState.ReadFromBuffer(buf);
        }

        public void WriteToBuffer(SerializationBuffer buf, bool mustBeFullState)
        {
            KinematicCharacterSubsystemState.WriteToBuffer(buf, mustBeFullState);
        }
    }
}