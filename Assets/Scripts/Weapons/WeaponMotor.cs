using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponMotor : MonoBehaviourPunCallbacks
{
	
	public GameObject networkObjToSpawn;

	public virtual void UpdateUI() { }

	public virtual void PrimaryFire() {  }
	public virtual void PrimaryFireHolding() { }
	public virtual void PrimaryFireButtonUp() { }

	public virtual void SecondaryFire() { }
	public virtual void SecondaryFireHolding() { }

	public virtual void ScrollWheelUp() { }
	public virtual void ScrollWheelDown() { }

	public virtual void OnSwitchAwayFromWeapon() { }
	public virtual void OnSwitchToWeapon() { }

	public virtual void ReloadButton() { }

	public virtual void UseButtonHolding() { }
	public virtual void UseButtonUp() { }

	public virtual void GetWeaponStats(Weapon wep) { }
	public virtual void GetWeaponStats(RangedProjectile wep) { }

}
