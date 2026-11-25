using System;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    private const int MaxRewardOptions = 3;

    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private PlayerAbilityManager playerAbilityManager;
    [SerializeField] private RewardSelectionUI rewardSelectionUI;

    [Header("Power Upgrades")]
    [SerializeField] private PowerUpgradeData[] powerUpgradePool;

    private readonly List<PowerUpgradeData> selectedPowerUpgrades = new List<PowerUpgradeData>();

    public RunState State { get; private set; } = RunState.Inactive;
    public RunRoomType CurrentRoomType { get; private set; }
    public int CurrentRoomNumber { get; private set; }

    public RewardCategory PendingRewardCategory { get; private set; } = RewardCategory.None;
    public IReadOnlyList<PowerUpgradeData> SelectedPowerUpgrades => selectedPowerUpgrades;

    public event Action<RunState> OnStateChanged;
    public event Action OnRoomEntered;
    public event Action OnRoomCleared;
    public event Action OnRunVictory;
    public event Action OnRunDefeat;

    private void Start()
    {
        if (dungeonManager.RoomGenerationEnabled)
            StartRun();
    }

    public void StartRun()
    {
        if (State != RunState.Inactive)
            return;

        if (!dungeonManager.RoomGenerationEnabled)
            return;

        CurrentRoomNumber = 0;
        PendingRewardCategory = RewardCategory.None;

        selectedPowerUpgrades.Clear();

        dungeonManager.InitializeRoomPool();

        LoadNextRoom();
    }

    // ROOM PROGRESSION

    public void TryAdvanceRoom()
    {
        if (State != RunState.RoomCleared)
            return;

        if (CurrentRoomType == RunRoomType.Boss)
            return;

        if (dungeonManager.IsNextRoomBoss)
        {
            PendingRewardCategory = RewardCategory.None;
            LoadNextRoom();
            return;
        }

        if (GetAvailablePowerUpgrades().Count == 0)
        {
            Debug.LogError("No power upgrades have been configured.");
            return;
        }

        bool canChooseAbility = GetEligibleAbilityRewards().Count > 0;

        SetState(RunState.SelectingDoorReward);

        if (!rewardSelectionUI.ShowDoorRewardSelection(canChooseAbility, SelectDoorReward, CancelDoorReward))
        {
            Debug.LogError("Failed to open door reward selection.");
            SetState(RunState.RoomCleared);
        }
    }

    private void SelectDoorReward(RewardCategory category)
    {
        if (State != RunState.SelectingDoorReward)
            return;

        if (category != RewardCategory.Power && category != RewardCategory.Ability)
            return;

        if (category == RewardCategory.Ability && GetEligibleAbilityRewards().Count == 0)
            return;

        PendingRewardCategory = category;

        Debug.Log($"Next room reward: {PendingRewardCategory}");

        rewardSelectionUI.Close();

        LoadNextRoom();
    }

    private void CancelDoorReward()
    {
        if (State != RunState.SelectingDoorReward)
            return;

        PendingRewardCategory = RewardCategory.None;

        rewardSelectionUI.Close();

        SetState(RunState.RoomCleared);
    }

    private void LoadNextRoom()
    {
        SetState(RunState.LoadingRoom);

        if (!dungeonManager.TryLoadNextRoom(out RunRoomType roomType))
        {
            SetState(RunState.LoadFailed);
            return;
        }

        CurrentRoomType = roomType;
        CurrentRoomNumber++;

        Debug.Log($"Entered Room {CurrentRoomNumber}: {CurrentRoomType}");

        SetState(RunState.InRoom);

        if (CurrentRoomType != RunRoomType.Boss)
        {
            RoomEnemySpawner spawner = dungeonManager.CurrentRoom.GetComponent<RoomEnemySpawner>();

            if (spawner == null)
            {
                Debug.LogError("Current room is missing a RoomEnemySpawner.");
                SetState(RunState.LoadFailed);
                return;
            }

            if (!spawner.TryBegin(this))
            {
                SetState(RunState.LoadFailed);
                return;
            }
        }

        OnRoomEntered?.Invoke();
    }

    // ROOM COMPLETION

    public void MarkRoomCleared()
    {
        if (State != RunState.InRoom)
            return;

        if (CurrentRoomType == RunRoomType.Boss)
            return;

        SetState(RunState.SelectingRoomReward);

        OnRoomCleared?.Invoke();

        bool rewardOpened;

        if (CurrentRoomType == RunRoomType.Starting)
        {
            rewardOpened = rewardSelectionUI.ShowInitialAbilitySelection(playerAbilityManager, SelectInitialAbility);
        }
        else
        {
            rewardOpened = ShowNormalRoomReward();
        }

        if (!rewardOpened)
        {
            Debug.LogError("Failed to open room reward selection.");
            SetState(RunState.LoadFailed);
        }
    }

    // INITIAL ABILITY

    private void SelectInitialAbility(AbilityType abilityType)
    {
        if (State != RunState.SelectingRoomReward)
            return;

        if (CurrentRoomType != RunRoomType.Starting)
            return;

        if (playerAbilityManager.LearnedAbilityCount != 0)
            return;

        if (!TryGetClearedSpawner(out RoomEnemySpawner spawner))
            return;

        if (!playerAbilityManager.LearnAbility(abilityType))
            return;

        Debug.Log($"Initial ability selected: {abilityType}");

        CompleteRoomReward(spawner);
    }

    // NORMAL ROOM REWARDS

    private bool ShowNormalRoomReward()
    {
        List<RewardCardOption> options;

        switch (PendingRewardCategory)
        {
            case RewardCategory.Power:
                options = GeneratePowerRewardOptions();
                break;

            case RewardCategory.Ability:
                options = GenerateAbilityRewardOptions();
                break;

            default:
                Debug.LogError("No reward category selected for this room.");
                return false;
        }

        RandomizeRewardOptions(options);

        return rewardSelectionUI.ShowUpgradeSelection(PendingRewardCategory, options);
    }

    // ABILITY REWARDS

    private List<PlayerAbility> GetEligibleAbilityRewards()
    {
        List<PlayerAbility> eligibleAbilities = new List<PlayerAbility>();

        foreach (AbilityType abilityType in Enum.GetValues(typeof(AbilityType)))
        {
            PlayerAbility ability = playerAbilityManager.GetAbility(abilityType);

            if (ability == null || ability.Data == null)
                continue;

            if (ability.IsUnlocked)
            {
                if (!ability.IsMaxLevel)
                    eligibleAbilities.Add(ability);
            }
            else if (playerAbilityManager.CanLearnNewAbility)
            {
                eligibleAbilities.Add(ability);
            }
        }

        return eligibleAbilities;
    }

    private List<RewardCardOption> GenerateAbilityRewardOptions()
    {
        List<RewardCardOption> options = new List<RewardCardOption>();

        foreach (PlayerAbility ability in GetEligibleAbilityRewards())
        {
            AbilityData data = ability.Data;
            AbilityType abilityType = data.Type;

            bool isUnlocked = ability.IsUnlocked;
            int nextLevel = ability.Level + 1;

            string rewardType = isUnlocked ? $"Level {ability.Level} → {nextLevel}" : "Level 1 - Unlock";
            string buttonLabel = isUnlocked ? "Upgrade Ability" : "Learn Ability";

            options.Add(new RewardCardOption(
                data.AbilityIcon,
                data.AbilityName,
                rewardType,
                data.GetLevelDescription(nextLevel),
                buttonLabel,
                () => SelectAbilityReward(abilityType)
            ));
        }

        return options;
    }

    private void SelectAbilityReward(AbilityType abilityType)
    {
        if (State != RunState.SelectingRoomReward)
            return;

        if (CurrentRoomType != RunRoomType.Normal || PendingRewardCategory != RewardCategory.Ability)
            return;

        if (!TryGetClearedSpawner(out RoomEnemySpawner spawner))
            return;

        PlayerAbility ability = playerAbilityManager.GetAbility(abilityType);

        if (ability == null)
            return;

        bool success;

        if (ability.IsUnlocked)
        {
            success = playerAbilityManager.UpgradeAbility(abilityType);
        }
        else
        {
            success = playerAbilityManager.LearnAbility(abilityType);
        }

        if (!success)
            return;

        Debug.Log($"Ability reward selected: {abilityType}, Level {ability.Level}");

        CompleteRoomReward(spawner);
    }

    // POWER REWARDS

    private List<PowerUpgradeData> GetAvailablePowerUpgrades()
    {
        List<PowerUpgradeData> availableUpgrades = new List<PowerUpgradeData>();

        if (powerUpgradePool == null)
            return availableUpgrades;

        foreach (PowerUpgradeData upgrade in powerUpgradePool)
        {
            if (upgrade != null && !availableUpgrades.Contains(upgrade))
                availableUpgrades.Add(upgrade);
        }

        return availableUpgrades;
    }

    private List<RewardCardOption> GeneratePowerRewardOptions()
    {
        List<RewardCardOption> options = new List<RewardCardOption>();

        foreach (PowerUpgradeData upgrade in GetAvailablePowerUpgrades())
        {
            options.Add(new RewardCardOption(
                upgrade.UpgradeIcon,
                upgrade.UpgradeName,
                "Power Upgrade",
                upgrade.Description,
                "Select Upgrade",
                () => SelectPowerReward(upgrade)
            ));
        }

        return options;
    }

    private void SelectPowerReward(PowerUpgradeData upgrade)
    {
        if (State != RunState.SelectingRoomReward)
            return;

        if (CurrentRoomType != RunRoomType.Normal || PendingRewardCategory != RewardCategory.Power)
            return;

        if (upgrade == null)
            return;

        if (!TryGetClearedSpawner(out RoomEnemySpawner spawner))
            return;

        selectedPowerUpgrades.Add(upgrade);

        Debug.Log($"Power upgrade selected: {upgrade.UpgradeName}");

        // Actual stat modification will be implemented in Step 4.

        CompleteRoomReward(spawner);
    }

    // REWARD HELPERS

    private void RandomizeRewardOptions(List<RewardCardOption> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, options.Count);

            RewardCardOption temporaryOption = options[i];
            options[i] = options[randomIndex];
            options[randomIndex] = temporaryOption;
        }

        if (options.Count > MaxRewardOptions)
            options.RemoveRange(MaxRewardOptions, options.Count - MaxRewardOptions);
    }

    private bool TryGetClearedSpawner(out RoomEnemySpawner spawner)
    {
        spawner = null;

        if (dungeonManager.CurrentRoom == null)
            return false;

        spawner = dungeonManager.CurrentRoom.GetComponent<RoomEnemySpawner>();

        if (spawner == null || !spawner.IsRoomCleared)
        {
            Debug.LogError("Current room is missing a cleared enemy spawner.");
            return false;
        }

        return true;
    }

    private void CompleteRoomReward(RoomEnemySpawner spawner)
    {
        PendingRewardCategory = RewardCategory.None;

        rewardSelectionUI.Close();

        SetState(RunState.RoomCleared);

        spawner.UnlockDoorsAfterReward();
    }

    // RUN COMPLETION

    public void CompleteRun()
    {
        if (State != RunState.InRoom || CurrentRoomType != RunRoomType.Boss)
            return;

        SetState(RunState.Victory);

        OnRunVictory?.Invoke();
    }

    public void FailRun()
    {
        if (State != RunState.InRoom && State != RunState.RoomCleared)
            return;

        SetState(RunState.Defeat);

        OnRunDefeat?.Invoke();
    }

    private void SetState(RunState newState)
    {
        if (State == newState)
            return;

        State = newState;

        Debug.Log($"Run State: {State}");

        OnStateChanged?.Invoke(State);
    }
}