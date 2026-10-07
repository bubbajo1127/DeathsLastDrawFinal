using UnityEngine;

public class EnemyStats : Stats
{
    [Header("Enemy Combat")]
    [SerializeField] private float attackRange = 1.5f;

    // Attacks per second.
    [SerializeField] private float attackSpeed = 1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;

    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
    public float DetectionRange => detectionRange;
}