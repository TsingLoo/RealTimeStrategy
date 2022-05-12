using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class UnitBase : NetworkBehaviour
{
   [SerializeField] private Health health = null;

   public static event Action<int> ServerOnPlayerDie;//destroy the units of the died player
   public static event Action<UnitBase> ServerOnBaseSpawned;//everytime a base spawned, we add it to the list
   public static event Action<UnitBase> ServerOnBaseDespwaned;//remove the base from the list



   #region Server
   
   public override void OnStartServer()
   {
       health.ServerOnDie += ServerHandleDie;
       ServerOnBaseSpawned?.Invoke(this);
   }

   public override void OnStopServer()
   {
       ServerOnBaseDespwaned?.Invoke(this);
       health.ServerOnDie -= ServerHandleDie;
   }

   [Server]
   private void ServerHandleDie()
   {
       ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
       NetworkServer.Destroy(gameObject);
   }



   #endregion

   #region Client
   
   #endregion



}
