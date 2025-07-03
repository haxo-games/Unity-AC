using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI armorText;
    [SerializeField] TextMeshProUGUI interactText;
    Animator interactTextAnimator;
    bool interactTextShown;
    HealthLogic playerHealthLogic;

    void Start()
    {
        interactTextAnimator = interactText.GetComponent<Animator>();
        playerHealthLogic = GetComponent<HealthLogic>();

        playerHealthLogic.OnDamage += HandleDamage;
    }

    void HandleDamage(int newHealth, int newArmor)
    {
        healthText.text = newHealth.ToString();
        armorText.text = newArmor.ToString();
    }

    public void SetInteractText(string newInteractText)
    {
        interactTextShown = newInteractText != string.Empty;

        if (interactTextShown)
            interactText.text = newInteractText;

        interactTextAnimator.SetBool("isShown", interactTextShown);
    }
}
