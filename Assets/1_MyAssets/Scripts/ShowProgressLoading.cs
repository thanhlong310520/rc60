using UnityEngine;
using UnityEngine.UI;

public class ShowProgressLoading : MonoBehaviour
{
    public Slider slider;
    private void OnEnable()
    {
        SceneLoader.Instance.OnLoadProgress += ShowProgressLoad;
    }
    private void OnDisable()
    {
        SceneLoader.Instance.OnLoadProgress -= ShowProgressLoad;
    }
    void ShowProgressLoad(float progress)
    {
        if(slider != null) 
            slider.value = progress;
    }
}
