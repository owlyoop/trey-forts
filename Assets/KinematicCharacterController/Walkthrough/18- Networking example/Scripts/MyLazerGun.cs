using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public class MyLazerGun : MonoBehaviour
    {
        public MyCharacterController OwningCharacter;
        public GameObject LazerBeamPrefab;
        public GameObject ImpactPrefab;

        public void Fire(Quaternion rotation)
        {
            // Lazer beam
            Destroy(Instantiate(LazerBeamPrefab, transform.position, rotation), 0.2f);

            // Check for hit
            RaycastHit hit;
            MyCharacterController hitCC = null;
            if (Physics.Raycast(transform.position, rotation * Vector3.forward, out hit, 100, -1, QueryTriggerInteraction.Ignore))
            {
                // Spawn visual feedback
                Destroy(Instantiate(ImpactPrefab, hit.point, Quaternion.identity), 5f);
                
                // Authoritative logic
                if (!GameStatics.OnlineSession.IsClient())
                {
                    Rigidbody hitR = hit.collider.attachedRigidbody;
                    if (hitR)
                    {
                        hitCC = hitR.GetComponent<MyCharacterController>();
                        if (hitCC)
                        {
                            hitCC.OnHitFX();
                            GameStatics.GameManager.SimulationSystem.AddSimulationEvent(new LazerHitEvent(hitCC), GameStatics.GameManager.SimulationSystem.SimulationTick);
                        }
                    }
                }
            }

            if (GameStatics.OnlineSession.IsServer())
            {
                // Send lazer msg event
                LazerFireMsg lhm = new LazerFireMsg();
                lhm.AtTick = GameStatics.GameManager.SimulationSystem.SimulationTick;
                lhm.FiringPlayerId = OwningCharacter.OwningPlayer.GetId();
                lhm.FiringRotation = rotation;
                lhm.HitCharacterId = (hitCC == null) ? -1 : hitCC.GetId();
                lhm.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);
            }
        }
    }
}