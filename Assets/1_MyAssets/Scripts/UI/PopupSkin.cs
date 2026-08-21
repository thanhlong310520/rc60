using Raccoon;
using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.Purchase;
using Raccoon.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PopupSkin : PopupCanvas
{
    public List<TabTypeSkinUI> listButtonTab;
    public ScrollRect scrollRect;

    TabTypeSkinUI currentTab;
    TypeSkin skinType = TypeSkin.Hair;

    Dictionary<TypeSkin, List<SoSkin>> dicDataItem;
    protected List<ItemSkin> listItem;

    public ItemSkin itemPrefab;
    public Transform content;
    protected ItemSkin currentItem;



    private void Start()
    {
        foreach (var t in listButtonTab)
        {
            t.ActionClick = ClickTap;
        }
    }

    

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        HomeSceneUI.instance.uiPreview.SetActive(false);
        base.Show(p, afterPopup, obj);
        foreach (var t in listButtonTab)
        {
            t.SetSelect(false);
            if (skinType == t.type) currentTab = t;
        }
        currentTab?.SetSelect(true);
        if (dicDataItem == null) InitDic();
        if (listItem == null) listItem = new List<ItemSkin>();
        ShowItem();
    }
    public override void Hide()
    {
        HomeSceneUI.instance.uiPreview.SetActive(true);

        base.Hide();
    }
    void InitDic()
    {
        dicDataItem = new Dictionary<TypeSkin, List<SoSkin>>();
        foreach (TypeSkin type in Enum.GetValues(typeof(TypeSkin)))
        {
            dicDataItem[type] = new List<SoSkin>();
        }

        foreach (var data in GameData.Get.listSkinSO)
        {
            dicDataItem[data.typeSkin].Add(data);
        }
    }
    void ShowItem()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            ItemSkin item = listItem[i];
            if (i < dicDataItem[skinType].Count)
            {
                item.gameObject.SetActive(true);
                SetDataItem(item, dicDataItem[skinType][i]);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        int index = listItem.Count;
        for (int i = index; i < dicDataItem[skinType].Count; i++)
        {
            ItemSkin item = Spawn();
            item.SetEventClick(OnClick);
            item.gameObject.SetActive(true);
            SetDataItem(item, dicDataItem[skinType][i]);
        }

    }
    protected ItemSkin Spawn()
    {
        var newItem = Instantiate(itemPrefab, content);
        listItem.Add(newItem);
        return newItem;
    }

    void SetDataItem(ItemSkin item, SoSkin data)
    {
        SoSkin skin = GameData.Get.currentSkinSOs.FirstOrDefault(s => s.typeSkin == data.typeSkin);
        bool isUse = false;
        if (skin == data)
        {
            currentItem = item;
            isUse = true;
        }
        item.SetData(data, isUse);

    }

    protected void OnClick(ItemSkin item)
    {
        if (currentItem == item) return;
        bool haveSkin = GameData.Get.GetCharacterData().IsOwnSkin(item.data.typeSkin, item.data.id);
        if(haveSkin)
        {
            ChangeSkin(item);
            return;
        }
        switch (item.data.typePay)
        {
            case EShopType.Ads:
                ShowRewardedAd(success =>
                {
                    if (success)
                    {
                        ChangeSkin(item);
                    }
                    else
                    {
                        Fail("Ads chưa sẵn sàng hoặc bị bỏ giữa chừng");
                    }
                });
                break;

            case EShopType.Currencies:
                if (TrySpend(item.data.typeCurrency, item.data.price))
                {
                    GameData.Get.PayCurrency(item.data.typeCurrency, item.data.price);
                    ChangeSkin(item);
                }
                else
                {
                    Fail("Không đủ tiền");
                }
                break;

            case EShopType.IAP:
                Purchase(item.data.productData, success =>
                {
                    if (success)
                    {
                        ChangeSkin(item);
                    }
                    else
                    {
                        Fail("Mua thất bại");
                    }
                });
                break;

            default:
                ChangeSkin(item);
                break;
        }
    }
    private void ClickTap(TabTypeSkinUI uI)
    {
        if (uI == currentTab) return;
        currentTab?.SetSelect(false);
        currentTab = uI;
        currentTab?.SetSelect(true);
        skinType = currentTab.type;

        content.localPosition = Vector3.zero;
        scrollRect.velocity = Vector3.zero;
        ResetItem();
        ShowItem();
    }

    protected void ResetItem()
    {
        if (currentItem != null)
        {
            currentItem.SetSelected(false);
            currentItem = null;
        }

    }

    private bool TrySpend(TypeCurrency type, long amount)
    {
        // return CurrencyManager.Instance.TrySpend(type, amount);
        long c = GameData.Get.GetCurrency(type);

        return c >= amount;
    }
    private void ShowRewardedAd(Action<bool> callback)
    {
        // AdsManager.Instance.ShowRewarded(callback);
        callback?.Invoke(true);
    }

    private void Purchase(IAPProductData product, Action<bool> callback)
    {
        //if (!GameStoreController.Get.IsSubscribedTo(product.id))
        //{
        //    GameStoreController.Get.OnBuyProduct(product, callback);
        //}
        GameStoreController.Get.OnBuyProduct(product, callback);
    }
    private void Fail(string reason)
    {
        print(reason);
    }

    private void ChangeSkin(ItemSkin item)
    {
        currentItem?.SetSelected(false);
        currentItem = item;
        currentItem?.SetSelected(true);


        GameData.Get.ChangeSkin(item.data);
        HomeSceneUI.instance.previewCharacter.SetCharacter(item.data);
    }

}
