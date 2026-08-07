using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupCanvas : MonoBehaviour
{
    public bool IsActive = false;
    public PopupType popup;
    public PopupAnimation popupAnimation;
    public Transform panel;

    public UnityAction onCloseAction;
    public bool hideWhenShowOtherPopup = true;

    public virtual void Show(PopupType p, UnityAction afterPopup,object obj)
    {
        if (popup == p)
        {
            if (IsActive == true)
                return;
            IsActive = true;

            onCloseAction = afterPopup;
            this.gameObject.SetActive(true);

            switch(popupAnimation)
            {
                case PopupAnimation.Punch:
                    this.panel.localScale = Vector3.one;
                    this.panel.DOKill();
                    this.panel.DOPunchScale(Vector3.one * 0.1f, 0.25f, 2, 0.2f).OnComplete(DoneShow);
                    break;
                case PopupAnimation.MoveToUp:
                    var rect = this.panel.GetComponent<RectTransform>();
                    rect.anchoredPosition = rect.anchoredPosition.WithY(-1800);
                    rect.DOKill();
                    rect.DOAnchorPosY(0, 0.3f).OnComplete(DoneShow);
                    break;
                case PopupAnimation.MoveToRight:
                    var rectRight = this.panel.GetComponent<RectTransform>();
                    rectRight.anchoredPosition = rectRight.anchoredPosition.WithX(1800);
                    rectRight.DOKill();
                    rectRight.DOAnchorPosX(0, 0.3f).OnComplete(DoneShow);
                    break;
            }    
        }   
        else
        {
            if (hideWhenShowOtherPopup)
                Hide();
        }    
    }     

    public virtual void DoneShow()
    {

    }
    public virtual void Show(PopupType p, string description, UnityAction onActionFirst, UnityAction onActionSecond, params string[] value)
    {
        if (popup == p)
        {
            if (IsActive == true)
                return;
            IsActive = true;

            onCloseAction = null;
            this.gameObject.SetActive(true);

            switch (popupAnimation)
            {
                case PopupAnimation.Punch:
                    this.panel.localScale = Vector3.one;
                    this.panel.DOKill();
                    this.panel.DOPunchScale(Vector3.one * 0.1f, 0.25f, 2, 0.2f).OnComplete(DoneShow);
                    break;
                case PopupAnimation.MoveToUp:
                    var rect = this.panel.GetComponent<RectTransform>();
                    rect.anchoredPosition = rect.anchoredPosition.WithY(-1800);
                    rect.DOKill();
                    rect.DOAnchorPosY(0, 0.3f).OnComplete(DoneShow);
                    break;
                case PopupAnimation.MoveToRight:
                    var rectRight = this.panel.GetComponent<RectTransform>();
                    rectRight.anchoredPosition = rectRight.anchoredPosition.WithX(1800);
                    rectRight.DOKill();
                    rectRight.DOAnchorPosX(0, 0.3f).OnComplete(DoneShow);
                    break;
            }
        }
        else
        {
            if (hideWhenShowOtherPopup)
                Hide();
        }
    }    
    
    public virtual void Hide()
    {
        if (!IsActive)
            return;
        IsActive = false;

        onCloseAction?.Invoke();
        //CanvasManager.Instance.HidePopup(popup);
        switch (popupAnimation)
        {
            case PopupAnimation.Punch:
                this.panel.DOKill();
                this.panel.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
                {
                    //onCloseAction?.Invoke();
                    onCloseAction = null;
                    this.gameObject.SetActive(false);
                });
                break;
            case PopupAnimation.MoveToUp:
                var rect = this.panel.GetComponent<RectTransform>();
                rect.DOKill();
                rect.DOAnchorPosY(-1200, 0.15f).OnComplete(() =>
                {
                    //onCloseAction?.Invoke();
                    onCloseAction = null;
                    this.gameObject.SetActive(false);
                });
                break;
            case PopupAnimation.MoveToRight:
                var rectRight = this.panel.GetComponent<RectTransform>();
                rectRight.DOKill();
                rectRight.DOAnchorPosX(1200, 0.15f).OnComplete(() =>
                {
                    //onCloseAction?.Invoke();
                    onCloseAction = null;
                    this.gameObject.SetActive(false);
                });
                break;
            default:
                //onCloseAction?.Invoke();
                onCloseAction = null;
                this.gameObject.SetActive(false);
                break;
        }

    }

    public enum PopupType
    {
        Setting,
        Play,
        Dead,
        Shop,
        DailyReward,
        Skin,
        VipSub,
        RemoveAds
    }

    public enum PopupAnimation
    {
        None,
        Punch,
        MoveToUp,
        MoveToRight
    }
}
