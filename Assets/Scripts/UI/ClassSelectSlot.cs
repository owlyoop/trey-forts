using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectSlot : MonoBehaviour
{

	public Text buttonText;
	public Image icon;

    public bool isElite;
    public Text cost;
    public Text dollarSign;
    public Text numberOfLives;

	public WeaponSet weaponSet;
	public ClassSelectMenu menu;


	private void Start()
	{
		menu = GetComponentInParent<GridReference>().menu;
		buttonText.text = weaponSet.className;
        if (!isElite)
        {
            cost.enabled = false;
            dollarSign.enabled = false;
        }
	}

    public void UpdateLivesText()
    {
        numberOfLives.text = menu.player.eliteClassLives[weaponSet.className].ToString();
    }

	public void OnClickSlot()
	{
		//fix
		menu.selectedClassName.text = weaponSet.className;
		menu.selectedClass = weaponSet;

        if (weaponSet.isElite)
        {
            menu.ClassSelectButton.interactable = false;
            bool canSelect = false;
            if (menu.player.currentCurrency >= menu.selectedClass.eliteMoneyCost)
            {
                canSelect = true;
            }
            else
            {
                canSelect = false;
            }

            if (menu.player.gameManager.currentGameState == GamePhases.GameState.BuildPhase || menu.player.gameManager.currentGameState == GamePhases.GameState.WaitingForPlayers)
            {
                canSelect = false;
            }

            if (canSelect)
            {
                menu.EliteClassButton.interactable = true;
            }
            else
            {
                menu.EliteClassButton.interactable = false;
            }

            //check how many lives the player has for the elite class
            if (menu.player.eliteClassLives[weaponSet.className] > 0)
            {
                menu.SpawnAsEliteButton.interactable = true;
            }
            else
            {
                menu.SpawnAsEliteButton.interactable = false;
            }
        }
        else
        {
            menu.ClassSelectButton.interactable = true;
            menu.EliteClassButton.interactable = false;
        }


		//clear
		if (weaponSet.weaponList.Count > 0 && menu.selectedClassWeapons.Count > 0)
		{
			for (int i = 0; i < menu.selectedClassWeapons.Count; i++)
			{
				Destroy(menu.selectedClassWeapons[i]);
			}
		}


		for (int i = 0; i < weaponSet.weaponList.Count; i++)
		{
			GameObject newwep = Instantiate(menu.weaponUIPrefab, menu.wepGrid.transform);
			newwep.GetComponent<WeaponSlotUI>().wep = weaponSet.weaponList[i];
			newwep.GetComponent<WeaponSlotUI>().icon.sprite = weaponSet.weaponList[i].icon;
			newwep.GetComponent<WeaponSlotUI>().weaponName.text = weaponSet.weaponList[i].weaponName;
			menu.selectedClassWeapons.Add(newwep);
		}
	}

}
