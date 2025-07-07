using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;
    bool isCrouching;
    float crouchTimer;
    bool lerpCrouch;
    bool wasUngrounded;
    bool isSprinting;
    float initialUngroundedY;

    bool wasMoving;
    AudioSource footstepsAudioSource;

    float speed = 5f;
    float gravity = -9.81f;
    float jumpHeight = 0.7f;
    float groundDrag = 10f;
    float airDrag = 2f;
    float sprintSpeed = 8f;

    [SerializeField] AudioClip crouchAudioClip;
    [SerializeField] AudioClip uncrouchAudioClip;
    [SerializeField] AudioClip jumpAudioClip;
    [SerializeField] AudioClip landAudioClip;
    [SerializeField] AudioClip stepAudioClip;
    [SerializeField] AudioClip footstepsAudioClip;

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

        Vector3 targetVelocity = worldDirection * processedSpeed;
        float currentDrag = isGrounded ? groundDrag : airDrag;

        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, currentDrag * Time.deltaTime);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, currentDrag * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;

        bool isMoving = (input.x != 0 || input.y != 0) && isGrounded;
        HandleFootsteps(isMoving);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps(bool isMoving)
    {
        if (isMoving)
        {
            float pitchMultiplier = 1f;
            float volume = 0.4f;

            if (isCrouching)
            {
                pitchMultiplier = 0.7f;
                volume = 0.2f;
            }
            else if (isSprinting)
            {
                pitchMultiplier = 1.3f;
                volume = 0.5f;
            }

            // Check if we need to start footsteps audio
            if (footstepsAudioSource == null)
            {
                footstepsAudioSource = SfxManager.instance.PlaySoundHandled(footstepsAudioClip, transform, volume);
                if (footstepsAudioSource != null)
                {
                    footstepsAudioSource.loop = true;
                    footstepsAudioSource.pitch = pitchMultiplier;
                }
            }
            else
            {
                // Update existing footsteps audio
                footstepsAudioSource.volume = volume;
                footstepsAudioSource.pitch = pitchMultiplier;

                // Ensure it's still playing (in case it stopped for some reason)
                if (!footstepsAudioSource.isPlaying)
                {
                    footstepsAudioSource.Play();
                }
            }
        }
        else
        {
            // Stop footsteps when not moving
            if (footstepsAudioSource != null)
            {
                footstepsAudioSource.Stop();
                Destroy(footstepsAudioSource.gameObject);
                footstepsAudioSource = null;
            }
        }

        wasMoving = isMoving;
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
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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