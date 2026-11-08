using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private UIDocument _document;

    // Pause Menu
    private Button _resumeButton;
    private Button _mainMenuButton;

    // Popup
    private VisualElement _popupContainer;

    // Health Bar
    private VisualElement _hullBarFill;
    private VisualElement _hullBarChaser;
    private Label _hullValueLight;
    private Label _hullValueDark;
    private VisualElement _shieldBarFill;
    private VisualElement _shieldBarChaser;
    private Label _shieldValueLight;
    private Label _shieldValueDark;

    // LoadoutContainer
    private VisualElement _loadoutContainer;

    private void Awake()
    {
        if (_document == null) Debug.LogError("[GameUIBridge] ERROR: No UI Document assigned in references.");
    }

    private void OnEnable()
    {
        VisualElement root = _document.rootVisualElement;

        _resumeButton = root.Q<Button>("resume-button");
        _mainMenuButton = root.Q<Button>("mainMenu-button");
        _resumeButton?.RegisterCallback<ClickEvent>(ResumeClicked);
        _mainMenuButton?.RegisterCallback<ClickEvent>(MainMenuClicked);

        _popupContainer = root.Q<VisualElement>("PopupContainer");
        HidePopup();

        _hullBarFill = root.Q<VisualElement>("HullBarFill");
        _hullBarChaser = root.Q<VisualElement>("HullBarChaser");
        _hullValueLight = root.Q<Label>("HullValueLight");
        _hullValueDark = root.Q<Label>("HullValueDark");
        _shieldBarFill = root.Q<VisualElement>("ShieldBarFill");
        _shieldBarChaser = root.Q<VisualElement>("ShieldBarChaser");
        _shieldValueLight = root.Q<Label>("ShieldValueLight");
        _shieldValueDark = root.Q<Label>("ShieldValueDark");

        _loadoutContainer = root.Q<VisualElement>("LoadoutContainer");
    }
    private void OnDisable()
    {
        _resumeButton?.UnregisterCallback<ClickEvent>(ResumeClicked);
        _mainMenuButton?.UnregisterCallback<ClickEvent>(MainMenuClicked);
    }

    private void ResumeClicked(ClickEvent evt)
    {
        GameManager.Instance.ResumeGame();
    }
    private void MainMenuClicked(ClickEvent evt)
    {
        GameManager.Instance.ExitMission();
    }

    public void UnhidePopup()
    {
        _popupContainer.RemoveFromClassList("popupHidden");
    }
    public void HidePopup()
    {
        _popupContainer.AddToClassList("popupHidden");
    }

    public void UpdateHealthBars(float hullCurrent, float hullMax, float shieldCurrent, float shieldMax)
    {
        if (_hullBarFill == null) return; // guards against missing elements in this document
        if (_shieldBarFill == null) return;
        float hullPct = Mathf.Clamp01(hullCurrent / hullMax) * 100f;
        float shielfPct = Mathf.Clamp01(shieldCurrent / shieldMax) * 100f;
        _hullBarFill.style.width = new StyleLength(new Length(hullPct, LengthUnit.Percent));
        _hullBarChaser.style.width = new StyleLength(new Length(hullPct, LengthUnit.Percent));
        _hullValueLight.text = Mathf.RoundToInt(hullCurrent).ToString();
        _hullValueDark.text = Mathf.RoundToInt(hullCurrent).ToString();
        _shieldBarFill.style.width = new StyleLength(new Length(shielfPct, LengthUnit.Percent));
        _shieldBarChaser.style.width = new StyleLength(new Length(shielfPct, LengthUnit.Percent));
        _shieldValueLight.text = Mathf.RoundToInt(shieldCurrent).ToString();
        _shieldValueDark.text = Mathf.RoundToInt(shieldCurrent).ToString();
    }
    public void SetLoadoutContainer(VisualElement LoadoutContainer)
    {
        _loadoutContainer = LoadoutContainer;
    }
    public VisualElement GetLoadoutContainer()
    {
        return _loadoutContainer;
    }
}
