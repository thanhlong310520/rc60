using UnityEngine;

public class PopupDead : PopupCanvas
{
    public void OnClickSkipLevel()
    {
        GamePlayController.instance.SetNextPoint();
        Hide();


    }

    public void OnClickResume()
    {
        GamePlayController.instance.Resume();
        Hide();
    }
}
