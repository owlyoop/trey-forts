using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentAbilitiesUI : MonoBehaviour
{

    public GameObject grid;
    public GameObject abilityIconPrefab;

    public List<GameObject> abilityList;

    public UIManager mainUI;

    public void AddSlots()
    {
        if (abilityList.Count > 0)
        {
            for (int i = 0; i < abilityList.Count; i++)
            {
                Destroy(abilityList[i].gameObject);
            }

            abilityList.Clear();
        }

        for (int i = 0; i < mainUI.player.CurrentClass.abilityList.Count; i++)
        {
            GameObject slot = Instantiate(abilityIconPrefab, grid.transform);
            var ab = slot.GetComponent<AbilityIcon>();
            ab.baseAbility = mainUI.player.CurrentClass.abilityList[i];
            ab.InitializeValues();
            abilityList.Add(ab.gameObject);
        }
    }

    public void StartAbilityCooldown(string name)
    {
        for (int i = 0; i < abilityList.Count; i++)
        {
            if (name == abilityList[i].GetComponent<AbilityIcon>().baseAbility.AbilityName)
            {
                abilityList[i].GetComponent<AbilityIcon>().StartRadialCooldown();
            }
        }
    }

}
