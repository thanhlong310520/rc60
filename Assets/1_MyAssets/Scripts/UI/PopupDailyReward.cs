using Raccoon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupDailyReward : PopupCanvas
{
    public ItemDailyReward prefab;
    public Transform content;
    public List<ItemDailyReward> listItem;

    ItemDailyReward currentItem;
    bool init = false;
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        if(!init)
        {
            listItem = new List<ItemDailyReward>();
            init = true;
            SpawnItem();
        }
        SetData();
    }

    void SpawnItem()
    {
        foreach (var data in GameData.Get.listDailyRewardSO)
        {
            ItemDailyReward item = Spawn();
            Sprite bg = GameData.Get.GetBgDailyRewardCurrencyByType(data.type);
            item.Init(data,bg);
        }
    }
    protected ItemDailyReward Spawn()
    {
        var newItem = Instantiate(prefab, content);
        newItem.gameObject.SetActive(true);
        listItem.Add(newItem);
        return newItem;
    }
    
    public void OnClickClaimCurrentItem()
    {
        if (currentItem == null) return;
        if (!GameData.Get.CanClaimReward()) return;
        currentItem.SetClaimed(true);
        GameData.Get.ClaimReward(new List<SoDailyReward> { currentItem.data });
        currentItem = null;
    }

    public void OnClickClaimAllItem()
    {
        print("Click claim all ");
        currentItem = null;
        List<SoDailyReward> result = new List<SoDailyReward>();
        int currentDay = GameData.Get.GetDayReward();
        foreach (var item in listItem)
        {
            if (item.data.day >= currentDay)
            {
                result.Add(item.data);
                item.SetClaimed(true);
            }
        }
        GameData.Get.ClaimReward(result);
    }

    void SetData()
    {
        int currentDay = GameData.Get.GetDayReward();
        foreach (var item in listItem)
        {
            if(item.data.day < currentDay)
            {
                item.SetClaimed(true);
            }
            else if(item.data.day == currentDay)
            {
                if(GameData.Get.CanClaimReward())
                {
                    currentItem = item;
                    item.SetClaimed(false);
                    item.SetActiveLock(false);
                }    
            }
            else
            {
                item.SetClaimed(false);
            }
        }
    }
}
