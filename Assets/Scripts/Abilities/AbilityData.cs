using UnityEngine;

public abstract class AbilityData : ScriptableObject
{
    [Header("Ability")]
    [SerializeField] private string abilityName;

    public string AbilityName => abilityName;
    public abstract AbilityType Type { get; }
}