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


    public bool isMoney = false;

	private void Start()
	{
        
		Destroy(gameObject, 1f);
        
		offset = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        
    }


	public void SetText(string text)
	{
        anim.SetBool("IsMoney", isMoney);
        damageText.text = text;
        if (isMoney)
        {
            offset = Vector3.zero;
        }
    }


	private void Update()
	{
		if (target != null && !isMoney)
		{
			Vector3 worldPos = target.position + new Vector3(0, 1f, 0);
			Vector3 screenPosition = cam.WorldToScreenPoint(worldPos);
			transform.position = screenPosition + offset;
		}
	}
}
