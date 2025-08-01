using UnityEngine;

public class CubeInteractable : Interactable
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float interactionCooldown = 1f;
    
    private HealthLogic playerHealthLogic;
    private float lastInteractionTime;
    
    void Start()
    {

        promptMessage = "Press (E) to take damage";
        
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
            playerHealthLogic.TakeDamage(damageAmount);
            lastInteractionTime = Time.time;
            Debug.Log($"Cube dealt {damageAmount} damage to player!");
            
           
            StartCoroutine(FlashRed()); // Optional add later
        }
        else
        {
            Debug.LogWarning("CubeInteractable: Cannot deal damage - HealthLogic is null!");
        }
    }
    

    System.Collections.IEnumerator FlashRed()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            renderer.material.color = originalColor;
        }
    }
}