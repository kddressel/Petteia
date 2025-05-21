using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenButtons : MenuButtons
{
    public MenuScreen[] extraScreens;
    public Button gameStartButton;

    private void OnEnable()
    {
        PlayerSelection += EnableGameStart;

        GameManager.SelectedCharacter = null;
        GameManager.SelectedDifficulty = AIDifficulty.None;
        gameStartButton.interactable = false;
    }

    private void OnDisable()
    {
        PlayerSelection -= EnableGameStart;
    }

    public void EnableLevelSelect(bool enabled)
    {
        HideAllBut(MenuScreen.ScreenType.LevelSelect);
        GameObject.FindObjectOfType<CameraSlider>().SlideToMenuPos();

        // TODO: enable the file select screen. for now we're going straight to level seslect
        EnableMenuScreen(MenuScreen.ScreenType.LevelSelect, enabled);
        //EnableMenuScreen(MenuScreen.ScreenType.CharacterSelector, enabled);
    }

    public void EnableSettings(bool enabled)
    {
        EnableMenuScreen(MenuScreen.ScreenType.Settings, enabled);
        if (enabled && !initialized)
        {
            InitializeSettings();
        }
    }

    public void EnableHistory(bool enabled)
    {
        EnableMenuScreen(MenuScreen.ScreenType.History, enabled);
    }

    public void EnableCredits(bool enabled)
    {
        EnableMenuScreen(MenuScreen.ScreenType.Credits, enabled);
    }

    public void EnableStats(bool enabled)
    {
        EnableMenuScreen(MenuScreen.ScreenType.Stats, enabled);
    }

    public void HideAllBut(MenuScreen.ScreenType type)
    {
        // hide others
        foreach (var x in extraScreens)
        {
            if (x.type != type)
            {
                x.menuBox.CloseMenu();
            }
        }
    }

    public void EnableMenuScreen(MenuScreen.ScreenType type, bool activate)
    {
        if (type == MenuScreen.ScreenType.LevelSelect)
        {
            HideAllBut(MenuScreen.ScreenType.LevelSelect);
        }

        extraScreens.FirstOrDefault(x => x.type == type).menuBox.EnableAnimation(activate);
        //modalBlocker.SetActive(activate);
    }

    private void EnableGameStart()
    {
        if (GameManager.SelectedCharacter != null)
        {
            gameStartButton.interactable = true;
        }
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        SaveManager.RestoreDefaults();
        SettingsManager.InitializeSettings();
    }
}
