using UnityEngine;

public class CubeHealthRestoreInteractable : Interactable
{
    [Header("Healing Settings")]
    [SerializeField] private int targetHealth = 100;
    [SerializeField] private int shieldAmount = 50;
    [SerializeField] private float interactionCooldown = 1f;
    
    private HealthLogic playerHealthLogic;
    private float lastInteractionTime;
    
    void Start()
    {
        promptMessage = "Press (E) to restore health and gain shield";
        
        // Find player's health component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealthLogic = player.GetComponent<HealthLogic>();
            if (playerHealthLogic == null)
            {
                Debug.LogError("CubeHealthRestoreInteractable: Player doesn't have HealthLogic component!");
            }
        }
        else
        {
            Debug.LogError("CubeHealthRestoreInteractable: No GameObject with 'Player' tag found!");
        }
    }
    
    protected override void Interact()
    {
        // Check cooldown
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            Debug.Log("Cube interaction on cooldown!");
            return;
        }
        
        if (playerHealthLogic != null)
        {
            // Set health to target amount (100)
            playerHealthLogic.health = targetHealth;
            
            // Set shield to max amount (50) - don't exceed the cap
            playerHealthLogic.armor = shieldAmount;
            
            // Update UI - check if the player object has the Player tag
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                FPSUIManager.UpdateHealth(playerHealthLogic.health);
                FPSUIManager.UpdateShield(playerHealthLogic.armor);
            }
            
            lastInteractionTime = Time.time;
            Debug.Log($"Cube restored player health to {targetHealth} and set shield to {shieldAmount}!");
        }
        else
        {
            Debug.LogWarning("CubeHealthRestoreInteractable: Cannot restore health - HealthLogic is null!");
        }
    }
}