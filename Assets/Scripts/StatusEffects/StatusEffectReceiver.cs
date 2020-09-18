using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectReceiver : MonoBehaviour
{

	public PlayerStats player;

	public List<StatusEffect> CurrentStatusEffectsOnPlayer;

    public int BarbedWiresTouchingPlayer = 0;

	private void Start()
	{
		player = GetComponentInParent<PlayerStats>();
	}

	private void Update()
	{
	}

	public void AddPassiveStatusEffects()
	{
		for (int i = 0; i < player.currentClass.PassiveEffects.Count; i++)
		{
			StatusEffect s = Instantiate(player.currentClass.PassiveEffects[i], this.transform);
			s.receiver = player;
			CurrentStatusEffectsOnPlayer.Add(s);
		}
	}

	public void AddStatusEffect(StatusEffect effect)
	{
        bool alreadyAffected = false;
        StatusEffect existingEffect = null;
        foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
        {
            if (s.GetType() == effect.GetType())
            {
                existingEffect = s;
                Debug.Log("both are same type");
                alreadyAffected = true;
            }
        }

        //target doesnt have this status effect on them.
        if (!alreadyAffected)
        {
            StatusEffect s = Instantiate(effect, this.transform);
            s.receiver = player;
            CurrentStatusEffectsOnPlayer.Add(s);
            s.CurrentStacks = 1;
            s.OnApplyStatusEffect();
            s.ActiveIcon = s.receiver.ui.AddStatusEffectIcon(s);
            s.receiver.ui.UpdateStatusEffectIcon(s);
            existingEffect = s;
        }

        if (existingEffect != null)
        {
            if (effect.AllowStacking)
            {
                existingEffect.CurrentStacks++;
                existingEffect.receiver.ui.UpdateStatusEffectIcon(existingEffect);
            }
            else
            {

            }

        }
	}

	public void RemoveStatusEffect(StatusEffect effect)
	{
        int index = 0;
        bool foundEffect = false;
        for (int i = 0; i < CurrentStatusEffectsOnPlayer.Count; i++)
        {
            if (CurrentStatusEffectsOnPlayer[i] == effect)
            {
                foundEffect = true;
                index = i;
            }
        }

        if (foundEffect)
        {
            if (effect.ActiveIcon != null)
            {
                player.ui.RemoveStatusEffectIcon(effect);
            }

            CurrentStatusEffectsOnPlayer[index].OnUnapplyStatusEffect();
            Destroy(CurrentStatusEffectsOnPlayer[index].gameObject);
            CurrentStatusEffectsOnPlayer.RemoveAt(index);
        }
	}

	public void ClearAllStatusEffects()
	{
        foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
        {
            s.OnUnapplyStatusEffect();
            Destroy(s.ActiveIcon);
            Destroy(s.gameObject);
        }

        CurrentStatusEffectsOnPlayer.Clear();
	}

	public int OnBeforeDamageTaken(int damage)
	{
		int newDamage = damage;
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				newDamage = s.OnBeforeDamageTaken(newDamage);
			}
		}
		return newDamage;
	}

	public void OnAfterDamageTaken()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnAfterDamageTaken();
			}
		}
	}

	public void OnPlayerIsMoving()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				//s.OnPlayerIsMoving();
			}
		}
	}

	public void OnPlayerJump()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnPlayerJump();
				Debug.Log("s");
			}
		}
	}

	public void OnPlayerFiresWeapon()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnPlayerFiresWeapon();
			}
		}
	}

	public void OnPlayerGetsKill()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnPlayerGetsKill();
			}
		}
	}

	public void OnPlayerDeath()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnPlayerDeath();
			}
		}
	}

	public void OnPlayerTakesFatalDamage()
	{
		if (CurrentStatusEffectsOnPlayer.Count > 0)
		{
			foreach (StatusEffect s in CurrentStatusEffectsOnPlayer)
			{
				s.OnPlayerTakesFatalDamage();
			}
		}
	}
}
