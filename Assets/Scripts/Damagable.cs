using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
	void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation, PlayerStats.DamageIndicatorType uiType);

	void OnDeath();
}
