using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Petteia.Scripts.Model;
using System.Runtime.InteropServices;

public enum AIDifficulty { VeryEasy, Easy, Medium, Hard, VeryHard, None }

public class GameManager : MonoBehaviour
{
    public AssetLib AssetLib;

    public Text loadingText;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public bool goToMainMenu = false;
    public bool loadLightingScene = false;

    [Header("Region")]
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    public AudioClip gameStartSound;

    private static Scene persistantScene;
    private static Scene persistentLightingScene;
    private static GameManager instance;
    public static GameManager Instance => instance;

    public SaveData SaveData { get; private set; }
    public PlayerRecord PlayerRecord { get; private set; }
    public void Save() => Saver.SaveToDisk(SaveData);

    public static AIDifficulty SelectedDifficulty { get; set; } = AIDifficulty.Hard;    // default to medium when playing outside of menu flow

    public static List<CrewMember> MasterCrewList { get; set; }
    public static PlayableCharacter SelectedCharacter { get; set; }
    public static List<string> UrFlavor { get; private set; }
    public static List<string> UrInsults { get; private set; }
    public static List<string> UrWinText { get; private set; }
    public static List<string> UrLoseText { get; private set; }
    public static List<string> UrRosetteText { get; private set; }
    public static List<string> UrFlipText { get; private set; }
    public static List<string> UrCaptureText { get; private set; }
    public static List<string> UrMoveOnText { get; private set; }
    public static List<string> UrMoveOffText { get; private set; }

    void Awake()
    {

        InitializeSave();

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            instance.SetLoadingText("");
            MasterCrewList = CSVLoader.LoadMasterCrewRoster();
            CSVLoader.LoadUrText();
            persistantScene = SceneManager.GetSceneByBuildIndex(0);

            AudioSettings.OnAudioConfigurationChanged += deviceWasChanged =>
            {
                Debug.LogWarning($"Audio device change detected: {deviceWasChanged}");
                bgmSource.Pause();
                bgmSource.UnPause();
            };
            if (loadLightingScene)
            {
                StartCoroutine(LoadScene(1));
            }
            if (goToMainMenu)
            {
                Debug.Log("go to main menu");
                LoadMainMenu();
            }
        }
    }

    public void InitializeSave()
    {
        // load save data if we have it
        SaveData = Saver.LoadFromDisk();

        // TODO: profile selection
        // for now, always create and select a single player
        if (SaveData.Players.Count == 0)
        {
            SaveData.Players.Add(new PlayerRecord());
        }
        PlayerRecord = SaveData.Players.First();
        Debug.Log("player record: " + PlayerRecord);
        Save();
    }

    public static void PlaySFX(AudioClip clip)
    {
        if (instance == null) return;

        instance.sfxSource.Stop();
        instance.sfxSource.volume = SettingsManager.MasterVolume * SettingsManager.SFXVolume;
        instance.sfxSource.clip = clip;
        instance.sfxSource.Play();
    }

    public static void PlayButtonHover()
    {
        PlaySFX(instance?.buttonHoverSound);
    }

    public static void PlayButtonClick()
    {
        PlaySFX(instance?.buttonClickSound);
    }

    public static void PlayMenuOpen()
    {
        PlaySFX(instance?.menuOpenSound);
    }

    public static void PlayMenuClosed()
    {
        PlaySFX(instance?.menuCloseSound);
    }

    public static void PlayGameStart()
    {
        PlaySFX(instance?.gameStartSound);
    }

    public static void SetTextLists(List<string> flavor, List<string> insults, List<string> win, List<string> lose, List<string> rosette, List<string> flip, List<string> capture,
        List<string> moveOn, List<string> moveOff)
    {
        UrFlavor = flavor;
        UrInsults = insults;
        UrWinText = win;
        UrLoseText = lose;
        UrRosetteText = rosette;
        UrFlipText = flip;
        UrCaptureText = capture;
        UrMoveOnText = moveOn;
        UrMoveOffText = moveOff;
    }

    //Since we're using Async scene loading, we need to do that through a coroutine
    //But it's going to be much, much more convenient if we can call these scene loaders through static methods
    //So we make this instance a static variable, then use that to call the coroutine
    //Since you're not allowed to start coroutines inside a static method usually

    //Instead of having the coroutine being public so it can be called from anywhere, I just made methods for each scene
    //There's so few scenes that this is entirely feasible for this project
    //Plus, having these as static methods means we won't have to use GameObject.FindWithTag to make this script a variable EVERYWHERE
    //(because this is in the master scene, we can't just assign it as a variable in the inspector)
    //We will still have to write new methods to call these from buttons, but that's much easier
    //I'm not even sure if you can start a coroutine through a button without another encapsulating normal method, I don't think you can
    public static void LoadMainMenu()
    {
        LoadMainMenu(MenuScreen.ScreenType.Title);
    }

    public static void LoadMainMenu(MenuScreen.ScreenType startScreen)
    {
        instance.StartCoroutine(LoadMenuCoroutine(startScreen));
    }

    static IEnumerator LoadMenuCoroutine(MenuScreen.ScreenType startScreen)
    {
        yield return instance.LoadScene(2);
        GameObject.FindObjectOfType<TitleScreenButtons>().EnableMenuScreen(startScreen, true);
        yield return new WaitForSeconds(1);
        if (startScreen == MenuScreen.ScreenType.Title && CameraSlider.StartPosition != CameraSlider.Position.Title)
        {
            GameObject.FindObjectOfType<CameraSlider>().SlideToTitlePos();
        }
        else if (startScreen != MenuScreen.ScreenType.Title && CameraSlider.StartPosition != CameraSlider.Position.Menu)
        {
            GameObject.FindObjectOfType<CameraSlider>().SlideToMenuPos();
        }
    }

    public static void LoadLevel(LevelDef def)
    {
        GameManager.SelectedDifficulty = def.Difficulty;
        
        RulesFactory.UseDiceRoll = (def.RuleSet & LevelDef.Rules.Dice) != 0;
        RulesFactory.UseKing = (def.RuleSet & LevelDef.Rules.King) != 0;
        RulesFactory.UsePlacePieces = (def.RuleSet & LevelDef.Rules.PlacePieces) != 0;

        CameraSlider.StartPosition = CameraSlider.Position.Menu;

        PetteiaGameController.LevelDef = def;

        LoadGamePlay();
    }

    public static void LoadGamePlay()
    {
        var gameToLoad = Input.GetKey(KeyCode.U) ? 3 : 4;       // hold U to load Ur, default loads petteia
        instance.StartCoroutine(LoadLevelCoroutine(gameToLoad));
    }

    static IEnumerator LoadLevelCoroutine(int sceneIndex)
    {
        yield return instance.LoadScene(sceneIndex);
        yield return new WaitForSeconds(1);
        GameObject.FindObjectOfType<CameraSlider>().SlideToGamePos();
    }

    Scene[] GetLoadedAdditiveScenes()
    {
        var sceneCount = SceneManager.sceneCount;
        List<Scene> loadedAdditiveScenes = new List<Scene>();
        for(var i = 0; i < sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != persistantScene.name && scene.name != persistentLightingScene.name)
            {
                loadedAdditiveScenes.Add(scene);
            }
        }
        return loadedAdditiveScenes.ToArray();
    }

    private IEnumerator LoadScene(int index)
    {
        //If you're loading a scene from the pause menu, timeScale is 0, so we need to reset it
        //Most of this will still work, but not the artificially inflated loading
        Time.timeScale = 1;
        if (SceneManager.GetSceneByBuildIndex(index) == null)
        {
            Debug.Log($"Scene at index {index} is null");
        }

        if(index == 1)
        {
            Debug.Log("Loading lighting scene");
            yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            persistentLightingScene = SceneManager.GetSceneByBuildIndex(index);
            SceneManager.SetActiveScene(persistentLightingScene);
        }
        else
        {
            foreach(var scene in GetLoadedAdditiveScenes())
            {
                Debug.Log("Unloading " + scene.name);
                yield return SceneManager.UnloadSceneAsync(scene);
            }

            yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }
    }

    private void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }
}
