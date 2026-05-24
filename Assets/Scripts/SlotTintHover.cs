using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotTintHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private Image tintImage;
    [SerializeField] private Color hoverColor = new Color(0f, 0f, 0f, 0.4f);
    private Color normalColor = new Color(0f, 0f, 0f, 0f);

    private void Start()
    {
        if (tintImage != null)
            tintImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tintImage != null)
            tintImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tintImage != null)
            tintImage.color = normalColor;
    }
}