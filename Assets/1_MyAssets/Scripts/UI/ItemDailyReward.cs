using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemDailyReward : MonoBehaviour
{
    public SoDailyReward data;
    public UnityAction<ItemDailyReward> onClick;
    public GameObject claimedGO;
    public GameObject lockGO;
    public Image bg;
    public Image icon;
    public TMP_Text textDay;
    public void Init(SoDailyReward data, Sprite spriteBG)
    {
        this.data = data;
        bg.sprite = spriteBG;
        icon.sprite = data.icon;
        textDay.text = "DAY " + data.day;
    }

    public void OnClick()
    {
        onClick?.Invoke(this);  
    }

    public void SetClaimed(bool isClaimed)
    {
        claimedGO.SetActive(isClaimed);
        SetActiveLock(!isClaimed);
    }

    public void SetActiveLock(bool isLock)
    {
        lockGO.SetActive(isLock);
    }

}
