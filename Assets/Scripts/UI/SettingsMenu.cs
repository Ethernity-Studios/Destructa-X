using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    private PlayerInput input;

    public bool IsOpened;
    [SerializeField] GameObject SettingMenu;

    [HideInInspector]
    public PlayerEconomyManager playerEconomyManager;

    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    private void Awake()
    {
        input = new PlayerInput();
    }

    private void Start()
    {
        input.PlayerUI.Menu.performed += toggleMenu;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == UnityEngine.Device.Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnEnable()
    {
        input.PlayerUI.Enable();
    }

    private void OnDisable()
    {
        input.PlayerUI.Disable();
    }


    void toggleMenu(InputAction.CallbackContext context)
    {
        if(playerEconomyManager.IsShopOpen) playerEconomyManager.CloseShopUI();
        else IsOpened = !IsOpened;

        if (IsOpened)
        {
            Cursor.lockState = CursorLockMode.None;
            SettingMenu.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            SettingMenu.SetActive(false);
        }
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void ToggleFullscreen(bool state)
    {
        Screen.fullScreen = state;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}