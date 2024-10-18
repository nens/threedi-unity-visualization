using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundTerrain : MonoBehaviour
{
    public Transform playercamera;
    public GameObject Centre;
    public float rotateSpeed = 3.0F;

    void Start()
    {
    }
    void Update()
    {
        //camera.transform.position = player.transform.position + camera.transform.position;
        playercamera.transform.LookAt(Centre.transform.position);
        playercamera.transform.RotateAround(Centre.transform.position, Vector3.up, rotateSpeed * Time.deltaTime + 0.0000001f);
    }

}
