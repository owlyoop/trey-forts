using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

	public PlayerStats player;

	public int OwnerID;
	

	public float speed;
	public float life;
	public int baseDamage;
	public Damager.DamageTypes damageType;
}
