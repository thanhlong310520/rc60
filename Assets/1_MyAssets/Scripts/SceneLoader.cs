using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý việc load scene bất đồng bộ (async), có báo tiến độ (progress)
/// và event khi load xong. Gắn component này vào 1 object duy nhất
/// (nên đặt DontDestroyOnLoad) để dùng xuyên suốt game.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Cấu hình")]
    [Tooltip("Scene có tên này sẽ được hiển thị trong lúc load (loading screen), để trống nếu không dùng")]
    [SerializeField] private string loadingSceneName;

    [Tooltip("Có tự active scene ngay khi load xong 90% hay không")]
    [SerializeField] private bool autoActivate = true;

    // Bắn ra ngoài cho UI (thanh progress bar) lắng nghe
    public event Action<float> OnLoadProgress;      // 0 -> 1
    public event Action OnLoadStarted;
    public event Action<string> OnLoadCompleted;    // trả về tên scene đã load xong

    public bool IsLoading { get; private set; }

    private AsyncOperation pendingOperation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load 1 scene theo tên, bất đồng bộ.
    /// </summary>
    public void LoadScene(string sceneName, Action onCompleted = null)
    {
        if (IsLoading)
        {
            Debug.LogWarning("[SceneLoader] Đang load scene khác, bỏ qua yêu cầu mới.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName, onCompleted));
    }

    /// <summary>
    /// Load 1 scene theo build index, bất đồng bộ.
    /// </summary>
    public void LoadScene(int sceneBuildIndex, Action onCompleted = null)
    {
        if (IsLoading)
        {
            Debug.LogWarning("[SceneLoader] Đang load scene khác, bỏ qua yêu cầu mới.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneBuildIndex, onCompleted));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Action onCompleted)
    {
        yield return LoadSceneInternal(sceneName, onCompleted);
    }

    private IEnumerator LoadSceneRoutine(int sceneBuildIndex, Action onCompleted)
    {
        string sceneName = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
        yield return LoadSceneInternal(sceneName, onCompleted);
    }

    private IEnumerator LoadSceneInternal(string sceneName, Action onCompleted)
    {
        IsLoading = true;
        OnLoadStarted?.Invoke();

        // Nếu có loading scene riêng, hiện nó trước
        if (!string.IsNullOrEmpty(loadingSceneName))
        {
            Debug.Log("Load scene default ");
            yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        // Unity chỉ load tới 90% rồi dừng lại chờ allowSceneActivation = true
        while (operation.progress < 0.9f)
        {
            OnLoadProgress?.Invoke(operation.progress / 0.9f); // chuẩn hóa về khoảng 0 -> 1
            yield return null;
        }

        OnLoadProgress?.Invoke(1f);

        yield return null;
        if (autoActivate)
        {
            operation.allowSceneActivation = true;
        }
        else
        {
            // Chờ lệnh ActivateLoadedScene() được gọi từ bên ngoài (ví dụ khi bấm nút "Vào game")
            pendingOperation = operation;
            while (!operation.allowSceneActivation)
                yield return null;
            pendingOperation = null;
        }

        while (!operation.isDone)
            yield return null;

        // Dọn loading scene nếu có dùng
        if (!string.IsNullOrEmpty(loadingSceneName))
        {
            Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
            if (loadingScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(loadingScene);
        }

        IsLoading = false;
        OnLoadCompleted?.Invoke(sceneName);
        onCompleted?.Invoke();
    }

    /// <summary>
    /// Gọi hàm này khi muốn chủ động active scene sau khi đã load xong 90%
    /// (dùng khi autoActivate = false, ví dụ chờ người chơi bấm nút).
    /// </summary>
    public void ActivateLoadedScene()
    {
        if (pendingOperation != null)
        {
            pendingOperation.allowSceneActivation = true;
        }
    }
}
