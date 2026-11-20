using UnityEngine;

[CreateAssetMenu(fileName = "BossProgressiveAOEData", menuName = "Death's Last Draw/Boss/Progressive AOE Data")]
public class BossProgressiveAOEData : ScriptableObject
{
    [Header("Layers")]
    [SerializeField] private int layerCount = 6;
    [SerializeField] private float startingWidth = 3f;
    [SerializeField] private float widthIncreasePerLayer = 1.5f;
    [SerializeField] private float layerLength = 3f;
    [SerializeField] private float hitHeight = 2f;

    [Header("Timing")]
    [SerializeField] private float layerInterval = 0.2f;
    [SerializeField] private float layerTelegraphTime = 0.6f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    public int LayerCount => layerCount;
    public float StartingWidth => startingWidth;
    public float WidthIncreasePerLayer => widthIncreasePerLayer;
    public float LayerLength => layerLength;
    public float HitHeight => hitHeight;
    public float LayerInterval => layerInterval;
    public float LayerTelegraphTime => layerTelegraphTime;
    public float DamageMultiplier => damageMultiplier;
}