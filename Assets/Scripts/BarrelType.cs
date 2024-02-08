using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BarrelType : ScriptableObject
{
    [Range(1, 15)]
    public float radius;
    public float damage;
    public Color color = Color.red;
}
