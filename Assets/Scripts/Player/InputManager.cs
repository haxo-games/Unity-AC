using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerMovement PlayerMovement;
    PlayerLook playerLook;
    PlayerUI playerUI;

    public PlayerInput playerInput;

    void Awake()
    {
        playerInput = new PlayerInput();

        PlayerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerUI = GetComponent<PlayerUI>();

        playerInput.OnFoot.Jump.performed += ctx => PlayerMovement.Jump();
        playerInput.OnFoot.Crouch.performed += ctx => PlayerMovement.ToggleCrouch();
        playerInput.OnFoot.Crouch.canceled += ctx => PlayerMovement.ToggleCrouch();

        playerInput.UI.MainMenu.performed += ctx => HandleToggleMainMenu();
    }

    void HandleToggleMainMenu()
    {
        Debug.Log("TEST");

        playerUI.ToggleMainMenu();

        if (playerUI.isMainMenuOpen)
            OnDisable();
        else
            OnEnable();
    }

    void FixedUpdate()
    {
        PlayerMovement.ProcessMove(playerInput.OnFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        if (!Application.isFocused || !IsMouseInGameArea())
            return;

        playerLook.ProcessLook(playerInput.OnFoot.Look.ReadValue<Vector2>());
    }

    private bool IsMouseInGameArea()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height - 30;
    }

    void OnEnable()
    {
        playerInput.OnFoot.Enable();
    }

    void OnDisable()
    {
        playerInput.OnFoot.Disable();
    }
}
