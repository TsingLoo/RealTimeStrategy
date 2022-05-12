using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UnitSpawner : NetworkBehaviour, IPointerClickHandler 
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefeb = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private RTSPlayer player;
    
    private void Update()
    {
        if(isServer)
        {
            ProduceUnits();
        }

        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }



    #region  Server

    [Server]
    private void ProduceUnits()
    {
        if(queuedUnits == 0){return;}

        unitTimer += Time.deltaTime;    

        if(unitTimer<unitSpawnDuration){return;}

        GameObject unitInstance = Instantiate(unitPrefeb.gameObject,unitSpawnPoint.position,unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance,connectionToClient);
        
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }
        
    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
     private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit() //this code run on the server initiate a object on the server and then the mirror asign the owner
    {
        if(queuedUnits == maxUnitQueue){return;}

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        
        if(player.GetResources() < unitPrefeb.GetResourceCost()){return;}

        queuedUnits++;
    
        player.SetResources(player.GetResources() - unitPrefeb.GetResourceCost());
    
    }

    #endregion

    #region Client
    //call this method when you click on any objects that this script attached to
    public void OnPointerClick(PointerEventData eventData)// unity will call this function for me whenever I click on this gameobject
    {
        if(eventData.button != PointerEventData.InputButton.Left){return;}
        
        //this object is owned not by this client 
        if(!hasAuthority){return;}

        CmdSpawnUnit();

    }

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;
        
        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount,newProgress,ref progressImageVelocity,0.1f);
           
        }
    }


    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }


    #endregion
}
