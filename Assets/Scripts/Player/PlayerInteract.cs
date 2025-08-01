using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayerMask = -1;
    
   
    [SerializeField] private bool enableDebugRay = false;
    
    private Camera playerCamera;
    private PlayerUI playerUI;
    private Interactable currentInteractable;
    private bool isShowingPrompt = false;
    
    void Start()
    {
        
        PlayerLook playerLook = GetComponent<PlayerLook>();
        if (playerLook != null)
            playerCamera = playerLook.cam;
        
       
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
        
        
        playerUI = GetComponent<PlayerUI>();
        
        
        if (playerCamera == null)
            Debug.LogError("PlayerInteract: No camera found! Make sure PlayerLook component has a camera assigned or Camera.main exists.");
        if (playerUI == null)
            Debug.LogError("PlayerInteract: No PlayerUI component found! Add PlayerUI component to this GameObject.");
    }
    
    void Update()
    {
        HandleInteractionDetection();
        HandleInteractionInput();
    }
    
    void HandleInteractionDetection()
    {
        if (playerCamera == null || playerUI == null) return;
        
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        
        if (enableDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, 0.1f);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayerMask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null && interactable.canInteract)
            {
                
                if (currentInteractable != interactable)
                {
                    
                    ClearInteraction(); 
                    currentInteractable = interactable;
                    ShowInteractionPrompt();
                    
                    if (enableDebugRay)
                        Debug.Log($"New interactable detected: {interactable.name}");
                }
            }
            else
            {
                // Hit something but it's not interactable or is disabled
                ClearInteraction();
            }
        }
        else
        {
            // Didn't hit anything within range
            ClearInteraction();
        }
    }
    
    void HandleInteractionInput()
    {
        // Check for interaction input
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Interacting with: {currentInteractable.name}");
            currentInteractable.TriggerInteract();
            
   
        }
    }
    
    void ShowInteractionPrompt()
    {
        if (playerUI != null && currentInteractable != null && !isShowingPrompt)
        {
            string prompt = currentInteractable.GetInteractionPrompt();
            playerUI.SetInteractText(prompt);
            isShowingPrompt = true;
            
            if (enableDebugRay)
                Debug.Log($"Showing prompt: '{prompt}'");
        }
    }
    
    void ClearInteraction()
    {
        if (currentInteractable != null || isShowingPrompt)
        {
            currentInteractable = null;
            isShowingPrompt = false;
            
            if (playerUI != null)
            {
                playerUI.SetInteractText("");
                
                if (enableDebugRay)
                    Debug.Log("Cleared interaction text");
            }
        }
    }
    
    
    public void TriggerInteract()
    {
        if (currentInteractable != null)
        {
            Debug.Log($"TriggerInteract called externally on: {currentInteractable.name}");
            currentInteractable.TriggerInteract();
        }
    }
    
 
    public Interactable GetCurrentInteractable() => currentInteractable;
    public bool IsShowingPrompt() => isShowingPrompt;
    
   
    public void ForceCloseInteraction()
    {
        ClearInteraction();
    }
    
    void OnDisable()
    {
        
        ClearInteraction();
    }
}