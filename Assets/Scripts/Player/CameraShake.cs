
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    private Vector3 basePosition;
    private bool isShaking = false;

    void Start()
    {
        basePosition = transform.localPosition;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        if (isShaking) yield break; 
        
        isShaking = true;
        basePosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(basePosition.x, basePosition.y + y, basePosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = basePosition;
        isShaking = false;
    }   
}