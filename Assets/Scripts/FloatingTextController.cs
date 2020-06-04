using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
	public FloatingText popupTextPrefab;
	public Canvas canvas;
	private Camera cam;

    public Transform moneyParent;

	Vector3 offset;

	private void Start()
	{
		cam = GetComponentInParent<PlayerInput>().cam;
		offset = new Vector3();
	}


	public void CreateFloatingText(string text, Transform _location)
	{
		Transform location = _location;
		FloatingText instance = Instantiate(popupTextPrefab);
        instance.isMoney = false;
        instance.SetText(text);
        instance.target = location;
		instance.cam = cam;

		Vector3 worldPos = location.position;
		Vector3 screenPosition = cam.WorldToScreenPoint(worldPos);

		instance.transform.SetParent(canvas.transform, false);
		instance.transform.position = screenPosition;
	}

    public void CreateMoneyText(string text, Transform _location)
    {
        Transform location = _location;
        FloatingText instance = Instantiate(popupTextPrefab);
        instance.isMoney = true;
        instance.SetText(text);
        instance.target = location;
        instance.cam = cam;
        instance.transform.SetParent(moneyParent, false);
        
    }


}
