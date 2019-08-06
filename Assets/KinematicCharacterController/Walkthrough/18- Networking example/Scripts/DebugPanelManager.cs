using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public class DebugPanelManager : MonoBehaviour
    {
        [Header("References")]
        public MyGameManager MyGameManager;
        public MyCharacterController DebuggedCharacter;

        [Header("Properties")]
        public int DisplayedTickInterval = 100;

        [Header("UI")]
        public GameObject PanelRoot;
        public Text TickText;
        public Text CharacterIdText;

        private void Awake()
        {
            PanelRoot.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PanelRoot.SetActive(!PanelRoot.activeSelf);
            }

            if(PanelRoot.activeInHierarchy)
            {
                HandleDebugInfos();
            }
        }

        private void HandleDebugInfos()
        {
            HandleGeneralInfos();
            HandleCharacterInfos();
        }

        private void HandleGeneralInfos()
        {
            if(MyGameManager.SimulationSystem.PresentTick % DisplayedTickInterval == 0)
            {
                TickText.text = MyGameManager.SimulationSystem.PresentTick.ToString();
            }
        }

        private void HandleCharacterInfos()
        {
            if(DebuggedCharacter)
            {
                CharacterIdText.text = DebuggedCharacter.GetId().ToString();
            }
            else
            {
                CharacterIdText.text = "NA";
            }
        }
    }
}