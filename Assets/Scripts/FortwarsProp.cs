using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FortwarsProp : NetworkBehaviour, IDamagable
{

	public int maxHealth = 500;

	[SyncVar]
	public int currentHealth;

	public int currencyCost = 10;
	
	public Transform snapPositionBottom;
	public Transform snapPositionLeft;
	public Transform snapPositionRight;
	public Transform snapPositionTop;
	public Material defaultMat;
	public Material placementModeMat;

	public NetworkInstanceId nid;


	private void Start()
	{
		currentHealth = maxHealth;
		nid = this.GetComponent<NetworkIdentity>().netId;
	}

	public void OnDeath()
	{
		Destroy(gameObject);
	}

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType)
	{
		if (!isServer)
		{
			return;
		}

		currentHealth = currentHealth - damageTaken;
		if (currentHealth <= 0)
		{
			currentHealth = 0;
			OnDeath();
		}
	}



}
