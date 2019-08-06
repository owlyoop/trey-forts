using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public enum FrameworkNetworkMessageID
    {
        PlayerCommands = -1000,
        GlobalState,
        CommandRequest,
    }

    public struct CommandRequestMsg
    {
        public int ForPlayerId;
        public int ForTick;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)FrameworkNetworkMessageID.CommandRequest);

            buf.WriteInt(ForPlayerId);
            buf.WriteInt(ForTick);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            ForPlayerId = buf.ReadInt();
            ForTick = buf.ReadInt();
        }
    }
}