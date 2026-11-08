using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [SerializeField, HideInInspector] private string menuSceneName;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuScene;

    private void OnValidate()
    {
        if (menuScene != null)
            menuSceneName = menuScene.name;
    }
#endif

    public void LoadScene()
    {
        SceneManager.LoadScene(menuSceneName);
    }
    [Header("References")]

    public static GameManager Instance { get; private set; }

    private InputSystem_Actions _actions;
    public InputSystem_Actions Actions { get { return _actions; } }

    private bool _isPlaying = false;
    private void Awake()
    {
        if (menuSceneName == null) Debug.LogWarning("[GameManager] WARNING: Menu Scene has not been assigned on Game Manager");
        Instance = this;
        _actions = new InputSystem_Actions();
        _actions.Enable();
        _actions.Global.Enable(); // Always enabled action map for input that should work regardless of current action map (Pausing, etc.)
        _isPlaying = true;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        SetPlayerControls(); // Assumes starting in a mission scene
        FMODManager.Instance.PlayWarAmbienceSound();
    }
    private void OnEnable(){
        _actions.Global.Exit.performed += OnPausePerformed;
    
    }
    private void OnDisable(){
        _actions.Global.Exit.performed -= OnPausePerformed;
    }

    #region Control State Management
    private void SetUIControls()
    {
        _actions.Player.Disable();
        _actions.UI.Enable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void SetPlayerControls()
    {
        _actions.Player.Enable();
        _actions.UI.Disable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Pause Management
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (_isPlaying) PauseGame();
        else ResumeGame();
    }
    public void PauseGame()
    {
        _isPlaying = false;
        Time.timeScale = 0f;
        SetUIControls();
        UIManager.Instance.OnPause();
    }
    public void ResumeGame()
    {
        _isPlaying = true;
        Time.timeScale = 1f;
        SetPlayerControls();
        UIManager.Instance.OnResume();
    }

    #endregion

    #region Scene Management
    public void ExitMission()
    {
        FMODManager.Instance.StopAllSounds();
        SetUIControls();
        SceneManager.LoadScene(menuSceneName);
    }
    #endregion

    private void OnDestroy()
    {
        _actions.Global.Exit.performed -= OnPausePerformed;
        _actions.Disable();
        _actions.Dispose();
    }
}
