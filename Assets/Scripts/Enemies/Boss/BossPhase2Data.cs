using UnityEngine;

[CreateAssetMenu(fileName = "BossPhaseTwoData", menuName = "Death's Last Draw/Boss/Phase Two Data")]
public class BossPhaseTwoData : ScriptableObject
{
    [Header("Transition")]
    [SerializeField] private float transitionDuration = 2f;

    [Header("Hazards")]
    [SerializeField] private float initialHazardDelay = 1.5f;
    [SerializeField] private float hazardInterval = 3f;
    [SerializeField] private float telegraphDuration = 1f;
    [SerializeField] private int hazardsPerVolley = 2;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    public float TransitionDuration => transitionDuration;
    public float InitialHazardDelay => initialHazardDelay;
    public float HazardInterval => hazardInterval;
    public float TelegraphDuration => telegraphDuration;
    public int HazardsPerVolley => hazardsPerVolley;
    public float DamageMultiplier => damageMultiplier;
}
