using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Examples;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    [CreateAssetMenu]
    public class GameData : ScriptableObject
    {
        [Header("Scenes")]
        public string LobbySceneName;
        public string GameSceneName;

        [Header("Prefab")]
        public OnlineSession OnlineSessionPrefab;
        public MyPlayerController PlayerPrefab;
        public OrbitCamera CameraPrefab;
        public MyCharacterController CharacterPrefab;
    }
}