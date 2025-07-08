using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
    float movementTimer;
    bool footstepsStarted;

    float speed = 5f;
    float gravity = -9.81f;
    float jumpHeight = 0.7f;
    float groundDrag = 10f;
    float airDrag = 2f;
    float sprintSpeed = 8f;
    float footstepsDelay = 0.3f;

    // Player Recoil Settings
    [Header("Player Recoil Settings")]
    public float playerRecoilForce = 0.2f;
    public float playerRecoilDuration = 0.2f;
    public float playerRecoilRecoverySpeed = 20f;
    [Header("Player Physics")]
    public float playerMass = 70f; 
    public float baselineMass = 70f;
    public float maxVerticalRecoil = 0.1f;
    public float gravityResistance = 0.8f;

    // Player Recoil Variables
    private Vector3 playerOriginalPosition;
    private Vector3 currentPlayerRecoilOffset;
    private float playerRecoilTimer;
    private Vector3 playerRecoilTargetOffset;
    private bool isPlayerRecoilActive;

    [SerializeField] AudioClip crouchAudioClip;
    [SerializeField] AudioClip uncrouchAudioClip;
    [SerializeField] AudioClip jumpAudioClip;
    [SerializeField] AudioClip landAudioClip;
    [SerializeField] AudioClip stepAudioClip;
    [SerializeField] AudioClip footstepsAudioClip;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Initialize player recoil variables
        currentPlayerRecoilOffset = Vector3.zero;
        playerRecoilTimer = 0f;
        playerRecoilTargetOffset = Vector3.zero;
        isPlayerRecoilActive = false;
        playerOriginalPosition = transform.position;

        // Load the jump.ogg file for both jumping and footsteps
        AudioClip jumpOggClip = Resources.Load<AudioClip>("assets/audio/jump");
        if (jumpOggClip == null)
        {
            Debug.LogWarning("Could not load jump.ogg from assets/audio/jump. Make sure the file is in Resources/assets/audio/ folder.");
        }
        else
        {
            jumpAudioClip = jumpOggClip;
            footstepsAudioClip = jumpOggClip;
        }
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
            {
                if (SfxManager.instance != null && landAudioClip != null)
                    SfxManager.instance.PlaySound(landAudioClip, transform, 0.4f);
            }
            else
            {
                if (SfxManager.instance != null && stepAudioClip != null)
                    SfxManager.instance.PlaySound(stepAudioClip, transform, 0.4f);
            }
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
        
        // Handle player recoil
        HandlePlayerRecoil();
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

    public void ApplyPlayerRecoil(Camera fpsCam)
    {
        if (fpsCam == null) return;

        // Check if player is moving - if so, don't apply recoil
        bool isPlayerMoving = controller.velocity.magnitude > 0.1f;

        // Only apply recoil if player is not moving
        if (isPlayerMoving) return;

        // Use the opposite direction of where the camera is aiming
        Vector3 recoilDirection = -fpsCam.transform.forward;
        recoilDirection = recoilDirection.normalized;

        // Calculate mass-based recoil force (heavier player = less recoil)
        float massMultiplier = baselineMass / playerMass; // 70kg is baseline
        Vector3 baseRecoilMovement = recoilDirection * playerRecoilForce * massMultiplier;

        // Separate horizontal and vertical components
        Vector3 horizontalRecoil = new Vector3(baseRecoilMovement.x, 0, baseRecoilMovement.z);
        float verticalRecoil = baseRecoilMovement.y;

        // Apply gravity resistance to vertical recoil and clamp it
        if (verticalRecoil > 0) // Only limit upward recoil
        {
            verticalRecoil *= (1f - gravityResistance);
            verticalRecoil = Mathf.Min(verticalRecoil, maxVerticalRecoil);
        }

        // Combine horizontal and limited vertical recoil
        Vector3 finalRecoilMovement = horizontalRecoil + new Vector3(0, verticalRecoil, 0);

        // Update the original position to current position to avoid teleporting
        playerOriginalPosition = transform.position;
        
        // Set the new target as current position + recoil movement
        playerRecoilTargetOffset = finalRecoilMovement;
        playerRecoilTimer = playerRecoilDuration;
        isPlayerRecoilActive = true;
    }

    private void HandlePlayerRecoil()
    {
        if (!isPlayerRecoilActive) return;

        if (playerRecoilTimer > 0)
        {
            playerRecoilTimer -= Time.deltaTime;
            
            // Calculate the movement needed this frame
            Vector3 targetPosition = playerOriginalPosition + playerRecoilTargetOffset;
            Vector3 currentPosition = transform.position;
            Vector3 remainingMovement = targetPosition - currentPosition;
            
            // Apply a portion of the remaining movement this frame
            Vector3 frameMovement = remainingMovement * Time.deltaTime * playerRecoilRecoverySpeed;
            
            controller.Move(frameMovement);
            
            // Check if we're close enough to the target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isPlayerRecoilActive = false;
            }
        }
        else
        {
            // Timer finished, stop the recoil animation
            isPlayerRecoilActive = false;
        }
    }

    void HandleFootsteps(bool isMoving)
    {
        if (isMoving)
        {
            // Increment movement timer
            movementTimer += Time.deltaTime;

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

            // Only start footsteps after 500ms of movement
            if (movementTimer >= footstepsDelay && !footstepsStarted)
            {
                footstepsStarted = true;

                // Start the looped footsteps audio
                if (footstepsAudioSource == null && footstepsAudioClip != null && SfxManager.instance != null)
                {
                    footstepsAudioSource = SfxManager.instance.PlaySoundHandled(footstepsAudioClip, transform, volume);
                    if (footstepsAudioSource != null)
                    {
                        footstepsAudioSource.loop = true;
                        footstepsAudioSource.pitch = pitchMultiplier;
                    }
                }
            }
            else if (footstepsStarted && footstepsAudioSource != null)
            {
                footstepsAudioSource.volume = volume;
                footstepsAudioSource.pitch = pitchMultiplier;

                if (!footstepsAudioSource.isPlaying)
                    footstepsAudioSource.Play();
            }
        }
        else
        {
            movementTimer = 0f;
            footstepsStarted = false;

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
            if (SfxManager.instance != null && jumpAudioClip != null)
                SfxManager.instance.PlaySound(jumpAudioClip, transform, 0.4f);
        }
    }

    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;

        if (isCrouching)
        {
            if (SfxManager.instance != null && crouchAudioClip != null)
                SfxManager.instance.PlaySound(crouchAudioClip, transform, 0.4f);
        }
        else
        {
            if (SfxManager.instance != null && uncrouchAudioClip != null)
                SfxManager.instance.PlaySound(uncrouchAudioClip, transform, 0.4f);
        }
    }
}