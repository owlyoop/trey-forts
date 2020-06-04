using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    public UIManager ui;

    public Image Indicator;

    public Vector3 damageSourcePosition;

    public float MinAlpha = 0.05f;
    public float MaxAlpha = 0.8f;
    public float Lifetime = 0.5f;

    private void Start()
    {
        Invoke("Kill", Lifetime);
    }

    private void Update()
    {
        var targetLocal = ui.playerInput.cam.transform.InverseTransformPoint(damageSourcePosition);
        var targetAngle = -Mathf.Atan2(targetLocal.x, targetLocal.z) * Mathf.Rad2Deg + 180f;
        Indicator.rectTransform.eulerAngles = new Vector3(0, 0, targetAngle);
    }


    void Kill()
    {
        Destroy(this.gameObject);
    }
}
