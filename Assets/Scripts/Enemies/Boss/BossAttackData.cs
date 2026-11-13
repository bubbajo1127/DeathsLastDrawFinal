using UnityEngine;

[CreateAssetMenu(fileName = "BossAttackData", menuName = "Death's Last Draw/Boss/Attack Data")]
public class BossAttackData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string attackName;
    [SerializeField] private string animationTrigger = "Attack";

    [Header("Range")]
    [SerializeField] private float minRange = 0f;
    [SerializeField] private float maxRange = 10f;

    [Header("Timing")]
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float telegraphTime = 0.5f;
    [SerializeField] private float recoveryTime = 0.5f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    public string AttackName => attackName;
    public string AnimationTrigger => animationTrigger;

    public float MinRange => minRange;
    public float MaxRange => maxRange;

    public float Cooldown => cooldown;
    public float TelegraphTime => telegraphTime;
    public float RecoveryTime => recoveryTime;

    public float DamageMultiplier => damageMultiplier;
}
