using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialReload : MonoBehaviour
{

    public bool isReloading;

    public float reloadEndTime;
    public float reloadStartTime;
    public float reloadTime;

    public Image radialImage;
    

    private void Start()
    {
        
    }

    private void Update()
    {
        if (isReloading )
        {
            radialImage.fillAmount = (Time.time - reloadStartTime) / (reloadTime);
        }

        if (isReloading && Time.time > reloadStartTime + reloadTime)
        {
            //reload finished
            reloadStartTime = Time.time;
        }
        
    }

    public void StartReload(float reloadTime)
    {
        radialImage.enabled = true;
        this.reloadTime = reloadTime;
        reloadStartTime = Time.time;
        reloadEndTime = reloadStartTime + reloadTime;
        isReloading = true;

    }

    public void StopReload()
    {
        radialImage.enabled = false;
        isReloading = false;
    }
}
