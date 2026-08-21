
using Raccoon;
using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ItemSkin : MonoBehaviour
{
    public Image icon;
    public Image bg;
    public GameObject selected;
    public GameObject tagNew;
    public GameObject adsGO;
    public GameObject equipGO;
    public GameObject priceGO;
    public Image iconPrice;
    public TMP_Text priceTxt;
    public SoSkin data;
    UnityAction<ItemSkin> clickAciton;

    bool isUnlock;
    bool isUse;

    public void SetEventClick(UnityAction<ItemSkin> aciton)
    {
        clickAciton = aciton;
    }
    public virtual void SetData(SoSkin data, bool isUse)
    {
        this.data = data;
        this.isUse = isUse;
        iconPrice.sprite = GameData.Get.GetIconCurrencyByType(data.typeCurrency);
        priceTxt.text = data.price.ConvertCurrencyToString();
        SetSelected(isUse);
        SetIcon(data);
        tagNew.SetActive(data.tagNew);
    }
    void SetIcon(SoSkin data)
    {
        icon.sprite = data.icon;
    }
    public virtual void SetSelected(bool selected)
    {
        CharacterData cd = GameData.Get.GetCharacterData();
        if (cd.IsOwnSkin(data.typeSkin, data.id))
        {
            isUnlock = true;
        }

        this.selected.SetActive(selected);
        isUse = selected;
        SetUnlock();
    }

   void SetUnlock()
    {
        if (isUse)
        {
            equipGO.SetActive(false);
            adsGO.SetActive(false);
            priceGO.SetActive(false);
        }
        else
        {
            if (isUnlock || data.typePay == Raccoon.EnumHolder.EShopType.None)
            {
                equipGO.SetActive(true); adsGO.SetActive(false); priceGO.SetActive(false);
            }
            else
            {
                equipGO.SetActive(false); adsGO.SetActive(false); priceGO.SetActive(false);
                if (data.typePay == Raccoon.EnumHolder.EShopType.Ads) adsGO.SetActive(true);
                if (data.typePay == Raccoon.EnumHolder.EShopType.Currencies) priceGO.SetActive(true);
            }
        }

    }
    public void OnClick()
    {
        clickAciton?.Invoke(this);
    }
}
