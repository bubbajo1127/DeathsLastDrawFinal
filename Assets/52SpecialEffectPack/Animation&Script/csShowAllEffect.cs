using UnityEngine;
using TMPro;

public class csShowAllEffect : MonoBehaviour
{
    [Header("Effects")]
    public string[] EffectName;
    public Transform[] Effect;

    [Header("UI")]
    public TMP_Text Text1;

    public int i = 0;

    private void Start()
    {
        if (Effect.Length == 0)
            return;

        SpawnCurrentEffect();
        UpdateText();
    }

    private void Update()
    {
        UpdateText();

        // Previous effect
        if (Input.GetKeyDown(KeyCode.Z))
        {
            i--;

            if (i < 0)
                i = Effect.Length - 1;

            SpawnCurrentEffect();
        }

        // Next effect
        if (Input.GetKeyDown(KeyCode.X))
        {
            i++;

            if (i >= Effect.Length)
                i = 0;

            SpawnCurrentEffect();
        }

        // Replay current effect
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnCurrentEffect();
        }
    }

    private void SpawnCurrentEffect()
    {
        if (Effect.Length == 0)
            return;

        if (Effect[i] == null)
            return;

        Instantiate(
            Effect[i],
            Vector3.zero,
            Quaternion.identity
        );
    }

    private void UpdateText()
    {
        if (Text1 == null)
            return;

        if (EffectName.Length <= i)
            return;

        Text1.text = (i + 1) + ": " + EffectName[i];
    }
}