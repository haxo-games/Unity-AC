using UnityEngine;

public class GameManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CursorInitalize()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}