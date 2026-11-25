using System;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private PlayerAbilityManager playerAbilityManager;
    [SerializeField] private RewardSelectionUI rewardSelectionUI;

    [Header("Testing")]
    [SerializeField] private bool allowUnclearedRoomExit = true;

    public RunState State { get; private set; } = RunState.Inactive;
    public RunRoomType CurrentRoomType { get; private set; }
    public int CurrentRoomNumber { get; private set; }

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

        dungeonManager.InitializeRoomPool();

        LoadNextRoom();
    }

    public void TryAdvanceRoom()
    {
        if (CurrentRoomType == RunRoomType.Boss)
            return;

        bool canAdvance = State == RunState.RoomCleared;

        if (allowUnclearedRoomExit && State == RunState.InRoom)
            canAdvance = true;

        if (!canAdvance)
            return;

        LoadNextRoom();
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

    public void MarkRoomCleared()
    {
        if (State != RunState.InRoom)
            return;

        if (CurrentRoomType == RunRoomType.Boss)
            return;

        if (CurrentRoomType == RunRoomType.Starting)
        {
            SetState(RunState.SelectingRoomReward);

            OnRoomCleared?.Invoke();

            if (!rewardSelectionUI.ShowInitialAbilitySelection(playerAbilityManager, SelectInitialAbility))
            {
                Debug.LogError("Failed to open initial ability selection.");
                SetState(RunState.LoadFailed);
            }

            return;
        }

        // Temporary normal room behavior until Part 3B.
        SetState(RunState.RoomCleared);

        OnRoomCleared?.Invoke();
    }

    private void SelectInitialAbility(AbilityType abilityType)
    {
        if (State != RunState.SelectingRoomReward)
            return;

        if (CurrentRoomType != RunRoomType.Starting)
            return;

        if (playerAbilityManager.LearnedAbilityCount != 0)
            return;

        RoomEnemySpawner spawner = dungeonManager.CurrentRoom.GetComponent<RoomEnemySpawner>();

        if (spawner == null)
        {
            Debug.LogError("Current room is missing its enemy spawner.");
            return;
        }

        if (!playerAbilityManager.LearnAbility(abilityType))
            return;

        Debug.Log($"Initial ability selected: {abilityType}");

        rewardSelectionUI.Close();

        SetState(RunState.RoomCleared);

        spawner.UnlockDoorsAfterReward();
    }

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