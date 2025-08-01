using System;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform[] weapons;

    [Header("Keys")]
    [SerializeField] private KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    [Header("Settings")]
    [SerializeField] private float switchTime;
    
    [Header("Animation Settings")]
    [SerializeField] private float switchAnimationSpeed = 8f;
    [SerializeField] private float switchSlideDistance = 2f;
    [SerializeField] private float switchDownTime = 0.3f;

    private int selectedWeapon;
    private float timeSinceLastSwitch;
    
    // Animation variables
    private bool isSwitching = false;
    private int targetWeapon = -1;
    private Vector3[] originalPositions;
    private bool isAnimatingUp = false;

    void Start()
    {
        SetWeapons();
        
        // Reset all weapons to their original positions
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].localPosition = originalPositions[i];
        }
        
        Select(selectedWeapon);
        timeSinceLastSwitch = 0f;
    }
    private void SetWeapons()
    {
        weapons = new Transform[transform.childCount];
        originalPositions = new Vector3[transform.childCount];

        print($"Found {transform.childCount} weapons");

        for (int i = 0; i < transform.childCount; i++)
        {
            weapons[i] = transform.GetChild(i);
            originalPositions[i] = weapons[i].localPosition;
            print($"Weapon {i}: {weapons[i].name}, Original Position: {originalPositions[i]}");
        }

        if (keys == null) keys = new KeyCode[weapons.Length];
    }

    private void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        // Only allow switching if not currently switching and enough time has passed
        if (!isSwitching)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    print($"Key {keys[i]} pressed for weapon {i}. Current weapon: {selectedWeapon}, Time since last switch: {timeSinceLastSwitch}, Switch time: {switchTime}");
                    
                    if (timeSinceLastSwitch >= switchTime && i != selectedWeapon)
                    {
                        targetWeapon = i;
                        StartWeaponSwitch();
                    }
                }
            }
        }

        timeSinceLastSwitch += Time.deltaTime;
        HandleSwitchAnimation();
    }

    private void Select(int WeaponIndex)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == WeaponIndex);
        }
        selectedWeapon = WeaponIndex;
        timeSinceLastSwitch = 0f;

        OnWeaponSelected();
    }
    
    private void StartWeaponSwitch()
    {
        if (isSwitching) return;
        
        print($"Starting weapon switch from {selectedWeapon} to {targetWeapon}");
        
        isSwitching = true;
        isAnimatingUp = false;
        
        // Start sliding current weapon down
        CancelInvoke();
        Invoke("SwitchToNewWeapon", switchDownTime);
    }
    
    private void SwitchToNewWeapon()
    {
        print($"Switching to new weapon {targetWeapon}");
        
        // Switch to the new weapon and start sliding it up
        Select(targetWeapon);
        isAnimatingUp = true;
        
        // Set new weapon to down position initially
        if (targetWeapon < weapons.Length && weapons[targetWeapon] != null)
        {
            Vector3 downPosition = originalPositions[targetWeapon] + Vector3.down * switchSlideDistance;
            weapons[targetWeapon].localPosition = downPosition;
            print($"Set weapon {targetWeapon} to down position: {downPosition}");
        }
        
        Invoke("FinishWeaponSwitch", switchDownTime);
    }
    
    private void FinishWeaponSwitch()
    {
        isSwitching = false;
        isAnimatingUp = false;
        targetWeapon = -1;
    }
    
    private void HandleSwitchAnimation()
    {
        if (!isSwitching) return;
        
        if (!isAnimatingUp)
        {
            // Animate current weapon going down
            if (selectedWeapon < weapons.Length && weapons[selectedWeapon] != null && weapons[selectedWeapon].gameObject.activeSelf)
            {
                Vector3 currentPos = weapons[selectedWeapon].localPosition;
                Vector3 targetPos = originalPositions[selectedWeapon] + Vector3.down * switchSlideDistance;
                Vector3 newPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * switchAnimationSpeed);
                weapons[selectedWeapon].localPosition = newPos;
                
                print($"Animating down - Current: {currentPos}, Target: {targetPos}, New: {newPos}");
            }
        }
        else
        {
            // Animate new weapon coming up
            if (selectedWeapon < weapons.Length && weapons[selectedWeapon] != null && weapons[selectedWeapon].gameObject.activeSelf)
            {
                Vector3 currentPos = weapons[selectedWeapon].localPosition;
                Vector3 targetPos = originalPositions[selectedWeapon];
                Vector3 newPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * switchAnimationSpeed);
                weapons[selectedWeapon].localPosition = newPos;
                
                print($"Animating up - Current: {currentPos}, Target: {targetPos}, New: {newPos}");
            }
        }
    }
    
    private void OnWeaponSelected()
    {
        print("New weapon Selected...");
    }
    
    // Public property to check if switching is in progress
    public bool IsSwitching => isSwitching;
}
