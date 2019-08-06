using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public static class GameStatics
    {
        public static GameData GameData;
        public static OnlineSession OnlineSession;
        public static MyGameManager GameManager;

        public static void CreateOnlineSession()
        {
            if (OnlineSession)
            {
                GameObject.Destroy(OnlineSession.gameObject);
            }

            OnlineSession = GameObject.Instantiate<OnlineSession>(GameData.OnlineSessionPrefab);
        }
    }
}