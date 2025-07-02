using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerMotor playerMotor;
    PlayerLook playerLook;

    void Awake()
    {
        playerInput = new PlayerInput();

        playerMotor = GetComponent<PlayerMotor>();
        playerLook = GetComponent<PlayerLook>();

        playerInput.OnFoot.Jump.performed += ctx => playerMotor.Jump();
        playerInput.OnFoot.Crouch.performed += ctx => playerMotor.ToggleCrouch();
        playerInput.OnFoot.Crouch.canceled += ctx => playerMotor.ToggleCrouch();
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
