using Raccoon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioClip _audio;

    public void OnPointerDown(PointerEventData eventData)
    {
        //SoundManager.Instance.PlayOnShot(_audio);
        ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Button);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
