using System.Collections.Generic;
using UnityEngine;

public class LevelMap : MonoBehaviour
{
  [SerializeField] private LevelNode startingNode;

  private void OnEnable() => RefreshMap();

  public void SetCurrent(LevelNode newCurrent)
  {
    var player = GameManager.Instance?.PlayerRecord;
    player.CurrentLevel = newCurrent.Id;
    RefreshMap();
  }

  private void RefreshMap()
  {
    if (startingNode == null) return;

    var player = GameManager.Instance?.PlayerRecord;
    string currentId = player?.CurrentLevel ?? startingNode.Id;

    ApplyStates(startingNode, currentId, player, new HashSet<LevelNode>());
  }

  private void ApplyStates(LevelNode node, string currentId, PlayerRecord player, HashSet<LevelNode> visited)
  {
    if (!visited.Add(node)) return;

    bool isCurrent = node.Id == currentId;
    bool isCleared = player != null && player.IsLevelCleared(node.Id);

    node.state = isCurrent ? LevelState.Current
               : isCleared ? LevelState.Completed
               : LevelState.Locked;

    node.RefreshVisuals();

    bool unlockChildren = isCurrent || isCleared;

    for (int i = 0; i < node.nextNodes.Count; i++)
    {
      if (i < node.pathPebbles.Count && node.pathPebbles[i])
        node.pathPebbles[i].SetActive(unlockChildren);

      ApplyStates(node.nextNodes[i], currentId, player, visited);
    }
  }

  private bool IsReachable(LevelNode start, LevelNode target)
  {
    var stack = new Stack<LevelNode>();
    var visited = new HashSet<LevelNode>();

    stack.Push(start);
    while (stack.Count > 0)
    {
      var n = stack.Pop();
      if (!visited.Add(n)) continue;
      if (n == target) return true;

      foreach (var next in n.nextNodes)
        stack.Push(next);
    }
    return false;
  }

  private LevelNode FindNodeById(LevelNode start, string id)
  {
    var stack = new Stack<LevelNode>();
    var visited = new HashSet<LevelNode>();

    stack.Push(start);
    while (stack.Count > 0)
    {
      var n = stack.Pop();
      if (!visited.Add(n)) continue;
      if (n.Id == id) return n;

      foreach (var next in n.nextNodes)
        stack.Push(next);
    }
    return null;
  }
}
