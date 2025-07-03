using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string promptMessage;

    protected abstract void Interact();

    public void InitialInteract()
    {
        Interact();
    }
}
