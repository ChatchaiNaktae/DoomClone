using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAwareness : MonoBehaviour
{
    public float awarenessRadius = 8f;
    public bool isAggro;
    public Transform playerTransform;
    
    private void Start()
    {
        FindClosestPlayer();
    }
    
    private void Update()
    {
        // If there is no player yet, or if the existing one has disconnected or crashed, search again.
        if (playerTransform == null)
        {
            FindClosestPlayer();
            if (playerTransform == null)
            {
                isAggro = false;
                return;
            }
        }
        
        var dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist < awarenessRadius)
        {
            isAggro = true;
        }
        else
        {
            isAggro = false;
        }
    }
    
    // Function to find the nearest player in the scene.
    public void FindClosestPlayer()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        float closestDistance = Mathf.Infinity;
        Transform closest = null;
        
        foreach (var p in allPlayers)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = p.transform;
            }
        }
        
        playerTransform = closest;
    }
}
