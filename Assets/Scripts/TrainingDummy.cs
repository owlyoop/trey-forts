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

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation, PlayerStats.DamageIndicatorType uiType)
	{
        Debug.Log(giver.ToString());
		giver.dmgText.CreateFloatingText(damageTaken.ToString(), this.transform);
		CurrentHealth = CurrentHealth - damageTaken;
		if (CurrentHealth <= 0)
		{
			CurrentHealth = 0;
		}
	}

}
