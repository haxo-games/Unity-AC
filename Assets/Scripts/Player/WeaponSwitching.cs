using System;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform[] weapons;

    [Header("Keys")]
    [SerializeField] private KeyCode[] keys;

    [Header("Settings")]
    [SerializeField] private float switchTime;

    private int selectedWeapon;
    private float timeSinceLastSwitch;

    void Start()
    {
        SetWeapons();
        Select(selectedWeapon);

        timeSinceLastSwitch = 0f;
    }
    private void SetWeapons()
    {
        throw new NotImplementedException();
    }

    private void Select(int selectedWeapon)
    {
        throw new NotImplementedException();
    }
}
