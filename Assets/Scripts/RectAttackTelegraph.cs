using System.Collections;
using UnityEngine;

public class RectAttackTelegraph : MonoBehaviour
{
    [SerializeField] private Transform background;
    [SerializeField] private Transform fill;

    [Header("Width Animation")]
    [SerializeField, Range(0f, 1f)] private float startingWidthPercent = 0.15f;

    public IEnumerator Play(float width, float length, float duration)
    {
        float startingWidth = width * startingWidthPercent;

        background.localScale = new Vector3(startingWidth, length, 1f);
        background.localPosition = new Vector3(0f, 0.02f, length * 0.5f);

        fill.localScale = new Vector3(startingWidth, length, 1f);
        fill.localPosition = new Vector3(0f, 0.03f, length * 0.5f);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            float currentWidth = Mathf.Lerp(startingWidth, width, progress);

            background.localScale = new Vector3(currentWidth, length, 1f);
            fill.localScale = new Vector3(currentWidth, length, 1f);

            yield return null;
        }

        background.localScale = new Vector3(width, length, 1f);
        fill.localScale = new Vector3(width, length, 1f);

        Destroy(gameObject);
    }
}