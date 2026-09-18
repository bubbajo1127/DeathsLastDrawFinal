using UnityEngine;

public class PlayerStats : Stats
{
    [Header("Player Combat")]
    [SerializeField] private float attackRange = 8f;

    // Number of attacks the player can make per second.
    [SerializeField] private float attackSpeed = 1f;

    // Percentage of incoming damage reduced.
    // 0 = 0% reduction
    // 0.25 = 25% reduction
    // 1 = 100% reduction
    [SerializeField, Range(0f, 1f)] private float damageReduction = 0f;

    [Header("Player Resources")]
    [SerializeField] private float mana = 100f;
    [SerializeField] private int coins = 0;

    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
    public float DamageReduction => damageReduction;
    public float Mana => mana;
    public int Coins => coins;

    public void UseMana(float amount)
    {
        if (amount <= 0f) return;

        mana = Mathf.Max(mana - amount, 0f);
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f) return;

        mana += amount;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        coins += amount;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || coins < amount) return false;

        coins -= amount;
        return true;
    }

    public void IncreaseAttackRange(float amount)
    {
        if (amount > 0f) attackRange += amount;
    }

    public void IncreaseAttackSpeed(float amount)
    {
        if (amount > 0f) attackSpeed += amount;
    }

    public void IncreaseDamageReduction(float amount)
    {
        if (amount > 0f) damageReduction = Mathf.Clamp01(damageReduction + amount);
    }
}
