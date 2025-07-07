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

    // Cases
    bool shooting, readyToShoot, reloading;
    bool mousePressed; // Track if mouse was just pressed

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
    }

    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        mousePressed = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazingSize && !reloading)
        {
            Reload();
            if (audioSource != null && reloadSound != null)
                audioSource.PlayOneShot(reloadSound, reloadSoundVolume);
            
        }

        if (mousePressed)
            if (audioSource != null && shootSound != null)
                audioSource.PlayOneShot(shootSound, shootSoundVolume);

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            Shoot();
        
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Raycast Bullet
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);
            if (rayHit.collider.CompareTag("Enemy"))
                rayHit.collider.GetComponent<HealthLogic>().takeDamage(damage);
        }

        bulletsLeft--;
        Invoke("ResetShots", timeBetweenShots);
    }

    private void ResetShots()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazingSize;
        reloading = false;
    }
}