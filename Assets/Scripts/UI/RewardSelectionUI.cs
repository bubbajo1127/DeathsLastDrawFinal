using System;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly List<RewardCardUI> activeCards = new List<RewardCardUI>();

    private float previousTimeScale;
    private bool previousInputLock;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        backgroundDimmer.SetActive(false);
        initialAbilitySelectionPanel.SetActive(false);
    }

    public bool ShowInitialAbilitySelection(
        PlayerAbilityManager abilityManager,
        Action<AbilityType> onAbilitySelected)
    {
        if (IsOpen || abilityManager == null)
            return false;

        List<PlayerAbility> abilities = new List<PlayerAbility>();

        foreach (AbilityType abilityType in Enum.GetValues(typeof(AbilityType)))
        {
            PlayerAbility ability = abilityManager.GetAbility(abilityType);

            if (ability == null || ability.Data == null)
            {
                Debug.LogError($"Missing ability configuration: {abilityType}");
                return false;
            }

            if (!ability.IsUnlocked)
                abilities.Add(ability);
        }

        if (abilities.Count == 0)
            return false;

        ClearCards();

        foreach (PlayerAbility ability in abilities)
        {
            AbilityData data = ability.Data;
            AbilityType abilityType = data.Type;

            RewardCardUI card = Instantiate(rewardCardPrefab, initialAbilityCardContainer);

            card.Setup(
                data.AbilityIcon,
                data.AbilityName,
                "Level 1 - Unlock",
                data.GetLevelDescription(1),
                "Learn Ability",
                () => onAbilitySelected?.Invoke(abilityType)
            );

            activeCards.Add(card);
        }

        LockGameplay();

        backgroundDimmer.SetActive(true);
        initialAbilitySelectionPanel.SetActive(true);

        return true;
    }

    public void Close()
    {
        initialAbilitySelectionPanel.SetActive(false);
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