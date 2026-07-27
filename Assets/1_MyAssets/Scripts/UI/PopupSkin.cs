using Raccoon;
using Raccoon.Controller;
using Raccoon.EnumHolder;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    void InitDic()
    {
        dicDataItem = new Dictionary<TypeSkin, List<SoSkin>>();
        foreach (TypeSkin type in Enum.GetValues(typeof(TypeSkin)))
        {
            dicDataItem[type] = new List<SoSkin>();
        }

        foreach (var data in GameData.Get.listskin)
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
        CharacterData cd = GameData.Get.GetCharacterData();
        bool isUse = false;
        bool isUnlock = false;
        if (cd.GetIdCurrentSkin(data.typeSkin) == data.id)
        {
            currentItem = item;
            isUse = true;
        }
        if (cd.IsOwnSkin(data.typeSkin, data.id))
        {
            isUnlock = true;
        }
        item.SetData(data,isUnlock, isUse);

    }

    protected void OnClick(ItemSkin item)
    {
        if (currentItem == item) return;
        currentItem?.SetSelected(false);
        currentItem = item;
        currentItem?.SetSelected(true);
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


}
