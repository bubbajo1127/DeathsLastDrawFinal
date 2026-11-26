using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour
{
    private const int MaxLevel = 3;

    private float nextUseTime;
    private float cooldownDuration;

    public int Level { get; private set; }
    public bool IsUnlocked => Level > 0;
    public bool IsMaxLevel => Level >= MaxLevel;

    public float CooldownRemaining => Mathf.Max(0f, nextUseTime - Time.time);
    public float CooldownDuration => cooldownDuration;
    public bool IsOnCooldown => CooldownRemaining > 0f;

    public float CooldownNormalized => cooldownDuration > 0f ? Mathf.Clamp01(CooldownRemaining / cooldownDuration) : 0f;

    public abstract AbilityData Data { get; }

    public abstract bool TryUse();

    protected void StartCooldown(float duration)
    {
        cooldownDuration = Mathf.Max(0f, duration);
        nextUseTime = Time.time + cooldownDuration;
    }

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