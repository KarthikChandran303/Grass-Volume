using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody body;
    
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;
    
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 5f;

    Vector3 velocity = Vector3.zero;
    Vector2 playerInput;
    
    bool playerJump = false;
    bool onGround = false;
    // Update is called once per frame

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        // Bitwise OR (|) compares each bit of the two operands and produces a result where each bit of the output is set to 1 if at least one of the corresponding bits of the input operands is 1.
        playerJump |= Input.GetButtonDown("Jump");
    }

    // Each physics step begins with invoking all FixedUpdate methods, after which PhysX does its thing, and at the end the collision methods get invoked. 
    private void FixedUpdate()
    {
        velocity = body.velocity;
        Move2(playerInput);
        if (playerJump)
        {
            playerJump = false;
            Jump();
        }
        body.velocity = velocity;
        onGround = false;
    }

    private void Jump()
    {
        if (onGround)
        {
            velocity.y += Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
        }
    }
    void Move2(Vector2 playerInput)
    {
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
    }
    
    void Move(Vector2 playerInput)
    {
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        if(velocity.x < desiredVelocity.x)
        {
            velocity.x = Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
        }
        else if(velocity.x > desiredVelocity.x)
        {
            velocity.x = Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
        }
        if(velocity.z < desiredVelocity.z)
        {
            velocity.z = Mathf.Min(velocity.z + maxSpeedChange, desiredVelocity.z);
        }
        else if(velocity.z > desiredVelocity.z)
        {
            velocity.z = Mathf.Max(velocity.z - maxSpeedChange, desiredVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        EvaluateCollision(col);
    }

    private void OnCollisionStay(Collision col)
    {
        EvaluateCollision(col);
    }
    
    private void EvaluateCollision(Collision col)
    {
        for(int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.GetContact(i).normal;
            onGround |= normal.y >= 0.9f;
        }
    }

}