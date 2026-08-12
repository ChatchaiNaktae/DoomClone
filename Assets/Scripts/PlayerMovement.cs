using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float momentumDumping = 5f;
    
    private CharacterController controller;
    public Animator cameraAnimator;
    private bool isWalking;
    
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float gravity = -10f;
    
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        MovePlayer();
        
        cameraAnimator.SetBool("isWalking", isWalking);
    }

    void GetInput()
    {
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            inputVector.Normalize();
            inputVector = transform.TransformDirection(inputVector);

            isWalking = true;
        }
        else
        {
            inputVector = Vector3.Lerp(inputVector, Vector3.zero, momentumDumping * Time.deltaTime);
            
            isWalking = false;
        }
        
        movementVector = (inputVector * speed) + (Vector3.up * gravity);
    }   

    void MovePlayer()
    {
        controller.Move(movementVector * Time.deltaTime);
    }
}
