using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    float xRotation;
    float xSensitivity = 30f;
    float ySensitivity = 30f;
    public Camera cam;
    
    // Recoil offset that gets added to the base rotation
    private Vector3 recoilOffset = Vector3.zero;

    public void ProcessLook(Vector2 input)
    {
        xRotation -= input.y * Time.deltaTime * ySensitivity;
        xRotation = Math.Clamp(xRotation, -90, 90); // Perfect symmetry: straight up to straight down
        
        // Apply base rotation + recoil offset
        Vector3 finalRotation = new Vector3(xRotation, 0, 0) + recoilOffset;
        
        // Clamp the final rotation - equal range up and down
        finalRotation.x = Math.Clamp(finalRotation.x, -90, 90);
        
        cam.transform.localRotation = Quaternion.Euler(finalRotation);
        transform.Rotate(Vector3.up * input.x * Time.deltaTime * xSensitivity);
    }
    
    // Method for GunSystem to add recoil
    public void AddRecoilOffset(Vector3 offset)
    {
        recoilOffset += offset;
    }
    
    // Method for GunSystem to set recoil directly
    public void SetRecoilOffset(Vector3 offset)
    {
        recoilOffset = offset;
    }
    
    // Method to get current recoil offset
    public Vector3 GetRecoilOffset()
    {
        return recoilOffset;
    }
}