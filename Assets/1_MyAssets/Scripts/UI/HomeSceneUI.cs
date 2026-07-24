using UnityEngine;

public class HomeSceneUI : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneLoader.Instance.LoadScene("GamePlay");
    }
}
