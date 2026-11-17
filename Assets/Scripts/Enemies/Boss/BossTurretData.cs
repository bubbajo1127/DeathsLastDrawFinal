using UnityEngine;

[CreateAssetMenu(fileName = "BossTurretData", menuName = "Death's Last Draw/Boss/Turret Data")]
public class BossTurretData : ScriptableObject
{
    [Header("Attack")]
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstShotDelay = 0.15f;

    [Header("Stagger")]
    [SerializeField] private float turretStaggerDelay = 0.5f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    public float AttackInterval => attackInterval;
    public int BurstCount => burstCount;
    public float BurstShotDelay => burstShotDelay;
    public float TurretStaggerDelay => turretStaggerDelay;
    public float DamageMultiplier => damageMultiplier;
}