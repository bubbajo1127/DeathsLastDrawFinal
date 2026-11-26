using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AbilityHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private InputManager inputManager;

    [Header("Ability Slots")]
    [SerializeField] private AbilitySlotUI[] slots;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;

    [Header("Drag and Drop")]
    [SerializeField] private Image dragIcon;

    private readonly string[] keybinds = { "Q", "W", "E" };

    private AbilitySlotUI hoveredSlot;
    private AbilitySlotUI draggedSlot;

    private bool CanInteract => isActiveAndEnabled && inputManager != null && !inputManager.IsGameplayInputLocked;

    private void Awake()
    {
        tooltipPanel.SetActive(false);

        dragIcon.enabled = false;
        dragIcon.raycastTarget = false;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Initialize(this, i, keybinds[i]);
        }
    }

    private void Update()
    {
        RefreshSlots();

        if (!CanInteract)
        {
            hoveredSlot = null;

            CancelDrag();
            HideTooltip();

            return;
        }

        RefreshTooltip();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            PlayerAbility ability = abilityManager.GetAbilityInSlot(i);

            slots[i].Refresh(ability);
        }
    }

    // TOOLTIP

    public void SetHoveredSlot(AbilitySlotUI slot)
    {
        if (!CanInteract)
            return;

        hoveredSlot = slot;
    }

    public void ClearHoveredSlot(AbilitySlotUI slot)
    {
        if (hoveredSlot == slot)
        {
            hoveredSlot = null;
            HideTooltip();
        }
    }

    private void RefreshTooltip()
    {
        if (hoveredSlot == null || draggedSlot != null)
        {
            HideTooltip();
            return;
        }

        PlayerAbility ability = hoveredSlot.CurrentAbility;

        if (ability == null || !ability.IsUnlocked || ability.Data == null)
        {
            HideTooltip();
            return;
        }

        tooltipTitle.text = $"{ability.Data.AbilityName} - Level {ability.Level}";

        string description = string.Empty;

        for (int level = 1; level <= ability.Level; level++)
        {
            if (level > 1)
                description += "\n\n";

            description += $"<b>Level {level}</b>\n{ability.Data.GetLevelDescription(level)}";
        }

        tooltipDescription.text = description;

        tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    // DRAG AND DROP

    public void BeginDrag(AbilitySlotUI slot, PointerEventData eventData)
    {
        if (!CanInteract || draggedSlot != null)
            return;

        PlayerAbility ability = slot.CurrentAbility;

        if (ability == null || !ability.IsUnlocked || ability.Data == null)
            return;

        draggedSlot = slot;

        HideTooltip();

        dragIcon.sprite = ability.Data.AbilityIcon;
        dragIcon.enabled = dragIcon.sprite != null;

        UpdateDrag(slot, eventData);
    }

    public void UpdateDrag(AbilitySlotUI slot, PointerEventData eventData)
    {
        if (draggedSlot != slot)
            return;

        if (!CanInteract)
        {
            CancelDrag();
            return;
        }

        RectTransform parent = dragIcon.rectTransform.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out Vector2 localPosition))
        {
            dragIcon.rectTransform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        }
    }

    public void DropOnSlot(AbilitySlotUI targetSlot)
    {
        if (!CanInteract || draggedSlot == null)
            return;

        if (draggedSlot != targetSlot)
        {
            abilityManager.SwapSlots(draggedSlot.SlotIndex, targetSlot.SlotIndex);

            RefreshSlots();
        }
    }

    public void EndDrag(AbilitySlotUI slot)
    {
        if (draggedSlot == slot)
            CancelDrag();
    }

    private void CancelDrag()
    {
        draggedSlot = null;

        if (dragIcon != null)
        {
            dragIcon.enabled = false;
            dragIcon.sprite = null;
        }
    }

    private void OnDisable()
    {
        hoveredSlot = null;

        CancelDrag();

        if (tooltipPanel != null)
            HideTooltip();
    }
}