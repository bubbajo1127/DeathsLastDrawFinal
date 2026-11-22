using UnityEngine;

[CreateAssetMenu(fileName = "BlastAbilityData", menuName = "Death's Last Draw/Abilities/Blast")]
public class BlastAbilityData : ScriptableObject
{
    [Header("Level 1")]
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float castRange = 7f;
    [SerializeField] private float blastLength = 10f;
    [SerializeField] private float blastWidth = 2f;
    [SerializeField] private float hitHeight = 2f;
    [SerializeField] private float damageMultiplier = 1.5f;

    public float Cooldown => cooldown;
    public float CastRange => castRange;
    public float BlastLength => blastLength;
    public float BlastWidth => blastWidth;
    public float HitHeight => hitHeight;
    public float DamageMultiplier => damageMultiplier;
}