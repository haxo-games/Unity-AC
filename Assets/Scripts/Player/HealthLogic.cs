using System;
using UnityEngine;

public class HealthLogic : MonoBehaviour
{
    [SerializeField] AudioClip[] dieClipsArray;
    [SerializeField] AudioClip[] painClipsArray;
    
    [Header("Death Settings")]
    [SerializeField] float deathDelay = 0.2f; // Time for death, like when kill until deletion
    [SerializeField] bool useDeathDelay = true; 

    public int health = 100;
    public int armor = 0;
    public event Action<int, int> OnDamage;
    public event Action OnDeath; 

    private bool isDead = false;

    public int takeDamage(int damage)
    {
        if (isDead) return health; // Don't take damage if already dead

        int finalDamage = damage;

        if (armor > 0)
        {
            finalDamage = damage - armor;
            finalDamage = Math.Max(finalDamage, 0); // Prevent negative damage
            armor = Math.Max(armor - damage, 0);
        }

        health = Math.Max(health - finalDamage, 0);
        OnDamage?.Invoke(health, armor);

        if (health == 0 && !isDead)
        {
            Die();
        }
        else if (health > 0)
        {
            // Play pain sound
            if (painClipsArray != null && painClipsArray.Length > 0)
            {
                int randomClipIndex = UnityEngine.Random.Range(0, painClipsArray.Length);
                if (SfxManager.instance != null)
                {
                    SfxManager.instance.PlaySound(painClipsArray[randomClipIndex], transform, 0.4f);
                }
            }
        }

        return health;
    }

    private void Die()
    {
        isDead = true;
        
        // death sounds
        if (dieClipsArray != null && dieClipsArray.Length > 0)
        {
            int randomClipIndex = UnityEngine.Random.Range(0, dieClipsArray.Length);
            if (SfxManager.instance != null)
            {
                SfxManager.instance.PlaySound(dieClipsArray[randomClipIndex], transform, 0.4f);
            }
        }
        OnDeath?.Invoke();

        
        DisableEntityComponents(); // when dead disable comps

        // kill entities
        if (useDeathDelay)
        {
            Destroy(gameObject, deathDelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DisableEntityComponents()
    {
        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable AI components if we have any in the future
        var aiComponents = GetComponentsInChildren<MonoBehaviour>();
        foreach (var component in aiComponents)
        {
            if (component != this && component.GetType().Name.Contains("AI"))
            {
                component.enabled = false;
            }
        }

        // Disable Rigidbody animations if we add any in the future
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
}