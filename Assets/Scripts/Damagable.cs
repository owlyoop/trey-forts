using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
	void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType);

	void OnDeath();
}
