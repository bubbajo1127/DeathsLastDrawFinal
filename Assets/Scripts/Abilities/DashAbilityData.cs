using UnityEngine;

[CreateAssetMenu(fileName = "DashAbilityData", menuName = "Death's Last Draw/Abilities/Dash")]
public class DashAbilityData : AbilityData
{
    [Header("Level 1")]
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;

    public override AbilityType Type => AbilityType.Dash;

    public float Cooldown => cooldown;
    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
}