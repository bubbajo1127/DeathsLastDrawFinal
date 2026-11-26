using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text keybindText;
    [SerializeField] private TMP_Text abilityNameText;

    [Header("Level Indicators")]
    [SerializeField] private Image[] levelIndicators;
    [SerializeField] private Color unlockedLevelColor = new Color(0.85f, 0.67f, 0.38f);
    [SerializeField] private Color lockedLevelColor = new Color(0.25f, 0.22f, 0.18f);

    private AbilityHUD abilityHUD;

    public int SlotIndex { get; private set; }
    public PlayerAbility CurrentAbility { get; private set; }

    public void Initialize(AbilityHUD hud, int slotIndex, string keybind)
    {
        abilityHUD = hud;
        SlotIndex = slotIndex;

        keybindText.text = keybind;

        Refresh(null);
    }

    public void Refresh(PlayerAbility ability)
    {
        CurrentAbility = ability;

        bool hasAbility = ability != null && ability.IsUnlocked && ability.Data != null;

        if (!hasAbility)
        {
            ShowEmptySlot();
            return;
        }

        abilityIcon.sprite = ability.Data.AbilityIcon;
        abilityIcon.enabled = abilityIcon.sprite != null;

        abilityNameText.text = ability.Data.AbilityName;

        UpdateLevelIndicators(ability.Level);
        UpdateCooldown(ability);
    }

    private void ShowEmptySlot()
    {
        abilityIcon.sprite = null;
        abilityIcon.enabled = false;

        abilityNameText.text = "Empty";

        cooldownOverlay.enabled = false;
        cooldownOverlay.fillAmount = 0f;

        cooldownText.enabled = false;
        cooldownText.text = string.Empty;

        UpdateLevelIndicators(0);
    }

    private void UpdateLevelIndicators(int level)
    {
        for (int i = 0; i < levelIndicators.Length; i++)
        {
            levelIndicators[i].color = i < level ? unlockedLevelColor : lockedLevelColor;
        }
    }

    private void UpdateCooldown(PlayerAbility ability)
    {
        bool isOnCooldown = ability.IsOnCooldown;

        cooldownOverlay.enabled = isOnCooldown;
        cooldownText.enabled = isOnCooldown;

        cooldownOverlay.fillAmount = ability.CooldownNormalized;

        if (!isOnCooldown)
        {
            cooldownText.text = string.Empty;
            return;
        }

        float remaining = ability.CooldownRemaining;
        cooldownText.text = (Mathf.Ceil(remaining * 10f) / 10f).ToString("0.0");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        abilityHUD.SetHoveredSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        abilityHUD.ClearHoveredSlot(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        abilityHUD.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        abilityHUD.UpdateDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        abilityHUD.EndDrag(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        abilityHUD.DropOnSlot(this);
    }
}