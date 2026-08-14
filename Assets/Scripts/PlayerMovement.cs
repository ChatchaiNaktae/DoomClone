using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Initialize the spring with zero velocity
        momentumSpring = new SpringVector3(Vector3.zero, smoothTime);
    }
    
    void Update()
    {
        GetInput();
        MovePlayer();
        
        cameraAnimator.SetBool("isWalking", isWalking);
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
        controller.Move(movementVector * Time.deltaTime);
    }
}