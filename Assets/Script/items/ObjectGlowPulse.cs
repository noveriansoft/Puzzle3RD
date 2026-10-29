using UnityEngine;

public class ObjectGlowPulse : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Glow")]
    [SerializeField, ColorUsage(false, true)]
    private Color glowColor = Color.yellow;

    [SerializeField, Min(0f)] private float minIntensity = 0f;
    [SerializeField, Min(0f)] private float maxIntensity = 3f;
    [SerializeField, Min(0f)] private float pulseSpeed = 2f;

    private Material[] materials;
    private Color[] originalEmission;
    private bool[] originalEmissionEnabled;

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
            return;

        // Create material instances for this object only.
        materials = targetRenderer.materials;

        originalEmission = new Color[materials.Length];
        originalEmissionEnabled = new bool[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];

            if (!material.HasProperty(EmissionColorID))
                continue;

            originalEmission[i] =
                material.GetColor(EmissionColorID);

            originalEmissionEnabled[i] =
                material.IsKeywordEnabled("_EMISSION");
        }
    }

    private void Update()
    {
        if (materials == null)
            return;

        float pulse =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float intensity =
            Mathf.Lerp(minIntensity, maxIntensity, pulse);

        foreach (Material material in materials)
        {
            if (!material.HasProperty(EmissionColorID))
                continue;

            material.EnableKeyword("_EMISSION");
            material.SetColor(
                EmissionColorID,
                glowColor * intensity
            );
        }
    }

    private void OnDisable()
    {
        if (materials == null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];

            if (!material.HasProperty(EmissionColorID))
                continue;

            material.SetColor(
                EmissionColorID,
                originalEmission[i]
            );

            if (originalEmissionEnabled[i])
                material.EnableKeyword("_EMISSION");
            else
                material.DisableKeyword("_EMISSION");
        }
    }

    private void OnDestroy()
    {
        if (materials == null)
            return;

        foreach (Material material in materials)
        {
            if (material != null)
                Destroy(material);
        }
    }
}