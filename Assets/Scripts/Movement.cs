using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 displacement = new Vector3(playerInput.x, 0, playerInput.y);
        transform.localPosition += displacement * (speed * Time.deltaTime);
    }
}