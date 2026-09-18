using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;

    // Current health is runtime information, so it isn't directly edited
    // in the Inspector.
    public float CurrentHealth { get; private set; }

    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    public float MovementSpeed => movementSpeed;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        CurrentHealth -= damage;

        // Prevent health from going below zero.
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        Debug.Log($"{gameObject.name} took {damage} damage. " + $"Health: {CurrentHealth}/{MaxHealth}");
    }

    public virtual void Heal(float amount)
    {
        if (amount <= 0f) return;

        CurrentHealth += amount;

        // Prevent health from going above max health.
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0f;
    }
}
