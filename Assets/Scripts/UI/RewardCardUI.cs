using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardNameText;
    [SerializeField] private TMP_Text rewardTypeText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button selectButton;

    public void Setup(Sprite icon, string rewardName, string rewardType, string description, string buttonLabel, Action onSelected, bool isSelectable = true)
    {
        rewardIcon.sprite = icon;
        rewardIcon.enabled = icon != null;

        rewardNameText.text = rewardName;
        rewardTypeText.text = rewardType;
        descriptionText.text = description;
        buttonText.text = buttonLabel;

        selectButton.interactable = isSelectable;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelected?.Invoke());
    }
}