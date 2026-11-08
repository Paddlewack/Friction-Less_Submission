using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("Drag your game scene asset here")]
    [SerializeField] private SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneToLoad = sceneAsset.name;
        }
    }
#endif

    [HideInInspector]
    [SerializeField] private string sceneToLoad;

    private Button _startButton;
    private Button _exitButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _startButton = root.Q<Button>("start-button");
        _exitButton = root.Q<Button>("exit-button");

        if (_startButton != null)
            _startButton.clicked += StartGame;

        if (_exitButton != null)
            _exitButton.clicked += ExitGame;
    }

    private void OnDisable()
    {
        if (_startButton != null)
            _startButton.clicked -= StartGame;

        if (_exitButton != null)
            _exitButton.clicked -= ExitGame;
    }

    private void StartGame()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("MainMenu: No scene assigned! Drag a scene into the Scene Asset field.");
            return;
        }

        Debug.Log("Loading scene: " + sceneToLoad);

        SceneManager.LoadScene(sceneToLoad);
    }

    private void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}