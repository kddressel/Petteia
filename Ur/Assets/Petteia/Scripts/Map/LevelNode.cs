using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LevelState { Locked, Current, Completed }

public class LevelNode : MonoBehaviour
{
    public LevelDef LevelDef;
    public string Id => LevelDef.Id;

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

    private void OnPlayClick()
    {
        GameManager.LoadLevel(LevelDef);
    }

    private void OnInfoClick()
    {
        Debug.Log("Info clicked");
    }

    public void RefreshVisuals()
    {
        var root = nodeRoot ?? gameObject;

        root.SetActive(state != LevelState.Locked);
        if (lockedOverlay) lockedOverlay.SetActive(state == LevelState.Locked);
        if (currentOverlay) currentOverlay.SetActive(state == LevelState.Current);
        if (completedOverlay) completedOverlay.SetActive(state == LevelState.Completed);
    }
}
