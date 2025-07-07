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

    // Reload Animation Settings
    [Header("Reload Animation Settings")]
    public float reloadSlideDistance = 1f;
    public float reloadMotionSpeed = 8f;
    public float slideDownTime = 1.5f; // Time to stay down before sliding up

    // Cases
    bool shooting, readyToShoot, reloading;

    // Recoil Variables
    private Vector3 originalPosition;
    private Vector3 currentRecoilOffset;
    private float recoilTimer;

    // Reload Animation Variables
    private Vector3 reloadTargetOffset;
    private bool isReloadAnimating;
    private bool isSlideUp; // Track if we're sliding up or down
    private bool forceFinishReload; // Force reload to finish smoothly

    // References
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

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
    }

    private void Update()
    {
        MyInput();
        HandleRecoil();
        HandleReloadAnimation();
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