using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI interactText;
    [SerializeField] TextMeshProUGUI healthText;
    Animator interactTextAnimator;
    bool interactTextShown;
    HealthLogic playerHealthLogic;

    void Start()
    {
        interactTextAnimator = interactText.GetComponent<Animator>();
        playerHealthLogic = GetComponent<HealthLogic>();

        playerHealthLogic.OnDamage += newHealth => SetDisplayedHealth(newHealth);
    }

    void SetDisplayedHealth(int newHealth)
    {
        healthText.text = newHealth.ToString();
    }

    public void SetInteractText(string newInteractText)
    {
        interactTextShown = newInteractText != string.Empty;

        if (interactTextShown)
            interactText.text = newInteractText;

        interactTextAnimator.SetBool("isShown", interactTextShown);
    }
}
