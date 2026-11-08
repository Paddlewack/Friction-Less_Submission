using UnityEngine;
using UnityEngine.Events;

public class PopupObject : MonoBehaviour, ITriggerable
{
    [Header("References")]
    [SerializeField]
    private PopupData popupData;

    [Header("Popup Settings")]
    [SerializeField]
    private float displayTime = 15f; // Set to 0 for infinite display until forced to stop

    public int TriggeredCount { get; private set; }
    public void Trigger()
    {
        UIManager.Instance.NewPopup(popupData, displayTime);
        Debug.Log($"[PopupUI] Popup triggered: {gameObject.name}");
        TriggeredCount++;
    }
}