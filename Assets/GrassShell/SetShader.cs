using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShader : MonoBehaviour
{
    [SerializeField]
    private RenderTexture rt;

    [SerializeField] private Transform target;
    [SerializeField] private Material material;
    // Start is called before the first frame update
    void Awake()
    {

        material.SetTexture("_ParticleTexture", rt);
        material.SetFloat("_OrthographicCamSize",GetComponent<Camera>().orthographicSize);
    }
    
    void Update()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
        material.SetVector("_Position", transform.position);
    }
}
