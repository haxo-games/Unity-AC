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
    public float recoilForce = 0.05f;
    public float recoilDuration = 0.1f;
    public float recoilRecoverySpeed = 25f;

    // Reload Animation Settings
    [Header("Reload Animation Settings")]
    public float reloadSlideDistance = 1f;
    public float reloadMotionSpeed = 8f;
    public float slideDownTime = 1.5f; // Time to stay down before sliding up

    // Muzzle Flash Settings
    [Header("Muzzle Flash Settings")]
    public Texture2D muzzleFlashTexture; // Drag and drop your muzzle flash image here
    public float muzzleFlashDuration = 0.1f;
    public float muzzleFlashSize = 50f;
    public float muzzleFlashRandomRotation = 360f;
    public Vector2 muzzleFlashOffset = new Vector2(150f, -100f); // X: right, Y: down

    [Header("Background Removal Settings")]
    public bool removeBlackBackground = true; // Enable automatic black removal
    [Range(0f, 1f)]
    public float blackThreshold = 0.1f; // How dark before we make it transparent
    public bool useSmoothTransition = true; // Creates smooth edges instead of sharp cutoff

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

    // Muzzle Flash Variables
    private GameObject muzzleFlashUI;
    private UnityEngine.UI.Image muzzleFlashImage;
    private float muzzleFlashTimer;
    private bool isMuzzleFlashActive;
    private Texture2D processedMuzzleFlashTexture; // Store the processed texture

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

        // Process muzzle flash texture if needed
        ProcessMuzzleFlashTexture();

        // Initialize muzzle flash
        InitializeMuzzleFlash();
    }

    private void ProcessMuzzleFlashTexture()
    {
        if (muzzleFlashTexture == null || !removeBlackBackground)
            return;

        // Check if texture is readable
        if (!muzzleFlashTexture.isReadable)
        {
            Debug.LogError("Muzzle flash texture must be readable! Go to texture import settings and enable 'Read/Write Enabled'");
            return;
        }

        // Create a new texture with transparency support
        processedMuzzleFlashTexture = new Texture2D(muzzleFlashTexture.width, muzzleFlashTexture.height, TextureFormat.RGBA32, false);

        // Get all pixels from source
        Color[] pixels = muzzleFlashTexture.GetPixels();

        // Process each pixel
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            // Calculate how dark this pixel is (luminance)
            float darkness = (pixel.r + pixel.g + pixel.b) / 3f;

            if (useSmoothTransition)
            {
                // Smooth transition method
                if (darkness <= blackThreshold)
                {
                    // Completely transparent for very dark pixels
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f);
                }
                else if (darkness <= blackThreshold * 2f)
                {
                    // Partially transparent for slightly brighter pixels (smooth transition)
                    float alpha = (darkness - blackThreshold) / blackThreshold;
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, alpha);
                }
                else
                {
                    // Full opacity for bright pixels
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 1f);
                }
            }
            else
            {
                // Sharp cutoff method
                if (darkness <= blackThreshold)
                {
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f); // Fully transparent
                }
                else
                {
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 1f); // Keep original with full alpha
                }
            }
        }

        // Apply the processed pixels
        processedMuzzleFlashTexture.SetPixels(pixels);
        processedMuzzleFlashTexture.Apply();

        Debug.Log("Muzzle flash texture processed - black background removed!");
    }

    private void InitializeMuzzleFlash()
    {
        // Create a Canvas if one doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MuzzleFlash Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Make sure it's on top
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create muzzle flash UI object
        muzzleFlashUI = new GameObject("MuzzleFlash");
        muzzleFlashUI.transform.SetParent(canvas.transform, false);

        // Add Image component
        muzzleFlashImage = muzzleFlashUI.AddComponent<UnityEngine.UI.Image>();

        // Use the processed texture if available, otherwise use original
        Texture2D textureToUse = processedMuzzleFlashTexture != null ? processedMuzzleFlashTexture : muzzleFlashTexture;

        if (textureToUse != null)
        {
            // Create sprite from the texture
            Sprite muzzleFlashSprite = Sprite.Create(textureToUse,
                new Rect(0, 0, textureToUse.width, textureToUse.height),
                new Vector2(0.5f, 0.5f));
            muzzleFlashImage.sprite = muzzleFlashSprite;
        }
        else
        {
            Debug.LogWarning("No muzzle flash texture assigned! Please drag and drop a texture in the inspector.");
            Debug.LogWarning("Creating a simple circular muzzle flash as placeholder");

            // Create a simple circular flash as fallback
            CreateSimpleMuzzleFlash();
        }

        // Set up RectTransform for positioning
        RectTransform rectTransform = muzzleFlashUI.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = muzzleFlashOffset; // Position offset from center
        rectTransform.sizeDelta = new Vector2(muzzleFlashSize, muzzleFlashSize);

        // Initially hide the muzzle flash
        muzzleFlashUI.SetActive(false);
        isMuzzleFlashActive = false;
        muzzleFlashTimer = 0f;
    }

    private void CreateSimpleMuzzleFlash()
    {
        // Create a simple circular texture as fallback
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance) / radius);

                // Create a hot center that fades to orange/red edges
                Color flashColor;
                if (alpha > 0.7f)
                {
                    flashColor = Color.white; // Hot white center
                }
                else if (alpha > 0.3f)
                {
                    flashColor = Color.yellow; // Yellow middle
                }
                else
                {
                    flashColor = new Color(1f, 0.5f, 0f); // Orange edges
                }

                colors[y * size + x] = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        muzzleFlashImage.sprite = sprite;
    }

    private void Update()
    {
        MyInput();
        HandleRecoil();
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

        // Trigger muzzle flash
        TriggerMuzzleFlash();

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

    private void TriggerMuzzleFlash()
    {
        if (muzzleFlashUI != null)
        {
            // Show muzzle flash
            muzzleFlashUI.SetActive(true);
            isMuzzleFlashActive = true;
            muzzleFlashTimer = muzzleFlashDuration;

            // Randomize rotation for variety
            float randomRotation = Random.Range(0f, muzzleFlashRandomRotation);
            muzzleFlashUI.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

            // Optional: Randomize size slightly for more dynamic effect
            float sizeVariation = Random.Range(0.8f, 1.2f);
            RectTransform rectTransform = muzzleFlashUI.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(muzzleFlashSize * sizeVariation, muzzleFlashSize * sizeVariation);
        }
    }

    private void HandleMuzzleFlash()
    {
        if (isMuzzleFlashActive && muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;

            // Fade out the muzzle flash
            if (muzzleFlashImage != null)
            {
                float alpha = muzzleFlashTimer / muzzleFlashDuration;
                Color color = muzzleFlashImage.color;
                color.a = alpha;
                muzzleFlashImage.color = color;
            }

            // Hide muzzle flash when timer expires
            if (muzzleFlashTimer <= 0)
            {
                muzzleFlashUI.SetActive(false);
                isMuzzleFlashActive = false;

                // Reset alpha for next use
                if (muzzleFlashImage != null)
                {
                    Color color = muzzleFlashImage.color;
                    color.a = 1f;
                    muzzleFlashImage.color = color;
                }
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

    // Public method to manually reprocess the muzzle flash texture
    [ContextMenu("Reprocess Muzzle Flash")]
    public void ReprocessMuzzleFlash()
    {
        ProcessMuzzleFlashTexture();

        // Update the existing muzzle flash image with the new processed texture
        if (muzzleFlashImage != null && processedMuzzleFlashTexture != null)
        {
            Sprite newSprite = Sprite.Create(processedMuzzleFlashTexture,
                new Rect(0, 0, processedMuzzleFlashTexture.width, processedMuzzleFlashTexture.height),
                new Vector2(0.5f, 0.5f));
            muzzleFlashImage.sprite = newSprite;
            Debug.Log("Muzzle flash updated with reprocessed texture!");
        }
    }
}