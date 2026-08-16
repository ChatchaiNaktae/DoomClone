using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpriteLook : MonoBehaviour
{
    private Transform targetCamera;
    public bool canLookVertically;
    
    private void Update()
    {
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            FindActiveCamera();
            if (targetCamera == null) return;
        }
        
        if (canLookVertically)
        {
            transform.LookAt(targetCamera);
        }
        else
        {
            Vector3 modifiedTarget = targetCamera.position;
            modifiedTarget.y = transform.position.y;
            transform.LookAt(modifiedTarget);
        }
    }
    
    private void FindActiveCamera()
    {
        Camera[] allCams = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCams)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy && cam.gameObject.name != "LobbyCamera")
            {
                targetCamera = cam.transform;
                break;
            }
        }
    }
}