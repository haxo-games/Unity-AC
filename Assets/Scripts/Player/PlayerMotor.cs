using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private float crouchTimer;
    private bool lerpCrouch;
    private bool wasUngrounded;
    private bool isSprinting;
    private float initialUngroundedY;

    [SerializeField] AudioClip crouchAudioClip;
    [SerializeField] AudioClip uncrouchAudioClip;
    [SerializeField] AudioClip jumpAudioClip;
    [SerializeField] AudioClip landAudioClip;
    [SerializeField] AudioClip stepAudioClip;

    private float speed = 5f;
    private float gravity = -9.81f;
    private float jumpHeight = 0.7f;
    private float groundDrag = 10f;  // Higher = more responsive, lower = more sliding
    private float airDrag = 2f;     // Drag when in air 
    private float sprintSpeed = 8f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (!isGrounded)
        {
            if (!wasUngrounded)
            {
                initialUngroundedY = transform.position.y;
                wasUngrounded = true;
            }
        }
        else if (wasUngrounded)
        {
            float distanceFallen = Mathf.Abs(transform.position.y - initialUngroundedY);
            wasUngrounded = false;

            if (distanceFallen >= 1.5f)
                SfxManager.instance.PlaySound(landAudioClip, transform, 0.4f);
            else
                SfxManager.instance.PlaySound(stepAudioClip, transform, 0.4f);

        }

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer * crouchTimer;

            if (isCrouching)
                controller.height = Mathf.Lerp(controller.height, 1, p);
            else
                controller.height = Mathf.Lerp(controller.height, 2, p);

            if (p > 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }

        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        SetSprint(sprintInput);
    }

    public void ProcessMove(Vector2 input)
    {
        Vector3 worldDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y));
        float processedSpeed = speed;


        if (isCrouching)
            processedSpeed /= 2f;

        else if (isSprinting)
            processedSpeed = sprintSpeed;


        Vector3 targetVelocity = worldDirection * processedSpeed; // Target Velo Calcs


        float currentDrag = isGrounded ? groundDrag : airDrag; // Applying drag
        float dragMultiplier = 1f - (currentDrag * Time.deltaTime);


        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, currentDrag * Time.deltaTime); // Smoothing movment
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, currentDrag * Time.deltaTime);


        velocity.y += gravity * Time.deltaTime; // Grav

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    public void SetSprint(bool Sprinting)
    {
        if (isGrounded && !isCrouching)
            isSprinting = Sprinting;
        else
            isSprinting = false;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // v² = u² + 2as -> u = √(-2 × gravity × jumpHeight)
            SfxManager.instance.PlaySound(jumpAudioClip, transform, 0.4f);
        }
    }

    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;

        if (isCrouching)
            SfxManager.instance.PlaySound(crouchAudioClip, transform, 0.4f);
        else
            SfxManager.instance.PlaySound(uncrouchAudioClip, transform, 0.4f);
    }
}