using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    private Transform targetCamera;
    
    // Update is called once per frame
    private void Update()
    {
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            FindActiveCamera();
            if (targetCamera == null) return;
        }

        transform.LookAt(targetCamera);
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