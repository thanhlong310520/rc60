using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;


public class MenuEditor
{
    [UnityEditor.MenuItem("Tools/Button Event Finder")]
    static void ButtonEventFinder()
    {
        var currentScene = SceneManager.GetActiveScene();
        for (int i = 0; i < currentScene.rootCount; i++)
        {
            GameObject obj = currentScene.GetRootGameObjects()[i];
            if (obj.name == "Canvas")
                ButtonFinder(obj.transform);
        }
    }

    static void ButtonFinder(Transform parent)
    {
        parent.ForChild((child) =>
        {
            var button = child.GetComponent<Button>();
            if (button != null)
                GetButtonEvents(button);
            ButtonFinder(child);
        });
    }

    static void GetButtonEvents(Button button)
    {
        int onClickEventsCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < onClickEventsCount; i++)
        {
            string methodName = button.onClick.GetPersistentMethodName(i);
            Object targetObject = button.onClick.GetPersistentTarget(i);
            Debug.Log($"Button in {button.gameObject.name}, Target: {targetObject}.{methodName}");
        }
    }


    [UnityEditor.MenuItem("Tools/Convert TextmeshPro -> Text")]
    static void ConvertToText()
    {
        var currentScene = SceneManager.GetActiveScene();
        for (int i = 0; i < currentScene.rootCount; i++)
        {
            GameObject obj = currentScene.GetRootGameObjects()[i];
            ReadChildObjects(obj.transform);
        }
    }

    [MenuItem("Tools/Scene/Loading %s1")]
    static void OpenLoadingScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
    }

    [MenuItem("Tools/Scene/Game %s2")]
    static void OpenGameScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
    }

    [MenuItem("Tools/Scene/Change %`")]
    static void ChangeScene()
    {
        var scene = SceneManager.GetActiveScene();
        switch(scene.name)
        {
            case "Menu":
                OpenGameScene();
                break;
            case "Game":
                OpenLoadingScene();
                break;
            default:
                OpenGameScene();
                break;
        }    

        Debug.Log("Active Scene is '" + scene.name + "'.");
    }

    static void ReadChildObjects(Transform parentTransform)
    {
        foreach (Transform childTransform in parentTransform)
        {
            GameObject childObject = childTransform.gameObject;
            var tmp = childObject.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                var _tex = tmp.text;
                var _color = tmp.color;
                var _fontSize = tmp.fontSize;

                Undo.DestroyObjectImmediate(tmp);
                Debug.Log("Component removed");

                var text = childObject.AddComponent<Text>();
                text.text = _tex;
                text.color = _color;
                text.fontSize = (int)_fontSize;

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Debug.Log("Scene saved.");
            }



            ReadChildObjects(childTransform);
        }
    }
}
