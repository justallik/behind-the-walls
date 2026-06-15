using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ItemSelector : MonoBehaviour
{
    [Header("Ray Settings")]
    public Camera playerCamera;
    public float rayDistance = 2.5f;
    public LayerMask interactableMask = ~0;

    [Header("UI")]
    public GameObject promptUI;
    public TextMeshProUGUI tmpText;

    private InteractableItem currentItem;
    private InteractableBed currentBed;

    private float lastDetectionTime = 0f;
    private const float DETECTION_DELAY = 0.05f;

    private void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            if (tmpText != null) tmpText.text = "...";
            promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentItem != null)
            {
                currentItem.Interact();
                ClearSelection();
                return;
            }
            else if (currentBed != null)
            {
                currentBed.Interact();
                ClearSelection();
                return;
            }
        }

        if (Time.time - lastDetectionTime >= DETECTION_DELAY)
        {
            DetectItem();
            lastDetectionTime = Time.time;
        }
    }

    private void DetectItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        InteractableItem foundItem = null;
        InteractableBed foundBed = null;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableMask))
        {
            // Сначала ищем кровать (приоритет)
            foundBed = hit.collider.GetComponent<InteractableBed>();
            if (foundBed == null)
                foundBed = hit.collider.GetComponentInParent<InteractableBed>();

            if (foundBed == null)
            {
                foundItem = hit.collider.GetComponent<InteractableItem>();
                if (foundItem == null)
                    foundItem = hit.collider.GetComponentInParent<InteractableItem>();
            }
        }

        if (foundItem != null)
        {
            currentItem = foundItem;
            currentBed = null;
            UpdateItemUI();
        }
        else if (foundBed != null)
        {
            currentBed = foundBed;
            currentItem = null;
            UpdateBedUI();
        }
        else
        {
            if (currentItem != null || currentBed != null)
                ClearSelection();
        }
    }

    private void UpdateItemUI()
    {
        if (currentItem == null || currentItem.itemData == null) return;

        string actionText = currentItem.itemData.itemType == ItemData.ItemType.Note
            ? "Read "
            : "Pick up ";

        if (tmpText != null)
            tmpText.text = "[E] " + actionText + currentItem.itemData.itemName;

        if (promptUI != null)
            promptUI.SetActive(true);
    }

    private void UpdateBedUI()
    {
        Debug.Log("UpdateBedUI вызван!");
        if (tmpText != null)
            tmpText.text = "[E] Sleep";
        else
            Debug.Log("tmpText == null!");

        if (promptUI != null)
            promptUI.SetActive(true);
        else
            Debug.Log("promptUI == null!");
    }

    private void ClearSelection()
    {
        currentItem = null;
        currentBed = null;
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}