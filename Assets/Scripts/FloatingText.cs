using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{

	public Animator anim;
	public Text damageText;
	public Transform target;
	public Camera cam;
	Vector3 offset;

	private void Start()
	{
		AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
		Destroy(gameObject, clipInfo[0].clip.length);
		offset = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
	}


	public void SetText(string text)
	{
		damageText.text = text;
	}


	private void Update()
	{
		if (target != null)
		{
			Vector3 worldPos = target.position + new Vector3(0, 1f, 0);
			Vector3 screenPosition = cam.WorldToScreenPoint(worldPos);
			transform.position = screenPosition + offset;
		}

	}

}
