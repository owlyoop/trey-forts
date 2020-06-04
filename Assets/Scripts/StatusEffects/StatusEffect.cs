using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class StatusEffect : MonoBehaviour
{

    [Header("UI")]
    public Sprite icon;
    public bool IsUrgentEffect;
    public string UrgentText;
    public StatusEffectIcon ActiveIcon;


	public PlayerStats giver;
	public PlayerStats receiver;

    public bool AllowStacking;
    public bool UsesDuration;
    
    public int CurrentStacks = 0;
    public int MaxStacks = 99;

	public float Duration;


    private void Start()
    {
        
    }
    public virtual void OnApplyStatusEffect() { }
    public virtual void OnUnapplyStatusEffect() { }
    public virtual int OnBeforeDamageTaken(int damage) { return damage; }
    public virtual void OnAfterDamageTaken() { }
    public virtual void OnPlayerDealsDamage() { }
    public virtual void OnPlayerIsMoving(float moveSpeed) { }
    public virtual void OnPlayerJump() { }
    public virtual void OnPlayerFiresWeapon() { }
    public virtual void OnPlayerGetsKill() { }
    public virtual void OnPlayerDeath() { }
    public virtual void OnPlayerTakesFatalDamage() { }
    public virtual void OnAddStack() { }
    public virtual void OnRemoveStack() { }
}
