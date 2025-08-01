using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI armorText;
    [SerializeField] TextMeshProUGUI interactText;
    [SerializeField] GameObject mainMenu;
    // Animator interactTextAnimator;
    bool interactTextShown;
    HealthLogic playerHealthLogic;

    public bool isMainMenuOpen;

    void Start()
    {
        // interactTextAnimator = interactText.GetComponent<Animator>();
        playerHealthLogic = GetComponent<HealthLogic>();

        playerHealthLogic.OnDamage += HandleDamage;

        mainMenu.SetActive(isMainMenuOpen);
    }

    void HandleDamage(int newHealth, int newArmor)
    {
        if (healthText != null)
            healthText.text = newHealth.ToString();
        else
            Debug.LogWarning("PlayerUI: healthText is not assigned in the inspector!");

        if (armorText != null)
            armorText.text = newArmor.ToString();
        else
            Debug.LogWarning("PlayerUI: armorText is not assigned in the inspector!");
    }

    public void SetInteractText(string newInteractText)
    {
        interactTextShown = newInteractText != string.Empty;
        
        if (interactText != null)
            interactText.text = newInteractText;
        else
            Debug.LogWarning("PlayerUI: interactText is not assigned in the inspector!");

        // interactTextAnimator.SetBool("isShown", interactTextShown);
    }

    public void ToggleMainMenu()
    {
        isMainMenuOpen = !isMainMenuOpen;
        mainMenu.SetActive(isMainMenuOpen);
    }
}
