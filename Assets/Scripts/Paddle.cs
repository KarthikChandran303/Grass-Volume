using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField] private bool useAcceleration = false;
    
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 5f;

    Vector3 velocity = Vector3.zero;
    Vector2 playerInput;
    // Start is called before the first frame update
    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        transform.position += new Vector3(playerInput.x, playerInput.y, 0) * (Time.deltaTime * maxSpeed);
        // Bitwise OR (|) compares each bit of the two operands and produces a result where each bit of the output is set to 1 if at least one of the corresponding bits of the input operands is 1.
        //playerJump |= Input.GetButtonDown("Jump");    
    }

    private void FixedUpdate()
    {
        
    }

    void Move(Vector2 playerInput)
    {
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            Rigidbody ballRB = collision.transform.GetComponent<Rigidbody>();
            
        }
    }
}
