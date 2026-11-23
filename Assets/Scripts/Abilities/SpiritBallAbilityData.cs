using UnityEngine;

[CreateAssetMenu(fileName = "SpiritBallAbilityData", menuName = "Death's Last Draw/Abilities/Spirit Ball")]
public class SpiritBallAbilityData : AbilityData
{
    [Header("Level 1")]
    [SerializeField] private float cooldown = 7f;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float maxThrowDistance = 12f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float damageMultiplier = 1.25f;
    [SerializeField] private float knockbackDistance = 4f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float arcHeight = 4f;

    public override AbilityType Type => AbilityType.SpiritBall;

    public float Cooldown => cooldown;
    public float ProjectileSpeed => projectileSpeed;
    public float MaxThrowDistance => maxThrowDistance;
    public float ExplosionRadius => explosionRadius;
    public float DamageMultiplier => damageMultiplier;
    public float KnockbackDistance => knockbackDistance;
    public float KnockbackDuration => knockbackDuration;
    public float ArcHeight => arcHeight;
}