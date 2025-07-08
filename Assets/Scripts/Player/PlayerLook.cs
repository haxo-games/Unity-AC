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
        
        // Apply base rotation + recoil offset
        Vector3 finalRotation = new Vector3(xRotation, 0, 0) + recoilOffset;
        
        // Clamp the final rotation to the viewing limits
        finalRotation.x = Math.Clamp(finalRotation.x, -90, 90);
        
        // Adjust base rotation to compensate for recoil
        // If we have positive recoil, we need to allow base rotation to go lower to look down
        // AND higher to look up past the recoil
        float minBaseRotation = -90 - recoilOffset.x; // Can go lower to compensate for positive recoil
        float maxBaseRotation = 90 - recoilOffset.x;  // Can go higher to compensate for positive recoil
        
        xRotation = Math.Clamp(xRotation, minBaseRotation, maxBaseRotation);
        
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