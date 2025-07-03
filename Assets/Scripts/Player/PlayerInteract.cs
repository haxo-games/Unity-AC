using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    Camera cam;
    [SerializeField] float distance = 3f;
    [SerializeField] LayerMask mask;
    Interactable currentInteractable;
    PlayerUI playerUI;
    InputManager inputManager;

    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        playerUI.SetInteractText(String.Empty);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            currentInteractable = hitInfo.collider.GetComponent<Interactable>();

            if (currentInteractable != null)
            {
                if (inputManager.playerInput.OnFoot.Interact.triggered)
                    currentInteractable.InitialInteract();

                playerUI.SetInteractText(currentInteractable.promptMessage);
            }
        }
    }

    public void TriggerInteract()
    {
        if (!currentInteractable)
            return;

        currentInteractable.InitialInteract();
    }
}
