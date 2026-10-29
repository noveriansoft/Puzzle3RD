using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuTextHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Text")]
    [SerializeField] private TMP_Text targetText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.red;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>(true);

        SetColor(normalColor);
    }

    private void OnEnable()
    {
        SetColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(normalColor);
    }

    private void OnDisable()
    {
        SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        if (targetText != null)
            targetText.color = color;
    }
}