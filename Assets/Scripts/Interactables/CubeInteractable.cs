using UnityEngine;

public class CubeInteractable : Interactable
{
    [Header("Damage Settings")]
    [SerializeField] private int minHealthDamage = 5;
    [SerializeField] private int maxHealthDamage = 20;
    [SerializeField] private int minArmorDamage = 3;
    [SerializeField] private int maxArmorDamage = 10;
    [SerializeField] private float interactionCooldown = 1f;

    private HealthLogic playerHealthLogic;
    private float lastInteractionTime;

    void Start()
    {
        promptMessage = "Press (E) to take damage";

        // Find player's health component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealthLogic = player.GetComponent<HealthLogic>();
            if (playerHealthLogic == null)
            {
                Debug.LogError("CubeInteractable: Player doesn't have HealthLogic component!");
            }
        }
        else
        {
            Debug.LogError("CubeInteractable: No GameObject with 'Player' tag found!");
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
            // Generate random damage amounts
            int healthDamage = Random.Range(minHealthDamage, maxHealthDamage + 1);
            int armorDamage = Random.Range(minArmorDamage, maxArmorDamage + 1);
            
            playerHealthLogic.TakeDirectDamage(healthDamage, armorDamage);
            lastInteractionTime = Time.time;
            Debug.Log($"Cube dealt {healthDamage} health damage and {armorDamage} armor damage to player!");
        }
        else
        {
            Debug.LogWarning("CubeInteractable: Cannot deal damage - HealthLogic is null!");
        }
    }
}