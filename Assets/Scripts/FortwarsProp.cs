using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FortwarsProp : MonoBehaviour, IDamagable
{

	public int maxHealth = 500;
	
	public int currentHealth;

	public int currencyCost = 10;

	public int idOfOwner;

	public PlayerStats player;
	public int team;
	
	public Transform snapPositionBottom;
	public Transform snapPositionLeft;
	public Transform snapPositionRight;
	public Transform snapPositionTop;
	public Material defaultMat;
	public Material placementModeMat;


	public Collider col;
	public Renderer rend;



	private void Start()
	{
		currentHealth = maxHealth;
		
	}

	public void OnDeath()
	{
		player.PropsOwnedByPlayer.Remove(this.gameObject);
		Destroy(gameObject);
	}

	public void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType)
	{
		currentHealth = currentHealth - damageTaken;
		PhotonView.Find(GiverPunID).GetComponent<PlayerStats>().dmgText.CreateFloatingText(damageTaken.ToString(), this.transform);
		if (currentHealth <= 0)
		{
			currentHealth = 0;
			OnDeath();
		}
	}



}
