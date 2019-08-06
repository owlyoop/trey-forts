using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{

    public struct BotCharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public class MyBotController : MonoBehaviour
    {
        public float MovementPeriod = 1f;
        public MyCharacterController[] Characters; 

        public void OnSimulate(float deltaTime)
        {
            BotCharacterInputs inputs = new BotCharacterInputs();

            float tickToTime = (float)GameStatics.GameManager.SimulationSystem.SimulationTick / (1f / Time.fixedDeltaTime);

            // Simulate an input on all controlled characters
            inputs.MoveVector = Mathf.Sin(tickToTime * MovementPeriod) * Vector3.forward;
            inputs.LookVector = Vector3.Slerp(-Vector3.forward, Vector3.forward, inputs.MoveVector.z).normalized;
            for (int i = 0; i < Characters.Length; i++)
            {
                Characters[i].SetInputs(ref inputs);
            }
        }
    }
}