using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainUiControl : MonoBehaviour
{

    public static MainUiControl instance;
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public TextMeshProUGUI textCoin;
    public List<PopupCanvas> popups;

    public Image iconAvatar;
    //public UIWarning uiWarning;
    public TMP_Text textWaitAds;

    public TMP_Text percentRun;
    public TMP_Text textIndexCheckPoint;
    public Slider sliderRun;



    //VipSubPopup vip;

    private void Start()
    {
        //textWaitAds.gameObject.SetActive(false);
    }


    public void ShowPopup(PopupCanvas.PopupType type,UnityAction actionHide, object obj)
    {
        var popup = popups.First(p => p.popup == type);
        if(popup != null) popup.Show(type,actionHide,obj);
    }


    public void ShowCoin(string text)
    {
        textCoin.text = text;
    }


    public PopupCanvas LoadPopup(PopupCanvas.PopupType type)
    {
        var popup = popups.FirstOrDefault(p => p.popup == type);
        if(popup == null) return null;
        return popup;
    }

    public void ChangeIconAvatar(Sprite icon)
    {
        iconAvatar.sprite = icon;
    }

    public bool IsPopupVipActive()
    {
        //if (vip == null) vip = LoadPopup(PopupCanvas.PopupType.VipSub).GetComponent<VipSubPopup>();
        //if (vip == null) return false;
        //if (vip.IsActive) return true;
        return false;
    }

    public void ClickButtonBackToHome()
    {
        SceneLoader.Instance.LoadScene("Home");
    }

    //public void WarringSteal()
    //{
    //    uiWarning.StartWarning();
    //}

    //public void ShowTextWaitAds(float amountWait)
    //{
    //    textWaitAds.text = "Loading ads " + (int)(amountWait * 100) + "%";
    //}

    //public void EnableTextWaitAds(bool isEnable)
    //{
    //    textWaitAds.gameObject.SetActive(isEnable);
    //}


    public void ShowCheckPoint(float percent)
    {
        // Implementation for showing checkpoint UI
        if(percent > 1) percent = 1;
        if(percent < 0) percent = 0;
        percentRun.text = Mathf.RoundToInt(percent * 100).ToString() + "%";
        sliderRun.value = percent;

    }
    public void ShowIndexCheckPoint(int index, int total)
    {
        textIndexCheckPoint.text = $"{index}/{total}";
    }

}
