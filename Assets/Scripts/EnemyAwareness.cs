using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAwareness : MonoBehaviour
{
    public float awarenessRadius = 8f;
    public bool isAggro;
    public Material aggroMaterial;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = FindObjectOfType<PlayerMovement>().transform;
    }

    private void Update()
    {
        var dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist < awarenessRadius)
        {
            isAggro = true;
        }
        
        if (isAggro)
        {
            GetComponent<MeshRenderer>().material = aggroMaterial;
        }
    }
}
