using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialReload : MonoBehaviour
{

    public bool isReloading;

    public float _reloadEndTime;
    public float _reloadStartTime;
    public float _reloadTime;

    public Image radialImage;
    

    private void Start()
    {
        
    }

    private void Update()
    {
        if (isReloading )
        {
            radialImage.fillAmount = (Time.time - _reloadStartTime) / (_reloadTime);
        }

        if (isReloading && Time.time > _reloadStartTime + _reloadTime)
        {
            //reload finished

            _reloadStartTime = Time.time;
        }
        
    }

    public void StartReload(float reloadTime)
    {
        radialImage.enabled = true;
        _reloadTime = reloadTime;
        _reloadStartTime = Time.time;
        _reloadEndTime = _reloadStartTime + reloadTime;
        isReloading = true;

    }

    public void StopReload()
    {
        radialImage.enabled = false;
        isReloading = false;
    }
}
