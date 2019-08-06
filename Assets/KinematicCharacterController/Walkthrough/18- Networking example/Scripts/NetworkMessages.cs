using System;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public enum NetworkMessageID
    {
        PlayerRegistration = 1000,
        PlayerAdd,
        SceneLoading,
        TickSync,
        SimpleEvent,
        SpawnPlayer,
        SpawnCharacter,
        LazerHit,
    }

    public struct PlayerRegistrationMsg
    {
        public string PlayerName;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.PlayerRegistration);

            buf.WriteString(PlayerName);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            PlayerName = buf.ReadString();
        }
    }

    public struct PlayerAddMsg
    {
        public bool Add;
        public bool ForLocalPlayer;
        public int PlayerId;
        public string PlayerName;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.PlayerAdd);

            buf.WriteBool(Add);
            buf.WriteBool(ForLocalPlayer);
            buf.WriteInt(PlayerId);
            buf.WriteString(PlayerName);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            Add = buf.ReadBool();
            ForLocalPlayer = buf.ReadBool();
            PlayerId = buf.ReadInt();
            PlayerName = buf.ReadString();
        }
    }

    public struct SceneLoadingMsg
    {
        public string SceneName;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.SceneLoading);

            buf.WriteString(SceneName);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            SceneName = buf.ReadString();
        }
    }

    public struct TickSyncMsg
    {
        public int Tick;
        public int Timestamp;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.TickSync);

            buf.WriteInt(Tick);
            buf.WriteInt(Timestamp);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            Tick = buf.ReadInt();
            Timestamp = buf.ReadInt();
        }
    }

    public struct SimpleEventMsg
    {
        public enum EventType
        {
            Invalid,
            TickSynced,
            InitiateGame,
        }

        public EventType Event;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.SimpleEvent);

            buf.WriteInt((int)Event);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            Event = (EventType)buf.ReadInt();
        }
    }

    public struct SpawnPlayerMsg
    {
        public int PlayerId;
        public bool IsLocalPlayer;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.SpawnPlayer);

            buf.WriteInt(PlayerId);
            buf.WriteBool(IsLocalPlayer);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            PlayerId = buf.ReadInt();
            IsLocalPlayer = buf.ReadBool();
        }
    }

    public struct SpawnCharacterMsg
    {
        public int CharacterId;
        public int ForPlayerId;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.SpawnCharacter);

            buf.WriteInt(CharacterId);
            buf.WriteInt(ForPlayerId);
            buf.WriteVector3(SpawnPosition);
            buf.WriteQuaternion(SpawnRotation);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            CharacterId = buf.ReadInt();
            ForPlayerId = buf.ReadInt();
            SpawnPosition = buf.ReadVector3();
            SpawnRotation = buf.ReadQuaternion();
        }
    }

    public struct LazerFireMsg
    {
        public int AtTick;
        public int FiringPlayerId;
        public Quaternion FiringRotation;
        public int HitCharacterId;

        public void Serialize(SerializationBuffer buf)
        {
            buf.SeekZero();
            buf.WriteInt((int)NetworkMessageID.LazerHit);

            buf.WriteInt(AtTick);
            buf.WriteInt(FiringPlayerId);
            buf.WriteQuaternion(FiringRotation);
            buf.WriteInt(HitCharacterId);
        }

        public void Deserialize(SerializationBuffer buf)
        {
            AtTick = buf.ReadInt();
            FiringPlayerId = buf.ReadInt();
            FiringRotation = buf.ReadQuaternion();
            HitCharacterId = buf.ReadInt();
        }
    }
}