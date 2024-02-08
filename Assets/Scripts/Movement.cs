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

    Vector3 velocity = Vector3.zero;
    // Update is called once per frame

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        Move(playerInput);
    }
    
    void Move2(Vector2 playerInput)
    {
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        transform.localPosition += displacement;
    }
    
    void Move(Vector2 playerInput)
    {
        velocity = body.velocity;
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
        body.velocity = velocity;
    }
}