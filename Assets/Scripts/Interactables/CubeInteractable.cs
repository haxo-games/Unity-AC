using UnityEngine;

public class CubeInteractable : Interactable
{
    void Start()
    {
        promptMessage = "Press (E) to interact";
    }

    protected override void Interact()
    {
        Debug.Log("Interacted");
    }
}
