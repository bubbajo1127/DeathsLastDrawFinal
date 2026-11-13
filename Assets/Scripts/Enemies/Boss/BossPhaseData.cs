using UnityEngine;

[System.Serializable]
public struct BossAttackEntry
{
    public BossAttackData attack;

    [Min(0f)]
    public float weight;
}

[CreateAssetMenu(fileName = "BossPhaseData", menuName = "Death's Last Draw/Boss/Phase Data")]
public class BossPhaseData : ScriptableObject
{
    [Header("Combat")]
    [SerializeField] private BossAttackEntry[] attacks;

    [Header("Positioning")]
    [SerializeField] private float preferredDistance = 5f;

    [Header("Phase Modifiers")]
    [SerializeField] private float cooldownMultiplier = 1f;
    [SerializeField] private float damageMultiplier = 1f;

    public float PreferredDistance => preferredDistance;
    public float CooldownMultiplier => cooldownMultiplier;
    public float DamageMultiplier => damageMultiplier;

    public bool TryGetWeight(BossAttackData attackData, out float weight)
    {
        foreach (BossAttackEntry entry in attacks)
        {
            if (entry.attack == attackData)
            {
                weight = entry.weight;
                return true;
            }
        }

        weight = 0f;
        return false;
    }
}