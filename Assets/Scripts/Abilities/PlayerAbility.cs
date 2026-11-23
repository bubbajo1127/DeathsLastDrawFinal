using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour
{
    private const int MaxLevel = 3;

    public int Level { get; private set; }
    public bool IsUnlocked => Level > 0;
    public bool IsMaxLevel => Level >= MaxLevel;

    public abstract AbilityData Data { get; }

    public abstract bool TryUse();

    public void Unlock()
    {
        if (IsUnlocked)
            return;

        Level = 1;

        Debug.Log($"{Data.AbilityName} unlocked at Level {Level}.");
    }

    public bool Upgrade()
    {
        if (!IsUnlocked || IsMaxLevel)
            return false;

        Level++;

        Debug.Log($"{Data.AbilityName} upgraded to Level {Level}.");

        return true;
    }
}