using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerMotor playerMotor;
    PlayerLook playerLook;
    PlayerUI playerUI;

    public PlayerInput playerInput;

    void Awake()
    {
        playerInput = new PlayerInput();

        playerMotor = GetComponent<PlayerMotor>();
        playerLook = GetComponent<PlayerLook>();
        playerUI = GetComponent<PlayerUI>();

        playerInput.OnFoot.Jump.performed += ctx => playerMotor.Jump();
        playerInput.OnFoot.Crouch.performed += ctx => playerMotor.ToggleCrouch();
        playerInput.OnFoot.Crouch.canceled += ctx => playerMotor.ToggleCrouch();

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
        playerMotor.ProcessMove(playerInput.OnFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        playerLook.ProcessLook(playerInput.OnFoot.Look.ReadValue<Vector2>());
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
