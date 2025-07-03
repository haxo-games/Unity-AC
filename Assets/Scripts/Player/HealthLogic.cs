using System;
using UnityEngine;

public class HealthLogic : MonoBehaviour
{
    [SerializeField] AudioClip[] dieClipsArray;
    [SerializeField] AudioClip[] painClipsArray;

    public int health = 100;
    public int armor = 0;
    public event Action<int> OnDamage;


    public int takeDamage(int damage)
    {
        int finalDamage = damage;

        if (armor > 0)
        {
            finalDamage = damage - armor;
            armor = Math.Max(armor - damage, 0);
        }

        health = Math.Max(health - finalDamage, 0);
        OnDamage?.Invoke(health);

        if (health == 0)
        {
            int randomClipIndex = UnityEngine.Random.Range(0, dieClipsArray.Length);
            SfxManager.instance.PlaySound(dieClipsArray[randomClipIndex], transform, 0.4f);
        }
        else
        {
            int randomClipIndex = UnityEngine.Random.Range(0, painClipsArray.Length);
            SfxManager.instance.PlaySound(painClipsArray[randomClipIndex], transform, 0.4f);
        }

        return health;
    }
}
