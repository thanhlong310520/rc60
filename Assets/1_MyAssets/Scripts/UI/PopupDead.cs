using UnityEngine;

public class PopupDead : PopupCanvas
{
    public void OnClickSkipLevel()
    {
        GamePlayController.instance.OnPlayAgain(true);
        Hide();


    }

    public void OnClickResume()
    {
        GamePlayController.instance.OnPlayAgain(false);
        Hide();
    }
}
