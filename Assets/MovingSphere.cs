using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    
    [SerializeField]
    private Camera cam;

    // Update is called once per frame
    void Update()
    {
        Vector3 camToSphere = transform.position - cam.transform.position;
        camToSphere.y = 0;
        camToSphere.Normalize();
        Vector3 camRight = Vector3.Cross(camToSphere, Vector3.up) * -1;
        camRight.Normalize();
        
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 displacement = camToSphere * playerInput.y + camRight * playerInput.x;
        transform.localPosition += displacement * (speed * Time.deltaTime);
    }
}
