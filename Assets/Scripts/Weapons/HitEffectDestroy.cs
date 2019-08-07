using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectDestroy : MonoBehaviour
{
	private void Awake()
	{
		Invoke("Kill", 5);
	}

	void Kill()
	{
		Destroy(gameObject);
	}
}
