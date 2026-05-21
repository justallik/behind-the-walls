using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject hoverIcon; // перетащи HoverIcon сюда

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverIcon != null)
            hoverIcon.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverIcon != null)
            hoverIcon.SetActive(false);
    }
}
