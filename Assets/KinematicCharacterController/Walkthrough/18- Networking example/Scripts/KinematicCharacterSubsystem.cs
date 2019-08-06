using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public struct KinematicCharacterSubsystemState
    {
        public List<IdentifiedData<MyCharacterState>> CharacterStates;
        public MyMovingPlatformState[] MovingPlatformStates;

        public void ReadFromBuffer(SerializationBuffer buf)
        {
            int CharacterStatesCount = buf.ReadInt();
            for (int i = 0; i < CharacterStatesCount; i++)
            {
                int characterId = buf.ReadInt();

                Vector3 position = buf.ReadVector3();
                Quaternion rotation = buf.ReadQuaternion();
                Vector3 velocity = buf.ReadVector3();
                bool isStunned = buf.ReadBool();

                // Find CharacterState corresponding to this ID and apply the state
                for (int j = 0; j < CharacterStates.Count; j++)
                {
                    if (CharacterStates[j].DataId == characterId)
                    {
                        IdentifiedData<MyCharacterState> tmpIdCharState = CharacterStates[j];

                        tmpIdCharState.DataStruct.MotorState.Position = position;
                        tmpIdCharState.DataStruct.MotorState.Rotation = rotation;
                        tmpIdCharState.DataStruct.MotorState.BaseVelocity = velocity;

                        tmpIdCharState.DataStruct.IsStunned = isStunned;

                        CharacterStates[j] = tmpIdCharState;
                        break;
                    }
                }
            }
        }

        public void WriteToBuffer(SerializationBuffer buf, bool mustBeFullState)
        {
            int characterStatesCount = CharacterStates.Count;
            buf.WriteInt(characterStatesCount);
            for (int i = 0; i < characterStatesCount; i++)
            {
                buf.WriteInt(CharacterStates[i].DataId);

                buf.WriteVector3(CharacterStates[i].DataStruct.MotorState.Position);
                buf.WriteQuaternion(CharacterStates[i].DataStruct.MotorState.Rotation);
                buf.WriteVector3(CharacterStates[i].DataStruct.MotorState.BaseVelocity);

                buf.WriteBool(CharacterStates[i].DataStruct.IsStunned);
            }
        }
    }

    public class KinematicCharacterSubsystem : MonoBehaviour
    {
        public bool Interpolation = true;
        public float InterpolationPhase = 0.04f;

        public MyMovingPlatform[] MovingPlatforms;
        public Dictionary<int, MyCharacterController> Characters;

        private int _idCounter = -1;

        public void Initialize(int maxCharacters)
        {
            // Set up for manual control over simulation
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.AutoSimulation = false;
            KinematicCharacterSystem.InterpolationMethod = CharacterSystemInterpolationMethod.None;

            Characters = new Dictionary<int, MyCharacterController>(maxCharacters);
        }

        private void Update()
        {
            UpdateInterpolation();
        }

        private void UpdateInterpolation()
        {
            // Handle interpolation
            if (Interpolation)
            {
                foreach (var c in Characters.Values)
                {
                    float interpFactor = (Time.time - c.FromInterpolationPoint.Time) / (c.ToInterpolationPoint.Time - c.FromInterpolationPoint.Time);
                    if (!float.IsNaN(interpFactor))
                    {
                        Vector3 interpPos = Vector3.Lerp(c.FromInterpolationPoint.Position, c.ToInterpolationPoint.Position, interpFactor);
                        Quaternion interpRot = Quaternion.Slerp(c.FromInterpolationPoint.Rotation, c.ToInterpolationPoint.Rotation, interpFactor);

                        c.MeshRoot.SetPositionAndRotation(interpPos, interpRot);
                    }
                }

                foreach (var m in MovingPlatforms)
                {
                    float interpFactor = (Time.time - m.FromInterpolationPoint.Time) / (m.ToInterpolationPoint.Time - m.FromInterpolationPoint.Time);
                    if (!float.IsNaN(interpFactor))
                    {
                        Vector3 interpPos = Vector3.Lerp(m.FromInterpolationPoint.Position, m.ToInterpolationPoint.Position, interpFactor);
                        Quaternion interpRot = Quaternion.Slerp(m.FromInterpolationPoint.Rotation, m.ToInterpolationPoint.Rotation, interpFactor);

                        m.Mover.Transform.SetPositionAndRotation(interpPos, interpRot);
                    }
                }
            }
        }

        public int RegisterCharacter(MyCharacterController character, bool hasOwnership)
        {
            _idCounter++;

            RegisterCharacterAtId(character, _idCounter, hasOwnership);

            return _idCounter;
        }

        public void RegisterCharacterAtId(MyCharacterController character, int id, bool hasOwnership)
        {
            Characters[id] = character;
            character.SetId(id);
            character.SetHasOwnership(hasOwnership);
        }

        public void UnregisterCharacter(int id)
        {
            MyCharacterController c;
            if(Characters.TryGetValue(id, out c))
            {
                Characters.Remove(id);
            }
        }

        public void OnPreTick(float deltaTime)
        {
            if (Interpolation)
            {
                foreach (var c in Characters.Values)
                {
                    if (c.GetHasOwnership())
                    {
                        c.MeshRoot.SetPositionAndRotation(c.ToInterpolationPoint.Position, c.ToInterpolationPoint.Rotation);

                        c.FromInterpolationPoint = new InterpolationPoint(Time.time, c.MeshRoot.position, c.MeshRoot.rotation);
                    }
                }

                foreach (var m in MovingPlatforms)
                {
                    m.Mover.Transform.SetPositionAndRotation(m.ToInterpolationPoint.Position, m.ToInterpolationPoint.Rotation);

                    m.FromInterpolationPoint = new InterpolationPoint(Time.time, m.Mover.Transform.position, m.Mover.Transform.rotation);
                }
            }
        }

        public void OnSimulateStep(float deltaTime)
        {
            KinematicCharacterSystem.Simulate(deltaTime);
        }

        public void OnPostTick(float deltaTime)
        {
            // Handle interpolation of local characters
            if (Interpolation)
            {
                foreach (var c in Characters.Values)
                {
                    if (c.GetHasOwnership())
                    {
                        c.ToInterpolationPoint = new InterpolationPoint(Time.time + Time.fixedDeltaTime, c.Motor.TransientPosition, c.Motor.TransientRotation);
                    }
                }

                foreach (var m in MovingPlatforms)
                {
                    m.ToInterpolationPoint = new InterpolationPoint(Time.time + Time.fixedDeltaTime, m.Mover.TransientPosition, m.Mover.TransientRotation);
                }
            }
        }

        public void HandlePreSnapshotInterpolation()
        {
            if (Interpolation)
            {
                foreach (var c in Characters.Values)
                {
                    if (!c.GetHasOwnership())
                    {
                        c.FromInterpolationPoint = new InterpolationPoint(Time.time, c.MeshRoot.position, c.MeshRoot.rotation);
                    }
                }
            }
        }

        public void HandlePostSnapshotInterpolation()
        {
            if (Interpolation)
            {
                foreach (var c in Characters.Values)
                {
                    if (!c.GetHasOwnership())
                    {
                        c.ToInterpolationPoint = new InterpolationPoint(Time.time + InterpolationPhase, c.Motor.TransientPosition, c.Motor.TransientRotation);
                    }
                }
            }
        }

        public void InitializeSnapshot(ref KinematicCharacterSubsystemState subsystemState, int maxCharacters)
        {
            subsystemState.CharacterStates = new List<IdentifiedData<MyCharacterState>>(maxCharacters);
            subsystemState.MovingPlatformStates = new MyMovingPlatformState[MovingPlatforms.Length];
        }

        public void SaveToSnapshot(ref KinematicCharacterSubsystemState subsystemState)
        {
            subsystemState.CharacterStates.Clear();
            foreach (var c in Characters)
            {
                IdentifiedData<MyCharacterState> charState = new IdentifiedData<MyCharacterState>();

                charState.DataId = c.Key;

                charState.DataStruct.MotorState = c.Value.Motor.GetState();

                charState.DataStruct.JumpConsumed = c.Value.JumpConsumed;
                charState.DataStruct.TimeSinceJumpRequested = c.Value.TimeSinceJumpRequested;
                charState.DataStruct.TimeSinceLastAbleToJump = c.Value.TimeSinceLastAbleToJump;

                charState.DataStruct.IsStunned = c.Value.IsStunned;
                charState.DataStruct.StunTimeRemaining = c.Value.StunTimeRemaining;

                subsystemState.CharacterStates.Add(charState);
            }

            for (int i = 0; i < MovingPlatforms.Length; i++)
            {
                subsystemState.MovingPlatformStates[i].MoverState = MovingPlatforms[i].Mover.GetState();
                subsystemState.MovingPlatformStates[i].DirectorTime = (float)MovingPlatforms[i].Director.time;
            }
        }

        public void RestoreStateFromSnapshot(ref KinematicCharacterSubsystemState subsystemState)
        {
            foreach (IdentifiedData<MyCharacterState> charState in subsystemState.CharacterStates)
            {
                MyCharacterController c;
                if (Characters.TryGetValue(charState.DataId, out c))
                {
                    c.Motor.ApplyState(charState.DataStruct.MotorState, false);

                    c.JumpConsumed = charState.DataStruct.JumpConsumed;
                    c.TimeSinceJumpRequested = charState.DataStruct.TimeSinceJumpRequested;
                    c.TimeSinceLastAbleToJump = charState.DataStruct.TimeSinceLastAbleToJump;

                    c.IsStunned = charState.DataStruct.IsStunned;
                    c.StunTimeRemaining = charState.DataStruct.StunTimeRemaining;
                }
            }

            for(int i = 0; i < MovingPlatforms.Length; i++)
            {
                MovingPlatforms[i].Mover.ApplyState(subsystemState.MovingPlatformStates[i].MoverState);
                MovingPlatforms[i].EvaluateAtTime(subsystemState.MovingPlatformStates[i].DirectorTime);
            }
        } 
    }
}