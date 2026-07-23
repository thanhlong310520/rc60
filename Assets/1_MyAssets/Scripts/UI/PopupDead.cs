using UnityEngine;

public class PopupDead : PopupCanvas
{
    public void OnClickSkipLevel()
    {

    }

    public void OnClickResume()
    {
        GamePlayController.instance.Resume();
        Hide();
    }
}
