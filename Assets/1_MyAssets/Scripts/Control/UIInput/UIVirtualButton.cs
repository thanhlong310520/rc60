using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Raccoon.UiInput
{

    public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }
        [System.Serializable]
        public class Event : UnityEvent { }

        [Header("Output")]
        public BoolEvent buttonStateOutputEvent;
        public Event buttonClickOutputEvent;

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OutputButtonStateValue(true);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OutputButtonStateValue(false);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OutputButtonClickEvent();
        }

        protected void OutputButtonStateValue(bool buttonState)
        {
            buttonStateOutputEvent.Invoke(buttonState);
        }

        protected void OutputButtonClickEvent()
        {
            buttonClickOutputEvent.Invoke();
        }
    }
}