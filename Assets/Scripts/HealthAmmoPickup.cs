using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthAmmoPickup : MonoBehaviour
{
    const int SMALL_HEALTH_PERCENT = 20;
    const int MEDIUM_HEALTH_PERCENT = 50;
    const int FULL_HEALTH_PERCENT = 100;

    const int SMALL_AMMO_PERCENT = 20;
    const int MEDIUM_AMMO_PERCENT = 50;
    const int FULL_AMMO_PERCENT = 100;

    public enum PickupType { HealthSmall, HealthMedium, HealthFull, AmmoSmall, AmmoMedium, AmmoFull}

    public PickupType TypeOfPickup;

    public float rotationSpeed = 90f; // degrees per second

    float PickupPercentAdd = 50;
    public float RespawnTime = 10f; // Time in seconds
    [SerializeField]
    bool isRespawning = false;
    [SerializeField]
    float nextSpawnTime = 0f;

    int sentinel = 0;

    [Header("References")]
    public GameObject mesh;
    public Collider trigger;

    private void Start()
    {
        switch(TypeOfPickup)
        {
            case PickupType.HealthSmall:
                {
                    PickupPercentAdd = SMALL_HEALTH_PERCENT;
                    break;
                }
            case PickupType.HealthMedium:
                {
                    PickupPercentAdd = MEDIUM_HEALTH_PERCENT;
                    break;
                }
            case PickupType.HealthFull:
                {
                    PickupPercentAdd = FULL_HEALTH_PERCENT;
                    break;
                }
            case PickupType.AmmoSmall:
                {
                    PickupPercentAdd = SMALL_AMMO_PERCENT;
                    break;
                }
            case PickupType.AmmoMedium:
                {
                    PickupPercentAdd = MEDIUM_AMMO_PERCENT;
                    break;
                }
            case PickupType.AmmoFull:
                {
                    PickupPercentAdd = FULL_AMMO_PERCENT;
                    break;
                }
            default:
                {
                    PickupPercentAdd = 0;
                    Debug.Log("Pickuptype does not have an assigned value");
                    break;
                }
        }
    }

    private void Update()
    {
        mesh.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);

        if (isRespawning)
        {
            if (nextSpawnTime < Time.time)
            {
                trigger.enabled = true;
                mesh.SetActive(true);
                isRespawning = false;
                sentinel = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool validPickup = false;
        if (sentinel == 0)
        {
            var player = other.GetComponent<PlayerStats>();
            if (player != null && player.CurrentClass.className != "Spectator")
            {
                if (TypeOfPickup == PickupType.AmmoFull || TypeOfPickup == PickupType.AmmoMedium || TypeOfPickup == PickupType.AmmoSmall)
                {
                    foreach (GameObject gb in player.wepSlots.weaponSlots)
                    {
                        var weapon = gb.GetComponent<WeaponMotor>();
                        float ammoAdd = (PickupPercentAdd / 100) * weapon.MaxAmmo;
                        if ((int)ammoAdd + weapon.CurrentAmmo > weapon.MaxAmmo)
                        {
                            weapon.CurrentAmmo = weapon.MaxAmmo;
                        }
                        else
                        {
                            weapon.CurrentAmmo += (int)ammoAdd;
                        }
                        Debug.Log(weapon.CurrentAmmo);
                    }
                    validPickup = true;
                    player.wepSlots.CurrentWeapon.UpdateUI();
                    sentinel++;
                }
                else
                {
                    if (player.CurrentHealth < player.maxHealth.Value)
                    {
                        player.OnHealthPickup(PickupPercentAdd);
                        validPickup = true;
                        sentinel++;
                    }
                }

                if (validPickup)
                {
                    Debug.Log("valid pickup");
                    isRespawning = true;
                    trigger.enabled = false;
                    mesh.SetActive(false);
                    nextSpawnTime = Time.time + RespawnTime;
                    validPickup = false;
                    sentinel++;
                }

            }
        }
    }
}
