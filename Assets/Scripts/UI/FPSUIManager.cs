using UnityEngine;
using TMPro;

public class FPSUIManager : MonoBehaviour
{
    public static FPSUIManager Instance;
    
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reserveAmmoText;
    
    [Header("Script References - Auto Found")]
    private HealthLogic playerHealthScript;
    private GunSystem gunSystemScript;
    
    
    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        FindAllScripts();
        FindUIElements(); 
    }
    
    void Start()
    {
        FindAllScripts();
        FindUIElements(); 
        SetRealValues();
    }
    
   
    void FindUIElements()
    {
        
        if (healthText == null)
            healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        
        if (shieldText == null)
            shieldText = GameObject.Find("ShieldText")?.GetComponent<TextMeshProUGUI>();
        
        if (ammoText == null)
            ammoText = GameObject.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();
        
        if (reserveAmmoText == null)
            reserveAmmoText = GameObject.Find("ReserveAmmoText")?.GetComponent<TextMeshProUGUI>();
        
        
        Debug.Log($"UI Elements Found - Health: {healthText != null}, Shield: {shieldText != null}, Ammo: {ammoText != null}, Reserve: {reserveAmmoText != null}");
    }
    
    void SetRealValues()
    {
        Debug.Log("Setting real values...");
        
        
        if (playerHealthScript != null)
        {
            Debug.Log("Health script found! Health value: " + playerHealthScript.health);
            UpdateHealth(playerHealthScript.health);
        }
        else
        {
            Debug.Log("Health script NOT found - using default");
            UpdateHealth(100);
        }
            
        // Get actual shield values
        if (playerHealthScript != null)
        {
            UpdateShield(playerHealthScript.armor);
        }
        else
        {
            UpdateShield(0);
        }
            
        if (gunSystemScript != null)
        {
            Debug.Log("Gun script found! Ammo: " + gunSystemScript.bulletsLeft + " Magazine: " + gunSystemScript.magazingSize);
            UpdateAmmo(gunSystemScript.bulletsLeft, gunSystemScript.magazingSize);
        }
        else
        {
            Debug.Log("Gun script NOT found - using default");
            UpdateAmmo(100, 0);
        }
    }
    
    void FindAllScripts()
    {
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealthScript = player.GetComponent<HealthLogic>();
        }
        else
        {
            playerHealthScript = FindFirstObjectByType<HealthLogic>();
        }
        
        gunSystemScript = FindFirstObjectByType<GunSystem>();
       
        
        // Debug 
        if (playerHealthScript != null)
            Debug.Log("Found HealthLogic script on: " + playerHealthScript.gameObject.name);
        else
            Debug.LogWarning("HealthLogic script not found in scene!");
            
        if (gunSystemScript != null)
            Debug.Log("Found GunSystem script on: " + gunSystemScript.gameObject.name);
        else
            Debug.LogWarning("GunSystem script not found in scene!");
            
        
    }
    
    public static void UpdateHealth(int health)
    {
        if (Instance != null && Instance.healthText != null)
        {
            Instance.healthText.text = health.ToString();
            Debug.Log("UI Health updated to: " + health);
        }
        else if (Instance != null && Instance.healthText == null)
        {
            Debug.LogWarning("healthText is not assigned!");
        }
    }
    
    public static void UpdateShield(int shield)
    {
        if (Instance != null && Instance.shieldText != null)
        {
            Instance.shieldText.text = shield.ToString();
            Debug.Log("UI Shield updated to: " + shield);
        }
        else if (Instance != null && Instance.shieldText == null)
        {
            Debug.LogWarning("shieldText is not assigned!");
        }
    }
    
    public static void UpdateAmmo(int current, int reserve)
    {
        Debug.Log("UpdateAmmo called with: " + current + "/" + reserve);
        if (Instance != null)
        {
            if (Instance.ammoText != null)
            {
                Instance.ammoText.text = current.ToString();
                Debug.Log("Set ammoText to: " + current);
            }
            else
            {
                Debug.LogWarning("ammoText is not assigned! Make sure you have a TextMeshProUGUI component named 'AmmoText' or assign it in the Inspector.");
            }
            
            if (Instance.reserveAmmoText != null)
            {
                Instance.reserveAmmoText.text = reserve.ToString();
                Debug.Log("Set reserveAmmoText to: " + reserve);
            }
            else
            {
                Debug.LogWarning("reserveAmmoText is not assigned! Make sure you have a TextMeshProUGUI component named 'ReserveAmmoText' or assign it in the Inspector.");
            }
        }
        else
        {
            Debug.LogWarning("FPSUIManager Instance is NULL!");
        }
    }
    
    
    public void RefreshAllUI()
    {
        if (playerHealthScript != null)
        {
            UpdateHealth(playerHealthScript.health);
            UpdateShield(playerHealthScript.armor); // FIXED: Use actual armor value
        }
        else
        {
            UpdateHealth(100);
            UpdateShield(0);
        }
            
        if (gunSystemScript != null)
            UpdateAmmo(gunSystemScript.bulletsLeft, gunSystemScript.magazingSize);
    }
    
    // Might use in future keep for now
    public void RefindScripts()
    {
        FindAllScripts();
        FindUIElements();
        RefreshAllUI();
    }
}