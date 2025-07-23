using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingDelay : MonoBehaviour
{
    public float delayTime = 2f;
    public string gameSceneName = "ShootingGround";

    void Start()
    {
        StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(gameSceneName);
    }
}