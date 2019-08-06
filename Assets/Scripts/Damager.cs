using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{

	public enum DamageTypes { Physical, Fire, Frost }


	public virtual void DamageTarget(IDamagable target)
	{

	}

}