using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Door State")]
    [SerializeField] private bool isUnlocked = true;

    public Transform InteractionPoint => interactionPoint;
    public float InteractionRange => interactionRange;
    public bool IsUnlocked => isUnlocked;

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }
}
