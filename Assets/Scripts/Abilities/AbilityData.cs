using UnityEngine;

public abstract class AbilityData : ScriptableObject
{
    [Header("Ability Information")]
    [SerializeField] private string abilityName;
    [SerializeField] private Sprite abilityIcon;

    [Header("Descriptions")]
    [SerializeField, TextArea(2, 4)] private string level1Description;
    [SerializeField, TextArea(2, 4)] private string level2Description;
    [SerializeField, TextArea(2, 4)] private string level3Description;

    public string AbilityName => abilityName;
    public Sprite AbilityIcon => abilityIcon;

    public abstract AbilityType Type { get; }

    public string GetLevelDescription(int level)
    {
        switch (level)
        {
            case 1:
                return level1Description;
            case 2:
                return level2Description;
            case 3:
                return level3Description;
            default:
                return string.Empty;
        }
    }
}