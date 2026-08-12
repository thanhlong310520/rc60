using Raccoon;
using UnityEngine;
using UnityEngine.UI;

public class ChooseMap : MonoBehaviour
{
    public Button bt;

    public MapData mapData;
    private void Awake()
    {
        bt = GetComponent<Button>();
        bt.onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        chonMap(mapData);
    }
    public void chonMap(MapData mapData)
    {
        GameData.Get.SetCurrentMap(mapData);
    }
}
