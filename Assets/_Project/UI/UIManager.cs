using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UIDocumentBridge uiDocBridge;
    [SerializeField] private SelectedPopupData selectedPopupData;
    [SerializeField] private PauseMenuData pauseMenuData;
    [SerializeField] private VisualTreeAsset weaponContainerTemplate;

    private VisualElement loadoutContainer;
    private PlayerTankController playerTank;

    // Weapon UI tracking
    private class WeaponUIElement
    {
        public WeaponInstance weapon;
        public VisualElement container;
        public Label nameLabel;
        public Label ammoLabel;
        public Image iconImage;
        public VisualElement reloadIndicator;
    }
    private WeaponUIElement[] weaponElements = new WeaponUIElement[3];

    private Coroutine _reloadProgressCoroutine;

    private void Awake()
    {
        Instance = this;
        ResetPauseMenu();
        ResetPopup();
        DebugLogConsole.AddCommand("showPopup", "Forcefully shows popup UI", ShowPopup);
        DebugLogConsole.AddCommand("hidePopup", "Forcefully hides popup UI", CancelPopup);
    }
    private void Start()
    {
        loadoutContainer = uiDocBridge.GetLoadoutContainer();
    }

    #region Public Methods
    public void OnPause()
    {
        if (pauseMenuData.visibility == Visibility.Visible) return;
        ShowPauseMenu();
    }
    public void OnResume()
    {
        if (pauseMenuData.visibility == Visibility.Hidden) return;
        HidePauseMenu();
    }

    public void InitializeWeaponUI(PlayerTankController tank)
    {
        playerTank = tank;

        if (playerTank?.weaponSystem == null)
        {
            Debug.LogError("[UIManager] PlayerTankController or WeaponSystem is null!");
            return;
        }

        // Set up UI for all three weapon slots
        SetupWeaponUI(WeaponSlot.Primary, 0);
        SetupWeaponUI(WeaponSlot.Secondary, 1);
        SetupWeaponUI(WeaponSlot.Auxiliary, 2);

        // Subscribe to weapon system events
        playerTank.weaponSystem.onAmmoChanged.AddListener(OnAmmoChanged);
        playerTank.weaponSystem.onWeaponSwitched.AddListener(OnWeaponSwitched);
        playerTank.weaponSystem.onReloadStart.AddListener(OnReloadStart);
        playerTank.weaponSystem.onReloadComplete.AddListener(OnReloadComplete);

        RefreshLoadoutUI();
    }

    #endregion

    #region PauseMenu Methods

    private void ShowPauseMenu()
    {
        pauseMenuData.visibility = Visibility.Visible;
    }
    private void HidePauseMenu()
    {
        pauseMenuData.visibility = Visibility.Hidden;
    }
    private void ResetPauseMenu()
    {
        pauseMenuData.visibility = Visibility.Hidden;
    }
    #endregion

    #region Popup Methods

    private Coroutine _popupCoroutine;
    private bool _popupIsActive;
    private const float PopupTransitionDuration = 1.5f;

    public void NewPopup(PopupData newPopupData, float displayTime)
    {
        if (displayTime < 0)
        {
            Debug.LogError("[UI MANAGER] ERROR: Received a NewPopup with an invalid display time! (less than 0)");
            return;
        }

        if (_popupCoroutine != null)
            StopCoroutine(_popupCoroutine);

        _popupCoroutine = StartCoroutine(PopupRoutine(newPopupData, displayTime));
    }

    private IEnumerator PopupRoutine(PopupData newData, float displayTime)
    {
        if (_popupIsActive)
        {
            HidePopup();
            _popupIsActive = false;
            yield return new WaitForSeconds(PopupTransitionDuration);
        }

        selectedPopupData.popupData = newData;
        ShowPopup();
        _popupIsActive = true;

        if (displayTime > 0)
        {
            yield return new WaitForSeconds(displayTime);
            HidePopup();
            ResetPopup();
            _popupIsActive = false;
        }

        _popupCoroutine = null;
    }

    public void CancelPopup()
    {
        if (_popupCoroutine != null)
        {
            StopCoroutine(_popupCoroutine);
            _popupCoroutine = null;
        }

        if (_popupIsActive)
        {
            HidePopup();
            _popupIsActive = false;
        }

        ResetPopup();
    }

    private void ShowPopup() => uiDocBridge.UnhidePopup();
    private void HidePopup() => uiDocBridge.HidePopup();
    private void ResetPopup() => selectedPopupData.popupData = null;

    #endregion

    #region Health Bar Methods

    public void UpdateHealthBars(float hullCurrent, float hullMax, float shieldCurrent, float shieldMax)
    {
        uiDocBridge.UpdateHealthBars(hullCurrent, hullMax, shieldCurrent, shieldMax);
    }

    #endregion

    #region Weapon UI Methods

    private void SetupWeaponUI(WeaponSlot slot, int index)
    {
        WeaponInstance weapon = playerTank.weaponSystem.GetWeaponBySlot(slot);
        if (weapon == null) return;

        weaponElements[index] = new WeaponUIElement { weapon = weapon };

        // Query for the Weapon instance by the name attribute (Weapon0, Weapon1, Weapon2)
        VisualElement containerInstance = loadoutContainer.Q<VisualElement>($"Weapon{index}");

        if (containerInstance == null)
        {
            Debug.LogError($"[UIManager] Could not find Weapon{index} in LoadoutContainer!");
            return;
        }

        weaponElements[index].container = containerInstance;

        // Query for Details and Icon within this specific instance
        var weaponDetails = containerInstance.Q<VisualElement>("Details");
        if (weaponDetails != null)
        {
            weaponElements[index].nameLabel = weaponDetails.Q<Label>("Name");
            weaponElements[index].ammoLabel = weaponDetails.Q<Label>("Ammo");
        }

        weaponElements[index].iconImage = containerInstance.Q<Image>("Icon");
        weaponElements[index].reloadIndicator = containerInstance.Q<VisualElement>("ReloadIndicator");

        // Update existing element values
        if (weaponElements[index].nameLabel != null)
            weaponElements[index].nameLabel.text = weapon.WeaponName;

        if (weaponElements[index].ammoLabel != null)
            weaponElements[index].ammoLabel.text = $"Ammo: {weapon.AmmoCurrent}/{weapon.AmmoMax}";

        if (weaponElements[index].iconImage != null)
            weaponElements[index].iconImage.sprite = weapon.WeaponIcon;

        // Initialize indicator to full height
        if (weaponElements[index].reloadIndicator != null)
            ResetWeaponIndicator(index);
    }

    private void OnAmmoChanged(int current, int max)
    {
        // Update the current weapon's ammo display
        WeaponInstance currentWeapon = playerTank.weaponSystem.CurrentWeapon;
        foreach (var element in weaponElements)
        {
            if (element?.weapon == currentWeapon && element.ammoLabel != null)
            {
                element.ammoLabel.text = $"Ammo: {current}/{max}";
                break;
            }
        }
    }

    private void OnWeaponSwitched(WeaponInstance newWeapon)
    {
        Debug.Log($"[UIManager] Switched to {newWeapon.WeaponName}");
        // Highlight the current weapon in UI
        HighlightCurrentWeapon();
    }

    private void OnReloadStart(float reloadTime)
    {
        Debug.Log($"[UIManager] Reload started for {reloadTime}s");

        if (_reloadProgressCoroutine != null)
            StopCoroutine(_reloadProgressCoroutine);

        _reloadProgressCoroutine = StartCoroutine(UpdateReloadProgress(reloadTime));
    }

    private void OnReloadComplete()
    {
        Debug.Log("[UIManager] Reload complete");

        if (_reloadProgressCoroutine != null)
        {
            StopCoroutine(_reloadProgressCoroutine);
            _reloadProgressCoroutine = null;
        }

        // Reset all weapon indicator heights to full
        for (int i = 0; i < weaponElements.Length; i++)
        {
            ResetWeaponIndicator(i);
        }
    }

    private IEnumerator UpdateReloadProgress(float reloadTime)
    {
        float elapsedTime = 0f;
        WeaponInstance currentWeapon = playerTank.weaponSystem.CurrentWeapon;

        // Find the index of the current weapon
        int currentWeaponIndex = -1;
        for (int i = 0; i < weaponElements.Length; i++)
        {
            if (weaponElements[i]?.weapon == currentWeapon)
            {
                currentWeaponIndex = i;
                break;
            }
        }

        if (currentWeaponIndex == -1 || weaponElements[currentWeaponIndex]?.reloadIndicator == null)
            yield break;

        VisualElement indicator = weaponElements[currentWeaponIndex].reloadIndicator;

        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / reloadTime;

            float heightPercentage = progress * 100f;

            indicator.style.height = new Length(heightPercentage, LengthUnit.Percent);

            yield return null;
        }

        indicator.style.height = new Length(100f, LengthUnit.Percent);
    }

    private void ResetWeaponIndicator(int index)
    {
        if (weaponElements[index]?.reloadIndicator != null)
        {
            weaponElements[index].reloadIndicator.style.height = new Length(100f, LengthUnit.Percent);
        }
    }

    private void HighlightCurrentWeapon()
    {
        foreach (var element in weaponElements)
        {
            if (element?.container != null)
            {
                if (element.weapon == playerTank.weaponSystem.CurrentWeapon)
                {
                    element.container.AddToClassList("active-weapon");
                }
                else
                {
                    element.container.RemoveFromClassList("active-weapon");
                }
            }
        }
    }

    private void RefreshLoadoutUI()
    {
        loadoutContainer.Clear();
        foreach (var element in weaponElements)
        {
            if (element?.container != null)
                loadoutContainer.Add(element.container);
        }
        uiDocBridge.SetLoadoutContainer(loadoutContainer);
    }

    #endregion
}