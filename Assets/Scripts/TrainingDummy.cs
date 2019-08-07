using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamagable
{

	public int MaxHealth = 500;
	public int CurrentHealth;


	void Start()
	{
		CurrentHealth = MaxHealth;
	}

	public void OnDeath()
	{

	}

	public void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType)
	{
		Debug.Log(GiverPunID);
		Debug.Log(PhotonView.Find(GiverPunID));
		PhotonView.Find(GiverPunID).GetComponent<PlayerStats>().dmgText.CreateFloatingText(damageTaken.ToString(), this.transform);
		CurrentHealth = CurrentHealth - damageTaken;
		if (CurrentHealth <= 0)
		{
			CurrentHealth = 0;
		}
	}
}
