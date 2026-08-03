using Raccoon;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupPlay : PopupCanvas
{
    public List<TMP_Text> textPercent;
    public Image avtMap;
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        CheckData();
        base.Show(p, afterPopup, obj);
        ShowUICheckpoint();
    }

    public void ShowUICheckpoint()
    {
        // Implementation for showing UI checkpoint
        string idCheckpoint = PlayerData.Get.GetLastCheckPointInMap(GameData.Get.currentMap.mapId);
        var currentMapController = GameData.Get.currentMap.mapPrefab.GetComponent<MapController>();

        int index = currentMapController.GetIndexCheckpoint(idCheckpoint);

        float percent = (float)(index + 1) / currentMapController.GetCheckpoints().Count;

        foreach (var text in textPercent)
        {
            text.text = Mathf.RoundToInt(percent * 100).ToString() + "%";
        }
    }

    void ChangeAvt()
    {
        var avt = GameData.Get.currentMap.avt;
        avtMap.sprite = avt;
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
