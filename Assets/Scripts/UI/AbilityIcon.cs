using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour
{
    public AbilityData baseAbility;
    public Image icon;
    public Text slot;

    float cooldown;
    bool isOnCooldown = false;
    float _cooldownStart;
    float _cooldownEnd;
    float _cooldownLength;

    private void Start()
    {
        icon = GetComponent<Image>();
        slot = GetComponentInChildren<Text>();
        _cooldownLength = baseAbility.Cooldown;
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            icon.fillAmount = (Time.time - _cooldownStart) / _cooldownLength;
        }
        if (isOnCooldown && Time.time > _cooldownStart + _cooldownLength)
        {
            StopRadialCooldown();
        }
    }

    public void InitializeValues()
    {
        icon.sprite = baseAbility.icon;
        cooldown = baseAbility.Cooldown;
        if (baseAbility.AbilitySlot == 0)
        {
            slot.text = "Q";
        }
    }

    public void StartRadialCooldown()
    {
        isOnCooldown = true;
        _cooldownStart = Time.time;
        _cooldownEnd = _cooldownStart + _cooldownLength;
    }

    public void StopRadialCooldown()
    {
        isOnCooldown = false;
    }
}
