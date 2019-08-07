using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
	public PlayerStats giver;
	public PlayerStats receiver;

	public bool isPassive;

	public float duration;

	public abstract void OnApplyStatusEffect();

	public abstract void OnUnapplyStatusEffect();

	public abstract int OnBeforeDamageTaken(int damage);

	public abstract void OnAfterDamageTaken();

	public abstract void OnPlayerDealsDamage();

	public abstract void OnPlayerIsMoving(float moveSpeed);

	public abstract void OnPlayerJump();

	public abstract void OnPlayerFiresWeapon();

	public abstract void OnPlayerGetsKill();

	public abstract void OnPlayerDeath();

	public abstract void OnPlayerTakesFatalDamage();
}
