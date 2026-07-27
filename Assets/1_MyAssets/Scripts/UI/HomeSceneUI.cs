using Raccoon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HomeSceneUI : MonoBehaviour
{
    public List<PopupCanvas> listPopups;
    public void OnClickPlay()
    {
        ShowPopup(PopupCanvas.PopupType.Play);
    }

    public void ShowPopup(PopupCanvas.PopupType type)
    {
        var popup = listPopups.FirstOrDefault(p => p.popup == type);
        if (popup != null) popup.Show(type, null, null);
    }

    public void OnClickNewGame()
    {
        GameData.Get.ResetMap(GameData.Get.currentMap.mapId);
        LoadScene();
    }

    public void OnClickContinueGame()
    {
        LoadScene();
    }
    void LoadScene()
    {
        SceneLoader.Instance.LoadScene("GamePlay");
    }

}
