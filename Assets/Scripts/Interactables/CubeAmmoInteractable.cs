using UnityEngine;

public class CubeAmmoRestoreInteractable : Interactable
{
    [Header("Ammo Restore Settings")]
    [SerializeField] private int reserveAmmoAmount = 100; // Amount to add to reserve
    [SerializeField] private int maxReserveCapacity = 200; // Maximum reserve ammo capacity
    [SerializeField] private bool refillMagazine = true; // Whether to also refill current magazine
    [SerializeField] private float interactionCooldown = 1f;
    
    private float lastInteractionTime;
    private WeaponSwitching weaponSwitching;
    
    void Start()
    {
        promptMessage = "Press (E) to restore ammo";
        
        // Find weapon switching component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            weaponSwitching = player.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching == null)
            {
                weaponSwitching = Object.FindFirstObjectByType<WeaponSwitching>();
            }
            
            if (weaponSwitching == null)
            {
                Debug.LogError("CubeAmmoRestoreInteractable: WeaponSwitching component not found!");
            }
        }
        else
        {
            Debug.LogError("CubeAmmoRestoreInteractable: No GameObject with 'Player' tag found!");
        }
    }
    
    protected override void Interact()
    {
        // Check cooldown
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            Debug.Log("Ammo cube interaction on cooldown!");
            return;
        }
        
        // Find all GunSystem components (all weapons)
        GunSystem[] allWeapons = Object.FindObjectsByType<GunSystem>(FindObjectsSortMode.None);
        
        if (allWeapons != null && allWeapons.Length > 0)
        {
            int weaponsRestored = 0;
            
            foreach (GunSystem weapon in allWeapons)
            {
                // Calculate how much reserve ammo we can actually add (respect the cap)
                int currentReserve = weapon.GetReserveAmmo();
                int ammoToAdd = Mathf.Min(reserveAmmoAmount, maxReserveCapacity - currentReserve);
                
                // Only add ammo if we're not already at the cap
                if (ammoToAdd > 0)
                {
                    weapon.AddReserveAmmo(ammoToAdd);
                }
                
                // Optionally refill the current magazine
                if (refillMagazine)
                {
                    weapon.bulletsLeft = weapon.magazingSize;
                }
                
                weaponsRestored++;
            }
            
            // Update UI for the currently active weapon
            if (weaponSwitching != null)
            {
                // Find the currently active weapon by checking which one is active in the hierarchy
                foreach (GunSystem weapon in allWeapons)
                {
                    if (weapon.gameObject.activeInHierarchy)
                    {
                        FPSUIManager.UpdateAmmo(weapon.bulletsLeft, weapon.GetReserveAmmo());
                        break;
                    }
                }
            }
            
            lastInteractionTime = Time.time;
            
            string message = refillMagazine ? 
                $"Ammo cube restored {reserveAmmoAmount} reserve ammo and refilled magazines for {weaponsRestored} weapons!" :
                $"Ammo cube added {reserveAmmoAmount} reserve ammo to {weaponsRestored} weapons!";
                
            Debug.Log(message);
        }
        else
        {
            Debug.LogWarning("CubeAmmoRestoreInteractable: No weapons found!");
        }
    }
}