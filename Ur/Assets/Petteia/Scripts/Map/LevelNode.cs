using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelState { Locked, Current, Completed }

public class LevelNode : MonoBehaviour
{
    public LevelDef LevelDef;
    public string Id => LevelDef.Id;

    public Image CrewPortrait;
    public TextMeshProUGUI CrewName;

    public GameObject nodeRoot;
    public GameObject lockedOverlay;
    public GameObject currentOverlay;
    public GameObject completedOverlay;

    public List<LevelNode> nextNodes = new();
    public List<GameObject> pathPebbles = new();

    public Button button;
    public Button infoButton;

    [HideInInspector] public LevelState state = LevelState.Locked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnPlayClick);
        }
        if (infoButton != null)
        {
            infoButton.onClick.AddListener(OnInfoClick);
        }
    }

    string GetLevelInfoText()
    {
        var crew = GameManager.MasterCrewList.FirstOrDefault(crew => crew.Id == LevelDef.CrewId);
        var text = "<b>" + crew.CrewName + "</b>";
        text += "\n" + LevelDef.Difficulty;
        if(LevelDef.Personality != LevelDef.PersonalityType.None)
        {
            text += "\n" + LevelDef.Personality;
        }
        text += "\nSpaces: " + LevelDef.MaxRoll;
        text += "\n" + LevelDef.Description;
        return text;
    }

    private void OnPlayClick()
    {
        GameObject.FindObjectOfType<TitleScreenButtons>().ShowTextDisplayBox(GetLevelInfoText(), () => GameManager.LoadLevel(LevelDef), () => { });
    }

    private void OnInfoClick()
    {
        GameObject.FindObjectOfType<TitleScreenButtons>().ShowTextDisplayBox(GetLevelInfoText(), () => { }, () => { });
    }

    public void RefreshVisuals()
    {
        var root = nodeRoot ?? gameObject;

        root.SetActive(state != LevelState.Locked);
        if (lockedOverlay) lockedOverlay.SetActive(state == LevelState.Locked);
        if (currentOverlay) currentOverlay.SetActive(state == LevelState.Current);
        if (completedOverlay) completedOverlay.SetActive(state == LevelState.Completed);

        var crew = GameManager.MasterCrewList.FirstOrDefault(crew => crew.Id == LevelDef.CrewId);
        if (crew == null) Debug.LogError("Missing crew " + LevelDef.CrewId);
        CrewName.text = "<b>" + crew.CrewName + "</b>\n" + LevelDef.Difficulty + "\nSpaces: " + LevelDef.MaxRoll;
        if(LevelDef.Personality != LevelDef.PersonalityType.None)
        {
            CrewName.text += "\n" + LevelDef.Personality;
        }
        CrewPortrait.sprite = crew.CrewPortrait;
    }
}
