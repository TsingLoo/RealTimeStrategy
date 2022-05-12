using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;


public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameOverHandler gameOverHandlerPrefeb = null;
    [SerializeField] private GameObject unitBasePrefeb = null;

    private bool useLan = false;   

    public void switchConnectionType()
    {
       useLan = !useLan;
       if(useLan)
       {
           //gameObject.GetComponent<NetworkManager>().
       }
        
    }

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;

    public List<RTSPlayer> Players {get;} = new List<RTSPlayer>();

    #region Server
    
    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress){return;}
        conn.Disconnect();
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        if(Players.Count < 2){return;}
        isGameInProgress = true; // no one else can connect now 

        ServerChangeScene("Scene_Map_01");

        
    }

    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f)));
        //GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefeb,conn.identity.transform.position,conn.identity.transform.rotation);//instantiate where is player is instantiate
        //NetworkServer.Spawn(unitSpawnerInstance,conn);

        player.SetPartyOwner(Players.Count == 1);

    }

    #endregion 

    #region Client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion

    public override void OnServerSceneChanged(string sceneName)//after the scene is changed
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))// if this new scene a Map? if so, then
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefeb);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefeb, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance,player.connectionToClient);

                NetworkServer.Spawn(baseInstance,player.connectionToClient);

            }
        }

    }
}
