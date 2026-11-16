using UnityEngine;

[CreateAssetMenu(fileName = "ProgressiveAOEData", menuName = "Death's Last Draw/Boss/Progressive AOE Data")]
public class BossProgressiveAOEData : ScriptableObject
{
    [Header("Shape")]
    [SerializeField] private int layerCount = 6;
    [SerializeField] private float width = 8f;
    [SerializeField] private float layerLength = 3f;
    [SerializeField] private float hitHeight = 2f;

    [Header("Timing")]
    [SerializeField] private float layerInterval = 0.2f;
    [SerializeField] private float layerTelegraphTime = 0.6f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    public int LayerCount => layerCount;
    public float Width => width;
    public float LayerLength => layerLength;
    public float HitHeight => hitHeight;
    public float LayerInterval => layerInterval;
    public float LayerTelegraphTime => layerTelegraphTime;
    public float DamageMultiplier => damageMultiplier;
}