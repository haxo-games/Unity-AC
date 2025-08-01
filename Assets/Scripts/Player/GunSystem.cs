using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    // Gun Stats
    public int damage;
    public float fireRate, spread, range, reloadTime, timeBetweenShots;
    public int magazingSize, reserveSize, bulletperTap;
    public bool allowButtonHold;
    public int bulletsLeft, bulletsShot;

    // Recoil Settings
    [Header("Recoil Settings")]
    public float recoilForce = 0.0f;
    public float recoilDuration = 0.1f;
    public float recoilRecoverySpeed = 25f;

    [Header("Camera Recoil Settings")]
    public float recoilUpAmount = 2.5f;
    public float recoilSettleAmount = 0.2f;
    public float recoilSettleSpeed = 3f;
    public float recoilBuildupSpeed = 8f;

    // Reload Animation Settings
    [Header("Reload Animation Settings")]
    public float reloadSlideDistance = 1f;
    public float reloadMotionSpeed = 8f;
    public float slideDownTime = 1.5f;

    // Muzzle Flash Settings
    [Header("Muzzle Flash Settings")]
    public GameObject muzzleFlashPrefab;
    public float muzzleFlashDuration = 0.05f;
    public float muzzleFlashDistance = 2f;
    public Vector3 muzzleFlashOffset = new Vector3(0, 0, 0);
    public float muzzleFlashScale = 1f;
    public LayerMask wallLayers = -1;
    public float minFlashDistance = 0.3f;

    // Input System Variables
    [Header("Input Settings")]
    public InputActionReference shootAction;
    public InputActionReference reloadAction;

    // Cases
    bool shooting, readyToShoot, reloading;

    // Recoil Variables
    private Vector3 originalPosition;
    private Vector3 currentRecoilOffset;
    private float recoilTimer;

    // Simple recoil system
    private Vector3 currentCameraRecoil = Vector3.zero;
    private Vector3 targetCameraRecoil = Vector3.zero;
    private Vector3 recoilVelocity = Vector3.zero;
    private bool wasShooting = false;

    // Reload Animation Variables
    private Vector3 reloadTargetOffset;
    private bool isReloadAnimating;
    private bool isSlideUp;
    private bool forceFinishReload;

    // Muzzle Flash Variables
    private GameObject currentMuzzleFlash;
    private float muzzleFlashTimer;

    // References
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    // Player Reference (for player recoil)
    private PlayerMovement PlayerMovement;
    private PlayerLook playerLook;

    // UI Reference
    private FPSUIManager uiManager;

    // Audio
    private AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    [Range(0f, 1f)]
    public float shootSoundVolume = 0.1f;
    [Range(0f, 1f)]
    public float reloadSoundVolume = 0.1f;
    private bool isShootSoundPlaying = false;

    private bool justActivated = false;
    private float activationTime = 0f;

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

        FindPlayerReferences();
        FindUIManager();
    }

    private void OnEnable()
    {
        // Enable input actions
        if (shootAction != null)
            shootAction.action.Enable();
        if (reloadAction != null)
            reloadAction.action.Enable();

        justActivated = true;
        activationTime = Time.time;

        currentRecoilOffset = Vector3.zero;
        recoilTimer = 0f;
        transform.localPosition = originalPosition;

        ResetCameraRecoil();

        isReloadAnimating = false;
        isSlideUp = false;
        reloadTargetOffset = Vector3.zero;
        forceFinishReload = false;

        if (uiManager != null)
        {
            FPSUIManager.UpdateAmmo(bulletsLeft, reserveSize);
        }
        else
        {
            FindUIManager();
            if (uiManager != null)
            {
                FPSUIManager.UpdateAmmo(bulletsLeft, reserveSize);
            }
        }
    }

    private void OnDisable()
    {
        reloading = false;

        // Disable input actions
        if (shootAction != null)
            shootAction.action.Disable();
        if (reloadAction != null)
            reloadAction.action.Disable();
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerMovement = playerObj.GetComponent<PlayerMovement>();
            playerLook = playerObj.GetComponent<PlayerLook>();
        }
        else
        {
            PlayerMovement = Object.FindFirstObjectByType<PlayerMovement>();

            if (PlayerMovement != null)
                playerLook = PlayerMovement.GetComponent<PlayerLook>();
        }

        if (playerLook == null)
            playerLook = Object.FindFirstObjectByType<PlayerLook>();
    }

    private void FindUIManager()
    {
        uiManager = Object.FindFirstObjectByType<FPSUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("FPSUIManager not found! Ammo UI won't update.");
        }
    }

    private void Update()
    {
        MyInput();
        HandleRecoil();
        HandleCameraRecoil();
        HandleReloadAnimation();
        HandleMuzzleFlash();
    }

    private void MyInput()
    {
        if (justActivated && Time.time - activationTime < 0.1f)
        {
            shooting = false;
        }
        else
        {
            // New Input System implementation
            if (shootAction != null)
            {
                if (allowButtonHold)
                    shooting = shootAction.action.IsPressed();
                else
                    shooting = shootAction.action.WasPressedThisFrame();
            }
            else
            {
                // Fallback to old input system if no InputActionReference is assigned
                if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
                else shooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Time.time - activationTime >= 0.1f)
                justActivated = false;
        }

        // Handle reload input
        bool reloadPressed = false;
        if (reloadAction != null)
        {
            reloadPressed = reloadAction.action.WasPressedThisFrame();
        }
        else
        {
            reloadPressed = Input.GetKeyDown(KeyCode.R);
        }

        if (reloadPressed && bulletsLeft < magazingSize && !reloading && reserveSize > 0 && gameObject.activeSelf)
        {
            Reload();
            if (audioSource != null && reloadSound != null)
            {
                audioSource.Stop();
                isShootSoundPlaying = false;

                audioSource.clip = reloadSound;
                audioSource.volume = reloadSoundVolume;
                audioSource.Play();
            }
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) // full auto 
            Shoot();
    }

    private void Shoot()
    {
        readyToShoot = false;

        if (audioSource != null && shootSound != null)
        {
            if (isShootSoundPlaying)
            {
                audioSource.Stop();
            }

            // Play the new shoot sound
            audioSource.clip = shootSound;
            audioSource.volume = shootSoundVolume;
            audioSource.Play();
            isShootSoundPlaying = true;

            Invoke("ResetShootSoundFlag", shootSound.length);
        }

        ApplyRecoil();
        ApplyPlayerRecoil();
        ApplyCameraRecoil();
        ShowMuzzleFlash();

        if (fpsCam == null) // Error check while changing scenes can be deleted later nts
        {
            Debug.LogError("FPS Camera is not assigned!");
            return;
        }

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"))
            {
                HealthLogic healthLogic = rayHit.collider.GetComponent<HealthLogic>();
                if (healthLogic != null)
                {
                    healthLogic.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning($"Enemy {rayHit.collider.name} doesn't have a HealthLogic component!");
                }
            }
        }

        bulletsLeft--;

        if (uiManager != null)
        {
            FPSUIManager.UpdateAmmo(bulletsLeft, reserveSize);
        }
        bulletsShot++;

        Invoke("ResetShots", timeBetweenShots);
    }

    private void ApplyPlayerRecoil()
    {
        if (PlayerMovement != null && fpsCam != null)
        {
            PlayerMovement.ApplyPlayerRecoil(fpsCam);
        }
    }

    private void ApplyCameraRecoil()
    {
        if (playerLook == null)
        {
            Debug.LogWarning("PlayerLook component not found! Camera recoil won't work.");
            return;
        }

        // Add recoil up per shot
        targetCameraRecoil += new Vector3(-recoilUpAmount, 0, 0);
    }

    public void ResetCameraRecoil()
    {
        if (playerLook != null)
        {
            currentCameraRecoil = Vector3.zero;
            targetCameraRecoil = Vector3.zero;
            recoilVelocity = Vector3.zero;
            wasShooting = false;
        }
    }

    private void HandleCameraRecoil()
    {
        if (playerLook == null) return;

        Vector3 currentRecoilOffset = playerLook.GetRecoilOffset();

        if (wasShooting && !shooting)
        {
            // Add upward recoil when shot ends
            targetCameraRecoil += new Vector3(2.3f, 0, 0);
        }

        wasShooting = shooting;

        Vector3 newRecoilOffset = Vector3.SmoothDamp(currentRecoilOffset, targetCameraRecoil, ref recoilVelocity, 0.1f);

        playerLook.SetRecoilOffset(newRecoilOffset);
    }

    private void ShowMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || fpsCam == null) return;

        // Remove any existing muzzle flash
        if (currentMuzzleFlash != null)
            Destroy(currentMuzzleFlash);

        Vector3 cameraPosition = fpsCam.transform.position;
        Vector3 forwardDirection = fpsCam.transform.forward;
        Vector3 rightDirection = fpsCam.transform.right;
        Vector3 upDirection = fpsCam.transform.up;

        Vector3 offsetInWorldSpace = rightDirection * muzzleFlashOffset.x +
                                    upDirection * muzzleFlashOffset.y +
                                    forwardDirection * muzzleFlashOffset.z;

        Vector3 desiredPosition = cameraPosition + forwardDirection * muzzleFlashDistance + offsetInWorldSpace;

        var flashData = GetValidFlashPositionAndScale(desiredPosition, cameraPosition);

        currentMuzzleFlash = Instantiate(muzzleFlashPrefab, flashData.position, fpsCam.transform.rotation);

        currentMuzzleFlash.transform.SetParent(fpsCam.transform);

        currentMuzzleFlash.transform.localScale = Vector3.one * muzzleFlashScale * flashData.scaleFactor;

        Vector3 currentRotation = currentMuzzleFlash.transform.eulerAngles;
        currentMuzzleFlash.transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, Random.Range(0, 360));

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

            float distanceRatio = actualDistance / desiredDistance;

            Vector3 originalOffset = desiredPosition - startPosition;
            Vector3 scaledOffset = originalOffset * distanceRatio;
            Vector3 finalPosition = startPosition + scaledOffset;

            float scaleFactor = distanceRatio;

            return (finalPosition, scaleFactor);
        }

        return (desiredPosition, 1f);
    }

    private void HandleMuzzleFlash()
    {
        if (muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0 && currentMuzzleFlash != null)
            {
                Destroy(currentMuzzleFlash);
                currentMuzzleFlash = null;
            }
        }
    }

    private void ApplyRecoil()
    {
        currentRecoilOffset = Vector3.zero;
        recoilTimer = 0f;

        currentRecoilOffset = Vector3.back * recoilForce;
        recoilTimer = recoilDuration;
    }

    private void HandleRecoil()
    {
        if (isReloadAnimating) return;

        if (recoilTimer > 0)
        {
            recoilTimer -= Time.deltaTime;
            transform.localPosition = originalPosition + currentRecoilOffset;
        }
        else
        {
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

            if (forceFinishReload && Vector3.Distance(newOffset, Vector3.zero) < 0.01f)
            {
                isReloadAnimating = false;
                isSlideUp = false;
                reloadTargetOffset = Vector3.zero;
                forceFinishReload = false;

                currentRecoilOffset = Vector3.zero;
                recoilTimer = 0f;

                transform.localPosition = originalPosition;
            }
        }
    }

    private void ResetShots()
    {
        readyToShoot = true;

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
        isSlideUp = false;

        reloadTargetOffset = Vector3.down * reloadSlideDistance;

        Invoke("StartReloadSlideUp", slideDownTime);
        Invoke("ReloadFinished", reloadTime);
    }

    private void StartReloadSlideUp()
    {
        isSlideUp = true;
        reloadTargetOffset = Vector3.zero;
    }

    private void ReloadFinished()
    {
        int bulletsNeeded = magazingSize - bulletsLeft;

        int bulletsToReload = Mathf.Min(bulletsNeeded, reserveSize);

        bulletsLeft += bulletsToReload;
        reserveSize -= bulletsToReload;

        if (uiManager != null)
        {
            FPSUIManager.UpdateAmmo(bulletsLeft, reserveSize);
        }

        reloading = false;
        forceFinishReload = true;
    }

    private void ResetShootSoundFlag()
    {
        isShootSoundPlaying = false;
    }

    // For when we add pickUp able ammo like the AC counterpart
    public void AddReserveAmmo(int amount)
    {
        reserveSize += amount;
        if (uiManager != null)
        {
            FPSUIManager.UpdateAmmo(bulletsLeft, reserveSize);
        }
    }

    public int GetReserveAmmo()
    {
        return reserveSize;
    }
}