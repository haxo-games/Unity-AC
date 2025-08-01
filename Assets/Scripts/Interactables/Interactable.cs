using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    
    public string promptMessage = "Press (E) to interact";
    public bool canInteract = true;
    
    protected abstract void Interact();
    
    public void TriggerInteract()
    {
        if (canInteract)
        {
            Interact();
        }
    }
    
    public virtual string GetInteractionPrompt()
    {
        return promptMessage;
    }
    
}