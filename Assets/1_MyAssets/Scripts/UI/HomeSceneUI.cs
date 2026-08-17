using Raccoon;
using Raccoon.Controller;
using Raccoon.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    public TMP_Text textCoin;  
    public TMP_Text textDiamond;
    private void Start()
    {
        uiPreview.SetActive(true);
        ShowCoin();
        ShowDiamond();
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

    public void ShowCoin()
    {
        textCoin.text = PlayerData.Get.GetCharacterData().coin.ConvertCurrencyToString();
    }

    public void ShowDiamond()
    {
        textDiamond.text = PlayerData.Get.GetCharacterData().diamond.ConvertCurrencyToString();
    }
}
