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
    private float verticalVelocity = 0f;
    
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
        if (!IsOwner || controller == null || !controller.enabled)
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
        
        // Handle ground check and vertical velocity
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // Slight downward force to stay grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        movementVector = (inputVector * speed) + (Vector3.up * verticalVelocity);
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
        
        if (IsOwner)
        {
            // If it is your character, turn on the camera and audio system
            if (plrCamera != null)
            {
                plrCamera.enabled = true;
                plrCamera.tag = "MainCamera";
            }
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            
            // Reposition slightly above floor and enable controller safely
            StartCoroutine(SafeSpawnPositionRoutine());
        }
        else
        {
            // If it is a remote character, turn off the camera, audio, and controller
            if (plrCamera != null) plrCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            if (controller != null) controller.enabled = false;
        }
    }
    
    private IEnumerator SafeSpawnPositionRoutine()
    {
        // Temporarily disable controller to prevent instant falling before physics settles
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Lift player slightly above the floor (+1.5 units) to prevent collider intersection
        transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        
        // Wait for physics system and scene colliders to initialize
        yield return new WaitForFixedUpdate();
        
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}