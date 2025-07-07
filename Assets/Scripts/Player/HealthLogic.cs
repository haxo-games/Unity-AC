using System;
using UnityEngine;

public class HealthLogic : MonoBehaviour
{
    [SerializeField] AudioClip[] dieClipsArray;
    [SerializeField] AudioClip[] painClipsArray;

    public int health = 100;
    public int armor = 0;
    public event Action<int, int> OnDamage;

    public int takeDamage(int damage)
    {
        int finalDamage = damage;

        if (armor > 0)
        {
            finalDamage = damage - armor;
            finalDamage = Math.Max(finalDamage, 0); // Prevent negative damage
            armor = Math.Max(armor - damage, 0);
        }

        health = Math.Max(health - finalDamage, 0);
        OnDamage?.Invoke(health, armor);

        // Play audio with safety checks
        if (health == 0)
        {
            if (dieClipsArray != null && dieClipsArray.Length > 0)
            {
                int randomClipIndex = UnityEngine.Random.Range(0, dieClipsArray.Length);
                if (SfxManager.instance != null)
                {
                    SfxManager.instance.PlaySound(dieClipsArray[randomClipIndex], transform, 0.4f);
                }
            }
        }
        else
        {
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
    }