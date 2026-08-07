using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Showpopup : MonoBehaviour
{
    Button button;
    public PopupCanvas.PopupType popupType;

    public UnityEvent<PopupCanvas.PopupType> eventShow;

    private void Start()
    {
        if(button == null) { button = GetComponent<Button>(); }
        button.onClick.AddListener(Show);
    }

    void Show()
    {
        eventShow?.Invoke(popupType);
    }
}
