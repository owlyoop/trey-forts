using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpToHeal : StatusEffect
{

	private void Start()
	{
		
	}

	public override void OnAfterDamageTaken()
	{

	}

	public override void OnApplyStatusEffect()
	{

	}

	public override int OnBeforeDamageTaken(int damage)
	{
		return damage;
	}

	public override void OnPlayerDealsDamage()
	{

	}

	public override void OnPlayerDeath()
	{

	}

	public override void OnPlayerFiresWeapon()
	{

	}

	public override void OnPlayerGetsKill()
	{

	}

	public override void OnPlayerIsMoving(float moveSpeed)
	{

	}

	public override void OnPlayerJump()
	{
		//receiver.TakeDamage(receiver.GetComponent<Photon.Pun.PhotonView>().ViewID, -5, Damager.DamageTypes.Physical);
	}

	public override void OnPlayerTakesFatalDamage()
	{
	}

	public override void OnUnapplyStatusEffect()
	{

	}
}
