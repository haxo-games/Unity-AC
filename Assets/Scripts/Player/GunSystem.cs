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

    // References
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    private void Awake()
    {
        bulletsLeft = magazingSize;
        readyToShoot = true;
    }

    private void Update()
    {
        MyInput();
    } 

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazingSize && !reloading) Reload();

        // Shooting Check
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        // Raycast Bullet
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"))
                rayHit.collider.GetComponent<MeshCollider>();
                //rayHit.collider.GetComponent<MeshCollider>().TakeDamage(damage);
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
