using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float WalkSpeed;
    [SerializeField] private float SprintSpeed;
    [SerializeField] private float CrouchSpeed;
    [SerializeField] private float Multiplier;
    [SerializeField] private float MaxDistDelta;
    [SerializeField] private Transform orientation;
    
    [Header("Normal Player Col")]
    [SerializeField] private Vector3 nPosition = new Vector3 (0, -0.3f, 0);
    [SerializeField] private float nHeight = 1.9f;

    [Header("Crouched Player Col")]
    [SerializeField] private Vector3 crouchPosition = new Vector3 (0, -0.75f, 0);
    [SerializeField] private float crouchHeight = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    Vector3 moveDir;
    float horizontal;
    float vertical;

    bool isSprinting;

    public enum WalkState
    {
        Walking,
        Sprinting,
        Crouching
    }

    Rigidbody rb;
    CapsuleCollider col;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        col = this.GetComponent<CapsuleCollider>();

    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");         
        vertical = Input.GetAxisRaw("Vertical");      

        //moveDir = new Vector3(horizontal, 0, vertical);
        moveDir = orientation.right * horizontal + orientation.forward * vertical;

        
        Move();

    }
  
    void Move(){
        
        float Speed;
        WalkState walkState;

        if(Input.GetKey(KeyCode.LeftShift))                     {   walkState = WalkState.Sprinting;    animator.SetBool("IsCrouching", false);    }
        else if(Input.GetKey(KeyCode.LeftControl))              {   walkState = WalkState.Crouching;    animator.SetBool("IsCrouching", true);      setCollider(crouchPosition, crouchHeight);  }
        else                                                    {   walkState = WalkState.Walking;      animator.SetBool("IsCrouching", false);     setCollider(nPosition, nHeight);            }


        bool isMoving;

        if(rb.velocity.magnitude > 0.1f){
            isMoving = true;
        }
        else{
            isMoving = false;
        }

        switch(walkState){
            case WalkState.Walking:
                animator.SetBool("IsWalking", isMoving);
                animator.SetBool("IsSprinting", false);
                animator.SetBool("IsCrouchWalking", false);
                Speed = WalkSpeed;
                break;
            case WalkState.Sprinting:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsSprinting", isMoving);
                animator.SetBool("IsCrouchWalking", false);
                Speed = SprintSpeed;
                break;
            case WalkState.Crouching:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsSprinting", false);
                animator.SetBool("IsCrouchWalking", isMoving);
                Speed = CrouchSpeed;
                break;
            default:
                Speed = WalkSpeed;
                break;
        }

        rb.velocity = moveDir.normalized * Speed * Multiplier;
    }


    void setCollider(Vector3 colPos, float colHeight){
        col.center = colPos;
        col.height = colHeight;
    }
}
