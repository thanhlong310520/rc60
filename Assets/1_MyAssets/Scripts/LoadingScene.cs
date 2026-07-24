using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Slider sliderLoading; // drag Image (fill bar) vào đây
    [SerializeField] private GameObject loadingUI; // drag UI loading panel vào đây
    private float multi_speed = 1f;
    private Coroutine _waitInterOpenAdCoroutine;

    private void Start()
    {
        LoadScene();
    }

    public void LoadScene()
    {
        loadingUI.SetActive(true); // bật UI loading
        sliderLoading.value = 0;
        StartCoroutine(LoadSceneAsync());
        _waitInterOpenAdCoroutine = StartCoroutine(WaitInterOpenAd());
    }

    private IEnumerator LoadSceneAsync()
    {
        var delayLoad = 15f;
#if UNITY_EDITOR
        delayLoad = 2f;
#endif
        var timeLoad = 0f;
        var wait = new WaitForEndOfFrame();
        yield return wait;
        
        string sceneLoading = "Home";
        string currentScene = "StartScene";

        // if(RuntimeStorageData.Player.HasInGame)
        //     sceneLoading = "Home";
        
        yield return wait;
        multi_speed = 1f;
        var operation = SceneManager.LoadSceneAsync(sceneLoading, LoadSceneMode.Additive);

        operation.allowSceneActivation = false;
        while (timeLoad < delayLoad)
        {
            timeLoad += Time.deltaTime * multi_speed;
            var progress = timeLoad / delayLoad;
            sliderLoading.value = progress;
            yield return wait;
            multi_speed = 15;
            //if (GameAds.Get != null && GameAds.Get.IsInterstitialOpenShowed)
            //    multi_speed = 15f;
        }
        operation.allowSceneActivation = true;

        while (operation != null && !operation.isDone)
        {
            yield return wait;
        }

        if (_waitInterOpenAdCoroutine != null)
        {
            StopCoroutine(_waitInterOpenAdCoroutine);
            _waitInterOpenAdCoroutine = null;
        }
        // GameAds.Get.LoadAdsInGame();
        sliderLoading.value = 1f;
        Debug.Log("Load Complete");
        yield return wait;
        // Set the newly loaded scene as active so any future Instantiate() calls
        // land in the correct scene, not in Loading (which is about to be unloaded).
        var gamePlayScene = SceneManager.GetSceneByName(sceneLoading);
        if (gamePlayScene.IsValid())
            SceneManager.SetActiveScene(gamePlayScene);
        SceneManager.UnloadSceneAsync(currentScene);
    }
    
    private IEnumerator WaitInterOpenAd()
    {
        var wait = new WaitForSeconds(1f);
        yield return wait;

        //float elapsed = 0f;
        //while (elapsed < 15f)
        //{
        //    yield return wait;
        //    elapsed += 1f;
        //    var ads = GameAds.Get;
        //    if (ads != null && ads.IsInterstitialOpenAvailable)
        //    {
        //        yield return wait;
        //        ads.ShowInterstitialOpenAd(
        //            () =>
        //            {
        //                Time.timeScale = 0;
        //            }
        //            ,
        //            () =>
        //            {
        //                Time.timeScale = 1;
        //                multi_speed = 999f;
        //            }
        //        );
        //        break;
        //    }
        //}
    }
}
