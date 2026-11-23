using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    private const int MaxAbilitySlots = 3;

    [Header("References")]
    [SerializeField] private InputManager inputManager;

    [Header("Available Abilities")]
    [SerializeField] private PlayerAbility[] availableAbilities;

    [Header("Testing")]
    [SerializeField] private bool useTestingLoadout = true;
    [SerializeField] private AbilityType[] testingStartingAbilities;

    private PlayerAbility[] abilitySlots = new PlayerAbility[MaxAbilitySlots];

    public int LearnedAbilityCount { get; private set; }
    public bool CanLearnNewAbility => LearnedAbilityCount < MaxAbilitySlots;

    private void Awake()
    {
        if (availableAbilities == null || availableAbilities.Length == 0)
            availableAbilities = GetComponents<PlayerAbility>();
    }

    private void Start()
    {
        if (!useTestingLoadout)
            return;

        foreach (AbilityType abilityType in testingStartingAbilities)
            LearnAbility(abilityType);
    }

    private void Update()
    {
        if (inputManager.Ability1Pressed)
            UseSlot(0);

        if (inputManager.Ability2Pressed)
            UseSlot(1);

        if (inputManager.Ability3Pressed)
            UseSlot(2);
    }

    public void UseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length)
            return;

        PlayerAbility ability = abilitySlots[slotIndex];

        if (ability == null)
            return;

        ability.TryUse();
    }

    public bool LearnAbility(AbilityType abilityType)
    {
        PlayerAbility ability = GetAbility(abilityType);

        if (ability == null)
        {
            Debug.LogError($"Ability {abilityType} does not exist on the player.");
            return false;
        }

        if (ability.IsUnlocked)
        {
            Debug.Log($"{ability.Data.AbilityName} is already learned.");
            return false;
        }

        if (!CanLearnNewAbility)
        {
            Debug.Log("Player already knows the maximum number of abilities.");
            return false;
        }

        int emptySlot = FindEmptySlot();

        if (emptySlot == -1)
            return false;

        ability.Unlock();
        abilitySlots[emptySlot] = ability;
        LearnedAbilityCount++;

        Debug.Log($"{ability.Data.AbilityName} assigned to Ability Slot {emptySlot + 1}.");

        return true;
    }

    public bool UpgradeAbility(AbilityType abilityType)
    {
        PlayerAbility ability = GetAbility(abilityType);

        if (ability == null || !ability.IsUnlocked)
            return false;

        return ability.Upgrade();
    }

    public void SwapSlots(int firstSlot, int secondSlot)
    {
        if (!IsValidSlot(firstSlot) || !IsValidSlot(secondSlot))
            return;

        PlayerAbility temporaryAbility = abilitySlots[firstSlot];
        abilitySlots[firstSlot] = abilitySlots[secondSlot];
        abilitySlots[secondSlot] = temporaryAbility;
    }

    public PlayerAbility GetAbilityInSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return null;

        return abilitySlots[slotIndex];
    }

    public PlayerAbility GetAbility(AbilityType abilityType)
    {
        foreach (PlayerAbility ability in availableAbilities)
        {
            if (ability != null && ability.Data != null && ability.Data.Type == abilityType)
                return ability;
        }

        return null;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < abilitySlots.Length; i++)
        {
            if (abilitySlots[i] == null)
                return i;
        }

        return -1;
    }

    private bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < abilitySlots.Length;
    }
}