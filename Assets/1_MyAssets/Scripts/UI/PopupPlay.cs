using Raccoon;
using UnityEngine;
using UnityEngine.Events;

public class PopupPlay : PopupCanvas
{
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        CheckData();
        base.Show(p, afterPopup, obj);
    }

    void CheckData()
    {
        var dataMap = GameData.Get.GetDataMap(GameData.Get.currentMap.mapId);
        if (dataMap == null) return;

        Debug.Log("[PopupPlay] " + dataMap.listCheckPoint.Count);
        foreach (var item in dataMap.listCheckPoint)
        {
            print("data " + item);
        }
        if (dataMap.won)
        {
            GameData.Get.ResetMap(dataMap.map_id);
        }
    }
}
