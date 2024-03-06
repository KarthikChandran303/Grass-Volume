using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;
    Vector3 velocity = Vector3.zero;

    private Rigidbody body;
    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        SpawnBall();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    private void SpawnBall()
    {
        body.velocity = Vector3.zero;
        transform.position = new Vector3 (0.0f, 2.4f, 0.0f);
        StartCoroutine(StartDelay());
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(1.0f);
        body.velocity = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0.2f, 1.0f), 0.0f).normalized * speed;
    }
}
