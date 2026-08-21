using UnityEngine;
using UnityEngine.UI;

public class RewardWinGame : MonoBehaviour
{

    public Button button;
    public GameObject noti;
    public ShakeUI shakeUI;
    public void OnEnableButton()
    {
        button.enabled = true;
        noti.SetActive(true);
        shakeUI.ShakeForever();
    }
    public void OnClickButton()
    {
        GamePlayController.instance.ShowRewardWin();
        OnDisableButton();
    }

    public void OnDisableButton()
    {
        button.enabled = false;
        noti.SetActive(false);
        shakeUI.StopShake();
    }
    public void BlingWhenFirstWin()
    {
        OnEnableButton();
    }
}
