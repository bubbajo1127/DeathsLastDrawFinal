using UnityEngine;

public enum PowerUpgradeType
{
    AttackDamagePercent,
    AttackSpeedPercent,
    MaxHealthFlat,
    MovementSpeedPercent,
    AttackRangeFlat,
    DamageReductionPercent
}

[CreateAssetMenu(fileName = "PowerUpgradeData", menuName = "Death's Last Draw/Rewards/Power Upgrade")]
public class PowerUpgradeData : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string upgradeName;
    [SerializeField] private Sprite upgradeIcon;

    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Effect")]
    [SerializeField] private PowerUpgradeType upgradeType;
    [SerializeField] private float amount;

    public string UpgradeName => upgradeName;
    public Sprite UpgradeIcon => upgradeIcon;
    public string Description => description;
    public PowerUpgradeType UpgradeType => upgradeType;
    public float Amount => amount;
}