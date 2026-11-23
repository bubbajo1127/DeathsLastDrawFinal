using UnityEngine;

[CreateAssetMenu(fileName = "ChargedShotAbilityData", menuName = "Death's Last Draw/Abilities/Charged Shot")]
public class ChargedShotAbilityData : AbilityData
{
    [Header("Level 1")]
    [SerializeField] private float cooldown = 6f;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float hitRadius = 0.4f;
    [SerializeField] private float damageMultiplier = 2f;

    public override AbilityType Type => AbilityType.ChargedShot;

    public float Cooldown => cooldown;
    public float ProjectileSpeed => projectileSpeed;
    public float MaxDistance => maxDistance;
    public float HitRadius => hitRadius;
    public float DamageMultiplier => damageMultiplier;
}