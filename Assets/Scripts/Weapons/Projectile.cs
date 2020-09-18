using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Projectile : NetworkBehaviour
{

	public PlayerStats player;

	public int OwnerID;

	public float speed;
	public float life;
	public int baseDamage;
	public Damager.DamageTypes damageType;
}
