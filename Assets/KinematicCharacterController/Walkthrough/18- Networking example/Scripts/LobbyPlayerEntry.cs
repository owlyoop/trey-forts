using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerEntry : MonoBehaviour
{
    public Text NameText;

    public int EntryId { get; private set; }

    public void Init(string name, int id)
    {
        EntryId = id;
        NameText.text = name;
    }
}
