using Raccoon;
using Raccoon.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupRewardWin : PopupCanvas
{
    [SerializeField] List<ViewInfor> views;

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);

        List<IncomeData> data = obj as List<IncomeData>;

        ShowData(data);
    }

    public void ShowData(List<IncomeData> data)
    {
        for(int i = 0; i < views.Count; i++)
        {
            if(i < data.Count)
            {
                views[i].go.SetActive(true);
                views[i].icon.sprite = data[i].icon;
                views[i].textNumber.text = data[i].amount.ConvertCurrencyToString();
            }
            else
                views[i].go.SetActive(false);
        }
    }

    public void OnClickClaim()
    {
        GamePlayController.instance.ClaimRewardWin();
        Hide();
    }

    public void OnClickX2Claim()
    {
        //ads
        GamePlayController.instance.ClaimRewardWin(2);
        Hide();
    }
}

[System.Serializable]
struct ViewInfor
{
    public GameObject go;
    public Image icon;
    public TMP_Text textNumber; 
}
