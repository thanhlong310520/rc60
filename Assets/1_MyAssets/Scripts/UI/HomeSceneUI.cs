using Raccoon;
using Raccoon.Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HomeSceneUI : MonoBehaviour
{
    #region singleton
    public static HomeSceneUI instance;

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
    #endregion
    public List<PopupCanvas> listPopups;

    public PreviewCharacter previewCharacter;
    public GameObject uiPreview;
    private void Start()
    {
        uiPreview.SetActive(true);
    }
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
