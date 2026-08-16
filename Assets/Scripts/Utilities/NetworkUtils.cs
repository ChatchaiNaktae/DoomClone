using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public static class NetworkUtils
{
    /// <summary>
    /// Finds and returns the local player (the one owned by this client).
    /// </summary>
    public static PlayerMovement GetLocalPlayer()
    {
        PlayerMovement[] players = Object.FindObjectsOfType<PlayerMovement>();
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                return player;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Finds the closest player to a given position (ideal for enemy AI targeting).
    /// </summary>
    public static Transform GetClosestPlayer(Vector3 originPosition)
    {
        PlayerMovement[] players = Object.FindObjectsOfType<PlayerMovement>();
        float closestDist = Mathf.Infinity;
        Transform closestTarget = null;
        
        foreach (var player in players)
        {
            float dist = Vector3.Distance(originPosition, player.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = player.transform;
            }
        }
        
        return closestTarget;
    }
    
    /// <summary>
    /// Safely despawns a NetworkObject on the server, or destroys it if it is a regular GameObject.
    /// </summary>
    public static void DespawnOrDestroy(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        NetworkObject netObj = targetObject.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            netObj.Despawn(true);
        }
        else
        {
            Object.Destroy(targetObject);
        }
    }
}