using UnityEngine;
using TMPro;
using System;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI interactText;
    Animator interactTextAnimator;
    bool interactTextShown;

    void Start()
    {
        interactTextAnimator = interactText.GetComponent<Animator>();
    }

    public void SetInteractText(string newInteractText)
    {
        interactTextShown = newInteractText != string.Empty;

        if (interactTextShown)
            interactText.text = newInteractText;

        interactTextAnimator.SetBool("isShown", interactTextShown);
    }
}
