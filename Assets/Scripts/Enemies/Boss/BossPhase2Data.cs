using UnityEngine;

[CreateAssetMenu(fileName = "BossPhaseTwoData", menuName = "Death's Last Draw/Boss/Phase Two Data")]
public class BossPhaseTwoData : ScriptableObject
{
    [Header("Transition")]
    [SerializeField] private float transitionDuration = 2f;

    public float TransitionDuration => transitionDuration;
}