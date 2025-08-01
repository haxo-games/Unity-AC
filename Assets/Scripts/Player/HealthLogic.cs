using System;
using System.Collections;
using UnityEngine;

public class HealthLogic : MonoBehaviour
{
    [SerializeField] AudioClip[] dieClipsArray;
    [SerializeField] AudioClip[] painClipsArray;

    [Header("Death Settings")]
    [SerializeField] float deathDelay = 2; // Time for death, like when kill until deletion
    [SerializeField] bool useDeathDelay = true;

    [Header("Player Respawn Settings")]
    [SerializeField] float respawnDelay = 0f; // Set to 0 for instant respawn
    [SerializeField] Transform spawnPoint; // Optional custom spawn point

    public int health = 100;
    public int armor = 0;
    public event Action<int, int> OnDamage;
    public event Action OnDeath;
    public event Action OnRespawn;

    private bool isDead = false;
    private int maxHealth;
    private int maxArmor;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Store initial values for respawn
        maxHealth = health;
        maxArmor = armor;
        
        // Store spawn position (use custom spawn point if set, otherwise current position)
        if (spawnPoint != null)
        {
            initialPosition = spawnPoint.position;
            initialRotation = spawnPoint.rotation;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
    }

    public int TakeDamage(int damage)
    {
        if (isDead) return health;

        int piercing = 0; // Later change this based on weapon that inflicted damage through people yk what piercing is
        int activeDamage = damage;

        if (armor <= 25)
            activeDamage = (int)(16.0f / 25.0f * armor);
        else if (armor <= 25)
            activeDamage = (int)(17.0f / 25.0f * armor) - 1;
        else if (armor <= 50)
            activeDamage = (int)(4.0f / 25.0f * armor) + 25;
        else if (armor <= 75)
            activeDamage = (int)(4.0f / 25.0f * armor) + 25;

        int reducedArmor = (int)(activeDamage * damage / 100.0f);
        int reducedDamage = (int)(reducedArmor - (reducedArmor * (piercing / 100.0f)));

        armor -= reducedArmor;
        damage -= reducedDamage;

        health -= damage;

        
        if (gameObject.CompareTag("Player"))
        {
            FPSUIManager.UpdateHealth(health);
            FPSUIManager.UpdateShield(armor);
        }

        OnDamage?.Invoke(health, armor);

        /* Handle death or pain sound */
        if (health <= 0 && !isDead)
            Die();
        else if (health > 0 && painClipsArray != null && painClipsArray.Length > 0)
        {
            int randomClipIndex = UnityEngine.Random.Range(0, painClipsArray.Length);
            if (SfxManager.instance != null)
                SfxManager.instance.PlaySound(painClipsArray[randomClipIndex], transform, 0.4f);
        }

        return damage;
    }

    public void TakeDirectDamage(int healthDamage, int armorDamage)
    {
        if (isDead) return;

        // Apply damage directly without complex armor calculations
        health -= healthDamage;
        armor -= armorDamage;

        // Ensure armor doesn't go below 0
        if (armor < 0) armor = 0;

        if (gameObject.CompareTag("Player"))
        {
            FPSUIManager.UpdateHealth(health);
            FPSUIManager.UpdateShield(armor);
        }

        OnDamage?.Invoke(health, armor);

        /* Handle death or pain sound */
        if (health <= 0 && !isDead)
            Die();
        else if (health > 0 && painClipsArray != null && painClipsArray.Length > 0)
        {
            int randomClipIndex = UnityEngine.Random.Range(0, painClipsArray.Length);
            if (SfxManager.instance != null)
                SfxManager.instance.PlaySound(painClipsArray[randomClipIndex], transform, 0.4f);
        }
    }

    private void Die()
    {
        isDead = true;

        if (dieClipsArray != null && dieClipsArray.Length > 0)
        {
            int randomClipIndex = UnityEngine.Random.Range(0, dieClipsArray.Length);
            if (SfxManager.instance != null)
                SfxManager.instance.PlaySound(dieClipsArray[randomClipIndex], transform, 0.4f);
        }

        OnDeath?.Invoke();

        // Check if this is the player - respawn instead of destroy
        if (gameObject.CompareTag("Player"))
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            DisableEntityComponents();
            if (useDeathDelay)
                Destroy(gameObject, deathDelay);
            else
                Destroy(gameObject);
        }
    }

    private void DisableEntityComponents()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (var collider in colliders)
            collider.enabled = false;

        var aiComponents = GetComponentsInChildren<MonoBehaviour>();

        foreach (var component in aiComponents)
        {
            if (component != this && component.GetType().Name.Contains("AI"))
                component.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    private IEnumerator RespawnCoroutine()
    {
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        // Reset position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Reset health and armor
        health = maxHealth;
        armor = maxArmor;
        isDead = false;

        // Update UI
        FPSUIManager.UpdateHealth(health);
        FPSUIManager.UpdateShield(armor);

        OnRespawn?.Invoke();
        Debug.Log("Player respawned!");
    }
}