using System.Collections;
using UnityEngine;

public class RectAttackTelegraph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform background;
    [SerializeField] private Transform fill;

    public IEnumerator Play(float width, float length, float duration)
    {
        // Full danger area.
        background.localScale = new Vector3(width, length, 1f);
        background.localPosition = new Vector3(0f, 0.02f, length * 0.5f);

        // Fill starts at zero.
        fill.localScale = new Vector3(width, 0f, 1f);
        fill.localPosition = new Vector3(0f, 0.03f, 0f);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / duration);
            float currentLength = length * progress;

            fill.localScale = new Vector3(width, currentLength, 1f);
            fill.localPosition = new Vector3(0f, 0.03f, currentLength * 0.5f);

            yield return null;
        }

        Destroy(gameObject);
    }
}
