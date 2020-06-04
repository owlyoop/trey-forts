using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamworksManager : MonoBehaviour
{



    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        var playername = SteamClient.Name;
        var playersteamid = SteamClient.SteamId;
    }

    private void Awake()
    {
        try
        {
            Steamworks.SteamClient.Init(211);
        }
        catch (System.Exception e)
        {
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
            //
        }

        var serverInit = new SteamServerInit("treyforts", "Trey Mode")
        {
            GamePort = 28015,
            Secure = true,
            QueryPort = 28016
        };

        try
        {
            Steamworks.SteamServer.Init(211, serverInit);
        }
        catch (System.Exception)
        {
            // Couldn't init for some reason (dll errors, blocked ports)
        }
    }

    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }

    public void GetSteamFriendsList()
    {

    }

    private void OnDisable()
    {
        Steamworks.SteamClient.Shutdown();
    }
}
