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
        ShowCoin(PlayerData.Get.GetCharacterData().coin);
        ShowDiamond(PlayerData.Get.GetCharacterData().diamond);
    }

    private void OnEnable()
    {
        GameData.Get.GetCharacterData().onChangeCoin += ShowCoin;
        GameData.Get.GetCharacterData().onChangeDiamond += ShowDiamond;
    }

    private void OnDisable()
    {
        GameData.Get.GetCharacterData().onChangeCoin -= ShowCoin;
        GameData.Get.GetCharacterData().onChangeDiamond -= ShowDiamond;
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

    public void ShowCoin(long coin)
    {
        textCoin.text = coin.ConvertCurrencyToString();
    }

    public void ShowDiamond(long diamond)
    {
        textDiamond.text = diamond.ConvertCurrencyToString();
    }
}
