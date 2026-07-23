using UnityEngine;

public class TestLoad : MonoBehaviour
{
    public void Loadscene()
    {
        SceneLoader.Instance.LoadScene("GamePlay");
    }
}
