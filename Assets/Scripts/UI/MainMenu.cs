using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button ConnectButton;
    public Button QuitButton;
    public InputField UsernameField;

    public PlayerStats player;

    public void Start()
    {
        
    }

    public void OnClickConnectButton()
    {
        SceneManager.LoadScene("mainphoton");
    }


    public void OnClickQuitButton()
    {

    }

    public void SetUsername()
    {
        player.Username = UsernameField.textComponent.text;
    }
}
