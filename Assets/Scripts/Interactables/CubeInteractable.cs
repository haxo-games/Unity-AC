using UnityEngine;

public class CubeInteractable : Interactable
{
    HealthLogic lpHealthLogic;

    void Start()
    {
        promptMessage = "Press (E) to interact";
        lpHealthLogic = GameObject.FindWithTag("Player").GetComponent<HealthLogic>();
    }

    protected override void Interact()
    {
        lpHealthLogic.TakeDamage(10);
    }
}
