using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

[Serializable]
public class CharacterStat
{
	public readonly ReadOnlyCollection<StatModifier> StatModifiers;

	public float BaseValue;

	protected bool isDirty = true;
	protected float _value;
	protected float lastBaseValue = float.MinValue;

	private readonly List<StatModifier> statModifiers;

	public CharacterStat()
	{
		statModifiers = new List<StatModifier>();
		StatModifiers = statModifiers.AsReadOnly();
	}

	public CharacterStat(float baseValue) : this()
	{
		BaseValue = baseValue;
	}

	public virtual float Value
	{
		get
		{
			if (isDirty || lastBaseValue != BaseValue)
			{
				lastBaseValue = BaseValue;
				_value = CalculateFinalValue();
				isDirty = false;
			}
			return _value;
		}
	}

	public virtual void AddModifier(StatModifier mod)
	{
		isDirty = true;
		statModifiers.Add(mod);
		statModifiers.Sort(CompareModifierOrder);
	}

	public virtual bool RemoveModifier(StatModifier mod)
	{
		if (statModifiers.Remove(mod))
		{
			isDirty = true;
			return true;
		}
		return false;
	}

	protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
	{
		if (a.Order < b.Order)
			return -1;
		else if (a.Order > b.Order)
			return 1;
		return 0; // if (a.Order == b.Order)
	}

	protected virtual float CalculateFinalValue()
	{
		float finalValue = BaseValue;
		float sumPercentAdd = 0; // This will hold the sum of our "PercentAdd" modifiers

		for (int i = 0; i < statModifiers.Count; i++)
		{
			StatModifier mod = statModifiers[i];

			if (mod.Type == StatModType.Flat)
			{
				finalValue += mod.Value;
			}
			else if (mod.Type == StatModType.PercentAdd) // When we encounter a "PercentAdd" modifier
			{
				sumPercentAdd += mod.Value; // Start adding together all modifiers of this type

				// If we're at the end of the list OR the next modifer isn't of this type
				if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
				{
					finalValue *= 1 + sumPercentAdd; // Multiply the sum with the "finalValue", like we do for "PercentMult" modifiers
					sumPercentAdd = 0; // Reset the sum back to 0
				}
			}
			else if (mod.Type == StatModType.PercentMult) // Percent renamed to PercentMult
			{
				finalValue *= 1 + mod.Value;
			}
		}

		return (float)Math.Round(finalValue, 4);
	}

	public virtual bool RemoveAllModifiersFromSource(object source)
	{
		bool didRemove = false;

		for (int i = statModifiers.Count - 1; i >= 0; i--)
		{
			if (statModifiers[i].Source == source)
			{
				isDirty = true;
				didRemove = true;
				statModifiers.RemoveAt(i);
			}
		}
		return didRemove;
	}
}

public enum StatModType
{
	Flat = 100,
	PercentAdd = 200,
	PercentMult = 300,
}

public class StatModifier
{
	public readonly float Value;
	public readonly StatModType Type;
	public readonly int Order;
	public readonly object Source;

	// "Main" constructor. Requires all variables.
	public StatModifier(float value, StatModType type, int order, object source) // Added "source" input parameter
	{
		Value = value;
		Type = type;
		Order = order;
		Source = source; // Assign Source to our new input parameter
	}

	// Requires Value and Type. Calls the "Main" constructor and sets Order and Source to their default values: (int)type and null, respectively.
	public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }

	// Requires Value, Type and Order. Sets Source to its default value: null
	public StatModifier(float value, StatModType type, int order) : this(value, type, order, null) { }

	// Requires Value, Type and Source. Sets Order to its default value: (int)Type
	public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }
}
