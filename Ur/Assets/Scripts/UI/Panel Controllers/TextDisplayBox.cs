using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDisplayBox : MenuPanelAnimator
{
    public Vector2 textPadding;

    [SerializeField] private Text messageText = null;
    [SerializeField] private RectTransform rect = null;
    [SerializeField] private Button confirmButton = null;
    [SerializeField] private Button closeButton = null;

    public event Action ConfirmButtonClicked;
    public event Action CloseButtonClicked;

    private void Awake() {
        rect = GetComponent<RectTransform>();

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    void OnConfirmButtonClicked()
    {
        ConfirmButtonClicked?.Invoke();
        EnableAnimation(false);
    }

    void OnCloseButtonClicked()
    {
        CloseButtonClicked?.Invoke();
        EnableAnimation(false);
    }

    public void DisplayMessage(string message, Vector2 anchorMin, Vector2 anchorMax) {
        if (rect == null) {
            rect = GetComponent<RectTransform>();
        }
		EnableAnimation(true);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = Vector2.zero;
        messageText.text = message;
    }
}
