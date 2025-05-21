using System.Collections.Generic;
using UnityEngine;

public enum LevelState { Locked, Current, Completed }

public class LevelNode : MonoBehaviour
{
  public string Id;

  public GameObject nodeRoot;
  public GameObject lockedOverlay;
  public GameObject currentOverlay;
  public GameObject completedOverlay;

  public List<LevelNode> nextNodes = new();
  public List<GameObject> pathPebbles = new();

  [HideInInspector] public LevelState state = LevelState.Locked;

  public void RefreshVisuals()
  {
    var root = nodeRoot ?? gameObject;

    root.SetActive(state != LevelState.Locked);
    if (lockedOverlay) lockedOverlay.SetActive(state == LevelState.Locked);
    if (currentOverlay) currentOverlay.SetActive(state == LevelState.Current);
    if (completedOverlay) completedOverlay.SetActive(state == LevelState.Completed);
  }
}
