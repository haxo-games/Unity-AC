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
    public float playerRecoilForce = 0.1f;
    public float playerRecoilDuration = 0.2f;
    public float playerRecoilRecoverySpeed = 10f;

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

    // Camera shake
    [Header("Camera Shake Settings")]
    [SerializeField] private CamShake camShake; //
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.3f;
    [SerializeField] private float screenKick = 1f;


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

        currentPlayerRecoilOffset = Vector3.zero;
        playerRecoilTimer = 0f;

        FindPlayerReferences();

        if (camShake == null && fpsCam != null)
        {
            camShake = fpsCam.GetComponent<CamShake>();
        }

        if (camShake == null)
        {
            Debug.LogWarning("CamShake component not found! Camera shake won't work.");
        }
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<CharacterController>();
        }
        else
        {
            playerController = Object.FindFirstObjectByType<CharacterController>();
            
            if (playerController != null)
                playerTransform = playerController.transform;
            
        }

        if (playerTransform != null)
            playerOriginalPosition = playerTransform.position;
        
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

        
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) // full auto 
            Shoot();
    }

    private void Shoot()
{
    readyToShoot = false;

    if (audioSource != null && shootSound != null)
        audioSource.PlayOneShot(shootSound, shootSoundVolume);

    ApplyRecoil();
    ApplyPlayerRecoil();
    ShowMuzzleFlash();
    
    if (camShake != null)
        {
            StartCoroutine(camShake.Shake(shakeDuration, shakeMagnitude));
        }

    if (fpsCam == null) // Error check while changing scenes can be deleted later
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
                healthLogic.takeDamage(damage);
            }
            else // Null check
            {
                Debug.LogWarning($"Enemy {rayHit.collider.name} doesn't have a HealthLogic component!");
            }
        }
    }

    bulletsLeft--;
    bulletsShot++;

    Invoke("ResetShots", timeBetweenShots);
}

    private void ApplyPlayerRecoil()
    {
        if (playerTransform == null || fpsCam == null) return;

        Vector3 recoilDirection = -fpsCam.transform.forward;

        recoilDirection.y = 0;
        recoilDirection = recoilDirection.normalized;

        Vector3 recoilMovement = recoilDirection * playerRecoilForce;

        if (playerController != null)
            playerController.Move(recoilMovement);
        
        else
            playerTransform.position += recoilMovement;

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
        bulletsLeft = magazingSize;
        reloading = false;

        forceFinishReload = true;
    }
}