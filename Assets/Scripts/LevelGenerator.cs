using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject Bar;

    private void Awake()
    {
        CameraBounds.updateBounds(10.0f);
        GenerateBox();
    }
    
    private void GenerateBox()
    {
        float height = CameraBounds.TOPRIGHT.y - CameraBounds.BOTTOMLEFT.y;
        float width = CameraBounds.TOPRIGHT.x - CameraBounds.BOTTOMLEFT.x;
        
        GameObject right = Instantiate(Bar, new Vector3(CameraBounds.TOPRIGHT.x, CameraBounds.TOPRIGHT.y - height/2.0f, CameraBounds.TOPRIGHT.z), Quaternion.identity);
        right.transform.localScale = new Vector3(right.transform.localScale.x, height, right.transform.lossyScale.z);
        
        GameObject left = Instantiate(Bar, new Vector3(CameraBounds.BOTTOMLEFT.x, CameraBounds.BOTTOMLEFT.y + height/2.0f, CameraBounds.BOTTOMLEFT.z), Quaternion.identity);
        left.transform.localScale = new Vector3(left.transform.localScale.x, height, left.transform.lossyScale.z);
        
        GameObject bottom = Instantiate(Bar, new Vector3(CameraBounds.BOTTOMLEFT.x + width/2.0f, CameraBounds.BOTTOMLEFT.y, CameraBounds.BOTTOMLEFT.z), Quaternion.identity);
        bottom.transform.localScale = new Vector3(width, bottom.transform.lossyScale.y, bottom.transform.lossyScale.z);
        
        GameObject top = Instantiate(Bar, new Vector3(CameraBounds.TOPRIGHT.x - width/2.0f, CameraBounds.TOPRIGHT.y, CameraBounds.TOPRIGHT.z), Quaternion.identity);
        top.transform.localScale = new Vector3(width, top.transform.lossyScale.y, top.transform.lossyScale.z);
    }
}
