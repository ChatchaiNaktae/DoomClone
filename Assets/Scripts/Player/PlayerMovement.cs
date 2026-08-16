using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 10f;
    public float smoothTime = 0.15f; 
    
    private CharacterController controller;
    public Animator cameraAnimator;
    private bool isWalking;
    
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float gravity = -10f;
    
    private SpringVector3 momentumSpring;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Start()
    {
        // Initialize the spring with zero velocity
        momentumSpring = new SpringVector3(Vector3.zero, smoothTime);
        
        if (cameraAnimator == null)
        {
            cameraAnimator = GetComponentInChildren<Animator>();
        }
    }
    
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        GetInput();
        MovePlayer();
        
        if (cameraAnimator != null)
        {
            cameraAnimator.SetBool("isWalking", isWalking);
        }
    }
    
    void GetInput()
    {
        Vector3 targetInput = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            targetInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            targetInput.Normalize();
            targetInput = transform.TransformDirection(targetInput);
            
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
        
        // Assign the target direction to the spring
        momentumSpring.Target = targetInput;
        
        // Update the spring to calculate the smoothed momentum
        inputVector = momentumSpring.Update(Time.deltaTime);
        
        movementVector = (inputVector * speed) + (Vector3.up * gravity);
    }   
    
    void MovePlayer()
    {
        if (controller != null && controller.enabled)
        {
            controller.Move(movementVector * Time.deltaTime);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Camera plrCamera = GetComponentInChildren<Camera>();
        AudioListener audioListener = GetComponentInChildren<AudioListener>();
        CharacterController charController = GetComponent<CharacterController>();

        if (IsOwner)
        {
            // if it is your character, turn on the camera and audio system.
            if (plrCamera != null)
            {
                plrCamera.enabled = true;
                plrCamera.tag = "MainCamera";
            }
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            if (charController != null)
            {
                charController.enabled = true;
            }
        }
        else
        {
            // if it is a friend's character, turn off the camera and audio system.
            if (plrCamera != null)
            {
                plrCamera.enabled = false;
            }
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            if (charController != null)
            {
                charController.enabled = false;
            }
        }
    }
}