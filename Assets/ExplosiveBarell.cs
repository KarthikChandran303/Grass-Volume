using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ExplosiveBarell : MonoBehaviour
{
    private void OnEnable() => BarellManager.explosiveBarells.Add(this);
    private void OnDisable() => BarellManager.explosiveBarells.Remove(this);

    private void OnDrawGizmos()
    {
    }
}
