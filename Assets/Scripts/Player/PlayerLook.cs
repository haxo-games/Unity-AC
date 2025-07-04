using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    float xRotation;
    float xSensitivity = 30f;
    float ySensitivity = 30f;
    public Camera cam;

    public void ProcessLook(Vector2 input)
    {
        xRotation -= input.y * Time.deltaTime * ySensitivity;
        xRotation = Math.Clamp(xRotation, -90, 90);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * input.x * Time.deltaTime * xSensitivity);
    }
}