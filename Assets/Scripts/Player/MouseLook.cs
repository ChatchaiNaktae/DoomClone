using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    public float sensitivity = 1.5f;
    public float smoothing = 1.5f;
    
    [Header("Camera Reference")]
    public Transform cameraTransform;

    private float xMousePos;
    private float yMousePos;
    
    private float smoothedMouseXPos;
    private float smoothedMouseYPos;

    private float currentLookingXPos;
    private float currentLookingYPos;

    void Start()
    {
        // lock and hide the cursor
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        if (PauseMenuController.IsPaused)
        {
            return;
        }
        
        if (!IsOwner)
        {
            return;
        }
        
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    void GetInput()
    {
        xMousePos = Input.GetAxisRaw("Mouse X");
        yMousePos = Input.GetAxisRaw("Mouse Y");
    }

    void ModifyInput()
    {
        xMousePos *= sensitivity * smoothing;
        yMousePos *= sensitivity * smoothing;
        
        smoothedMouseXPos = Mathf.Lerp(smoothedMouseXPos, xMousePos, 1f / smoothing);
        smoothedMouseYPos = Mathf.Lerp(smoothedMouseYPos, yMousePos, 1f / smoothing);
    }

    void MovePlayer()
    {
        currentLookingXPos += smoothedMouseXPos;
        currentLookingYPos -= smoothedMouseYPos;
        currentLookingYPos = Mathf.Clamp(currentLookingYPos, -80f, 80f);
        transform.localRotation = Quaternion.AngleAxis(currentLookingXPos, transform.up);
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.AngleAxis(currentLookingYPos, Vector3.right);
        }
    }
}
