using UnityEngine;
using UnityEngine.InputSystem.Interactions;
public class GunSystem : MonoBehaviour
{
    // Gun Stats
    public int damage;
    public float fireRate, spread, range, reloadTime, timeBetweenShots;
    public int magazingSize, bulletperTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    // Recoil Settings
    [Header("Recoil Settings")]
    public float recoilForce = 0.0f;
    public float recoilDuration = 0.1f;
    public float recoilRecoverySpeed = 25f;

    // Player Recoil Settings
    [Header("Player Recoil Settings")]
    public float playerRecoilForce = 0.1f; // How much the player moves backward
    public float playerRecoilDuration = 0.2f; // How long the backward movement lasts
    public float playerRecoilRecoverySpeed = 10f; // How quickly player recovers position

    // Reload Animation Settings
    [Header("Reload Animation Settings")]
    public float reloadSlideDistance = 1f;
    public float reloadMotionSpeed = 8f;
    public float slideDownTime = 1.5f; // Time to stay down before sliding up

    // Muzzle Flash Settings
    [Header("Muzzle Flash Settings")]
    public GameObject muzzleFlashPrefab; // Assign your muzzle flash prefab
    public float muzzleFlashDuration = 0.05f; // How long the flash lasts
    public float muzzleFlashDistance = 2f; // Distance from camera
    public Vector3 muzzleFlashOffset = new Vector3(0, 0, 0); // Position offset (X=right, Y=up, Z=forward)
    public float muzzleFlashScale = 1f; // Scale of the muzzle flash
    public LayerMask wallLayers = -1; // What layers count as walls for collision check
    public float minFlashDistance = 0.3f; // Minimum distance to keep flash visible

    // Cases
    bool shooting, readyToShoot, reloading;

    // Recoil Variables
    private Vector3 originalPosition;
    private Vector3 currentRecoilOffset;
    private float recoilTimer;

    // Player Recoil Variables
    private Vector3 playerOriginalPosition;
    private Vector3 currentPlayerRecoilOffset;
    private float playerRecoilTimer;

    // Reload Animation Variables
    private Vector3 reloadTargetOffset;
    private bool isReloadAnimating;
    private bool isSlideUp; // Track if we're sliding up or down
    private bool forceFinishReload; // Force reload to finish smoothly

    // Muzzle Flash Variables
    private GameObject currentMuzzleFlash;
    private float muzzleFlashTimer;

    // References
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    // Player Reference (automatically finds the player)
    private Transform playerTransform;
    private CharacterController playerController;

    // Audio
    private AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    [Range(0f, 1f)]
    public float shootSoundVolume = 0.1f;
    [Range(0f, 1f)]
    public float reloadSoundVolume = 0.1f;

    private void Awake()
    {
        bulletsLeft = magazingSize;
        readyToShoot = true;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (shootSound == null)
            shootSound = Resources.Load<AudioClip>("Audio/auto");
        if (reloadSound == null)
            reloadSound = Resources.Load<AudioClip>("Audio/auto_reload");

        // Store the original position for recoil and reload animations
        originalPosition = transform.localPosition;
        currentRecoilOffset = Vector3.zero;
        recoilTimer = 0f;
        isReloadAnimating = false;
        isSlideUp = false;
        reloadTargetOffset = Vector3.zero;
        forceFinishReload = false;
        muzzleFlashTimer = 0f;

        // Initialize player recoil variables
        currentPlayerRecoilOffset = Vector3.zero;
        playerRecoilTimer = 0f;

        // Find player transform and controller
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        // Try to find player by tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<CharacterController>();
        }
        else
        {
            // If no player tag, try to find by looking for CharacterController
            playerController = Object.FindFirstObjectByType<CharacterController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }

        // Store original player position if found
        if (playerTransform != null)
        {
            playerOriginalPosition = playerTransform.position;
        }
    }

    private void Update()
    {
        MyInput();
        HandleRecoil();
        HandlePlayerRecoil();
        HandleReloadAnimation();
        HandleMuzzleFlash();
    }

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazingSize && !reloading)
        {
            Reload();
            if (audioSource != null && reloadSound != null)
                audioSource.PlayOneShot(reloadSound, reloadSoundVolume);
        }

        // Shoot continuously while holding mouse button and have bullets
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            Shoot();
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Play shoot sound for each shot
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound, shootSoundVolume);

        // Apply recoil
        ApplyRecoil();

        // Apply player recoil (backward movement)
        ApplyPlayerRecoil();

        // Show muzzle flash
        ShowMuzzleFlash();

        // Raycast Bullet
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);
            if (rayHit.collider.CompareTag("Enemy"))
                rayHit.collider.GetComponent<HealthLogic>().takeDamage(damage);
        }

        bulletsLeft--;
        bulletsShot++;

        // Reset shots after time between shots
        Invoke("ResetShots", timeBetweenShots);
    }

    private void ApplyPlayerRecoil()
    {
        if (playerTransform == null || fpsCam == null) return;

        // Calculate backward direction (opposite to camera forward)
        Vector3 recoilDirection = -fpsCam.transform.forward;

        // Only use horizontal movement (remove Y component for ground-based movement)
        recoilDirection.y = 0;
        recoilDirection = recoilDirection.normalized;

        // Apply recoil force
        Vector3 recoilMovement = recoilDirection * playerRecoilForce;

        // If using CharacterController, use Move method
        if (playerController != null)
        {
            playerController.Move(recoilMovement);
        }
        else
        {
            // If no CharacterController, directly modify position
            playerTransform.position += recoilMovement;
        }

        // Set timer for recoil duration
        playerRecoilTimer = playerRecoilDuration;
    }

    private void HandlePlayerRecoil()
    {
        if (playerRecoilTimer > 0)
            playerRecoilTimer -= Time.deltaTime;
        
    }

    private void ShowMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || fpsCam == null) return;

        // Remove any existing muzzle flash
        if (currentMuzzleFlash != null)
        {
            Destroy(currentMuzzleFlash);
        }

        // Calculate position relative to camera instead of attack point
        Vector3 cameraPosition = fpsCam.transform.position;
        Vector3 forwardDirection = fpsCam.transform.forward;
        Vector3 rightDirection = fpsCam.transform.right;
        Vector3 upDirection = fpsCam.transform.up;

        // Apply offset in camera space (more predictable)
        Vector3 offsetInWorldSpace = rightDirection * muzzleFlashOffset.x +
                                    upDirection * muzzleFlashOffset.y +
                                    forwardDirection * muzzleFlashOffset.z;

        Vector3 desiredPosition = cameraPosition + forwardDirection * muzzleFlashDistance + offsetInWorldSpace;

        // Check for wall collision and get both position and scale factor
        var flashData = GetValidFlashPositionAndScale(desiredPosition, cameraPosition);

        // Create muzzle flash
        currentMuzzleFlash = Instantiate(muzzleFlashPrefab, flashData.position, fpsCam.transform.rotation);

        // Parent it to the camera so it follows naturally
        currentMuzzleFlash.transform.SetParent(fpsCam.transform);

        // Apply scale with distance compensation
        currentMuzzleFlash.transform.localScale = Vector3.one * muzzleFlashScale * flashData.scaleFactor;

        // Randomize Z rotation only
        Vector3 currentRotation = currentMuzzleFlash.transform.eulerAngles;
        currentMuzzleFlash.transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, Random.Range(0, 360));

        // Start the flash timer
        muzzleFlashTimer = muzzleFlashDuration;
    }

    private (Vector3 position, float scaleFactor) GetValidFlashPositionAndScale(Vector3 desiredPosition, Vector3 startPosition)
    {
        Vector3 directionToFlash = (desiredPosition - startPosition).normalized;
        float desiredDistance = Vector3.Distance(startPosition, desiredPosition);

        // Raycast from camera position to desired position to check for walls
        RaycastHit hit;
        if (Physics.Raycast(startPosition, directionToFlash, out hit, desiredDistance, wallLayers))
        {
            // Wall detected - calculate safe distance
            float actualDistance = Mathf.Max(hit.distance - 0.1f, minFlashDistance);

            // Calculate the scale factor based on distance ratio
            float distanceRatio = actualDistance / desiredDistance;

            // Instead of just moving along the direction, scale the entire offset vector
            Vector3 originalOffset = desiredPosition - startPosition;
            Vector3 scaledOffset = originalOffset * distanceRatio;
            Vector3 finalPosition = startPosition + scaledOffset;

            // Calculate scale factor to maintain consistent visual size
            float scaleFactor = distanceRatio;

            return (finalPosition, scaleFactor);
        }

        // No wall detected - use desired position with normal scale
        return (desiredPosition, 1f);
    }

    private void HandleMuzzleFlash()
    {
        // Handle muzzle flash timer
        if (muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;

            // If timer expired, destroy the muzzle flash
            if (muzzleFlashTimer <= 0 && currentMuzzleFlash != null)
            {
                Destroy(currentMuzzleFlash);
                currentMuzzleFlash = null;
            }
        }
    }

    private void ApplyRecoil()
    {
        // Reset any existing recoil before applying new one
        currentRecoilOffset = Vector3.zero;
        recoilTimer = 0f;

        // Add immediate recoil impulse
        currentRecoilOffset = Vector3.back * recoilForce;
        recoilTimer = recoilDuration;
    }

    private void HandleRecoil()
    {
        // Don't handle recoil during reload animation
        if (isReloadAnimating) return;

        // Handle recoil timer
        if (recoilTimer > 0)
        {
            recoilTimer -= Time.deltaTime;
            // During recoil phase, stay at recoil position
            transform.localPosition = originalPosition + currentRecoilOffset;
        }
        else
        {
            // Always smoothly return to original position when not recoiling
            currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            transform.localPosition = originalPosition + currentRecoilOffset;
        }
    }

    private void HandleReloadAnimation()
    {
        if (isReloadAnimating)
        {
            Vector3 currentOffset = transform.localPosition - originalPosition;
            float animationSpeed = isSlideUp ? reloadMotionSpeed : reloadMotionSpeed;
            Vector3 newOffset = Vector3.Lerp(currentOffset, reloadTargetOffset, Time.deltaTime * animationSpeed);
            transform.localPosition = originalPosition + newOffset;

            // Check if we've finished the reload animation
            if (forceFinishReload && Vector3.Distance(newOffset, Vector3.zero) < 0.01f)
            {
                // Animation complete - reset everything properly
                isReloadAnimating = false;
                isSlideUp = false;
                reloadTargetOffset = Vector3.zero;
                forceFinishReload = false;

                // RESET RECOIL STATE COMPLETELY
                currentRecoilOffset = Vector3.zero;
                recoilTimer = 0f;

                // Make sure gun is back at original position
                transform.localPosition = originalPosition;
            }
        }
    }

    private void ResetShots()
    {
        readyToShoot = true;

        // Reset recoil when ready to shoot again (between bullets)
        if (!isReloadAnimating)
        {
            currentRecoilOffset = Vector3.zero;
            recoilTimer = 0f;
            transform.localPosition = originalPosition;
        }
    }

    private void Reload()
    {
        reloading = true;
        isReloadAnimating = true;
        isSlideUp = false; // Start with sliding down

        // Start reload animation - slide down
        reloadTargetOffset = Vector3.down * reloadSlideDistance;

        // Schedule gun to slide back up after exactly 1.8 seconds
        Invoke("StartReloadSlideUp", slideDownTime);
        Invoke("ReloadFinished", reloadTime);
    }

    private void StartReloadSlideUp()
    {
        // Switch to sliding up mode (slower speed)
        isSlideUp = true;
        // Slide back up to original position
        reloadTargetOffset = Vector3.zero;
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazingSize;
        reloading = false;

        // Signal that reload should finish smoothly
        forceFinishReload = true;
    }
}