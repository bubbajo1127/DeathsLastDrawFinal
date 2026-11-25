using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSelectionUI : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerDoorInteraction playerDoorInteraction;

    [Header("Shared UI")]
    [SerializeField] private GameObject backgroundDimmer;
    [SerializeField] private RewardCardUI rewardCardPrefab;

    [Header("Initial Ability Selection")]
    [SerializeField] private GameObject initialAbilitySelectionPanel;
    [SerializeField] private Transform initialAbilityCardContainer;

    [Header("Door Reward Selection")]
    [SerializeField] private GameObject doorRewardSelectionPanel;
    [SerializeField] private Transform doorRewardCardContainer;
    [SerializeField] private Button cancelDoorButton;
    [SerializeField] private Sprite powerCategoryIcon;
    [SerializeField] private Sprite abilityCategoryIcon;

    [Header("Upgrade Selection")]
    [SerializeField] private GameObject upgradeSelectionPanel;
    [SerializeField] private Transform upgradeCardContainer;
    [SerializeField] private TMP_Text upgradeTitleText;
    [SerializeField] private TMP_Text upgradeSubtitleText;

    private readonly List<RewardCardUI> activeCards = new List<RewardCardUI>();

    private float previousTimeScale;
    private bool previousInputLock;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        backgroundDimmer.SetActive(false);
        initialAbilitySelectionPanel.SetActive(false);
        doorRewardSelectionPanel.SetActive(false);
        upgradeSelectionPanel.SetActive(false);
    }

    public bool ShowInitialAbilitySelection(PlayerAbilityManager abilityManager, Action<AbilityType> onAbilitySelected)
    {
        if (IsOpen || abilityManager == null)
            return false;

        List<RewardCardOption> options = new List<RewardCardOption>();

        foreach (AbilityType abilityType in Enum.GetValues(typeof(AbilityType)))
        {
            PlayerAbility ability = abilityManager.GetAbility(abilityType);

            if (ability == null || ability.Data == null)
            {
                Debug.LogError($"Missing ability configuration: {abilityType}");
                return false;
            }

            if (ability.IsUnlocked)
                continue;

            AbilityData data = ability.Data;

            options.Add(new RewardCardOption(
                data.AbilityIcon,
                data.AbilityName,
                "Level 1 - Unlock",
                data.GetLevelDescription(1),
                "Learn Ability",
                () => onAbilitySelected?.Invoke(abilityType)
            ));
        }

        return ShowCards(initialAbilitySelectionPanel, initialAbilityCardContainer, options);
    }

    public bool ShowDoorRewardSelection(bool canChooseAbility, Action<RewardCategory> onSelected, Action onCancelled)
    {
        if (IsOpen)
            return false;

        List<RewardCardOption> options = new List<RewardCardOption>();

        options.Add(new RewardCardOption(
            powerCategoryIcon,
            "Power Upgrade",
            "Next Room Reward",
            "Increase your stats and strengthen your character.",
            "Select",
            () => onSelected?.Invoke(RewardCategory.Power)
        ));

        options.Add(new RewardCardOption(
            abilityCategoryIcon,
            "Ability Upgrade",
            "Next Room Reward",
            canChooseAbility ? "Learn new abilities or improve your existing ones." : "All learned abilities are at maximum level.",
            canChooseAbility ? "Select" : "Unavailable",
            () => onSelected?.Invoke(RewardCategory.Ability),
            canChooseAbility
        ));

        cancelDoorButton.onClick.RemoveAllListeners();
        cancelDoorButton.onClick.AddListener(() => onCancelled?.Invoke());

        return ShowCards(doorRewardSelectionPanel, doorRewardCardContainer, options);
    }

    public bool ShowUpgradeSelection(RewardCategory category, List<RewardCardOption> options)
    {
        if (IsOpen)
            return false;

        if (category == RewardCategory.Power)
        {
            upgradeTitleText.text = "Choose a Power Upgrade";
            upgradeSubtitleText.text = "Strengthen yourself for the encounters ahead.";
        }
        else if (category == RewardCategory.Ability)
        {
            upgradeTitleText.text = "Choose an Ability Upgrade";
            upgradeSubtitleText.text = "Expand your arsenal or improve your abilities.";
        }
        else
        {
            return false;
        }

        return ShowCards(upgradeSelectionPanel, upgradeCardContainer, options);
    }

    private bool ShowCards(GameObject panel, Transform container, List<RewardCardOption> options)
    {
        if (IsOpen || options == null || options.Count == 0)
            return false;

        if (panel == null || container == null || rewardCardPrefab == null)
        {
            Debug.LogError("Reward UI is missing required references.");
            return false;
        }

        ClearCards();

        foreach (RewardCardOption option in options)
        {
            RewardCardUI card = Instantiate(rewardCardPrefab, container);

            card.Setup(
                option.Icon,
                option.Name,
                option.Type,
                option.Description,
                option.ButtonLabel,
                option.OnSelected,
                option.IsSelectable
            );

            activeCards.Add(card);
        }

        LockGameplay();

        backgroundDimmer.SetActive(true);
        panel.SetActive(true);

        return true;
    }

    public void Close()
    {
        initialAbilitySelectionPanel.SetActive(false);
        doorRewardSelectionPanel.SetActive(false);
        upgradeSelectionPanel.SetActive(false);
        backgroundDimmer.SetActive(false);

        ClearCards();
        UnlockGameplay();
    }

    private void LockGameplay()
    {
        if (IsOpen)
            return;

        previousTimeScale = Time.timeScale;
        previousInputLock = inputManager.IsGameplayInputLocked;

        IsOpen = true;

        inputManager.SetGameplayInputLocked(true);

        playerAttack.CancelAttack();
        playerDoorInteraction.CancelDoorInteraction();
        playerMovement.StopMovement();

        Time.timeScale = 0f;
    }

    private void UnlockGameplay()
    {
        if (!IsOpen)
            return;

        IsOpen = false;

        Time.timeScale = previousTimeScale;

        if (inputManager != null)
            inputManager.SetGameplayInputLocked(previousInputLock);
    }

    private void ClearCards()
    {
        foreach (RewardCardUI card in activeCards)
        {
            if (card == null)
                continue;

            card.gameObject.SetActive(false);
            Destroy(card.gameObject);
        }

        activeCards.Clear();
    }

    private void OnDisable()
    {
        UnlockGameplay();
    }
}