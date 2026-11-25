using System;
using UnityEngine;

public class RewardCardOption
{
    public Sprite Icon { get; }
    public string Name { get; }
    public string Type { get; }
    public string Description { get; }
    public string ButtonLabel { get; }
    public Action OnSelected { get; }
    public bool IsSelectable { get; }

    public RewardCardOption(Sprite icon, string name, string type, string description, string buttonLabel, Action onSelected, bool isSelectable = true)
    {
        Icon = icon;
        Name = name;
        Type = type;
        Description = description;
        ButtonLabel = buttonLabel;
        OnSelected = onSelected;
        IsSelectable = isSelectable;
    }
}