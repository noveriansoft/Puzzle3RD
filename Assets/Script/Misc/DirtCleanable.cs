using UnityEngine;

public class DirtCleanable : MonoBehaviour
{
    [SerializeField] private float cleanDuration = 3f;

    [Header("Visual")]
    [SerializeField] private float disableAlphaThreshold = 0.1f;

    private Renderer dirtRenderer;
    private Material dirtMaterial;

    private float cleanProgress = 0f;
    private Color originalColor;

    private void Start()
    {
        dirtRenderer = GetComponent<Renderer>();

        // Material instance supaya dirt lain tidak ikut berubah
        dirtMaterial = dirtRenderer.material;

        if (dirtMaterial.HasProperty("_BaseColor"))
            originalColor = dirtMaterial.GetColor("_BaseColor");
        else
            originalColor = dirtMaterial.color;
    }

    public void Clean(float amount)
    {
        cleanProgress += amount;

        float progress = Mathf.Clamp01(cleanProgress / cleanDuration);

        float alpha = Mathf.Lerp(originalColor.a, 0f, progress);

        Color newColor = originalColor;
        newColor.a = alpha;

        if (dirtMaterial.HasProperty("_BaseColor"))
            dirtMaterial.SetColor("_BaseColor", newColor);
        else
            dirtMaterial.color = newColor;

        // Kalau secara visual sudah hampir hilang, langsung dianggap selesai
        if (alpha <= disableAlphaThreshold || progress >= 1f)
        {
            gameObject.SetActive(false);
        }
    }
}