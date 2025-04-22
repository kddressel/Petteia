
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelRecord
{
	public string Id;
	public bool Cleared;
	public bool Draw;
	public float Time;
	public int NumPiecesLeft;
}

public class SaveRecord
{
	public LevelRecord[] LevelRecords;
	
	public bool IsLevelCleared(string id) => LevelRecords.Any(r => r.Id == id && r.Cleared);
}

public class LevelMapPathDot : MonoBehaviour
{
	public IEnumerator Activate()
  {
		yield return null;
		ActivateInstant();
  }

	public IEnumerator Deactivate()
	{
		yield return null;
		DeactivateInstant();
	}

	public void ActivateInstant()
  {
		gameObject.SetActive(true);
	}

	public void DeactivateInstant()
  {
		gameObject.SetActive(true);
	}
}

public class LevelMapPath
{
	public LevelMapNode Start;
	public LevelMapNode End;

	public LevelMapPathDot[] Dots;

	public IEnumerator Activate()
  {
		foreach(var dot in Dots)
    {
			yield return dot.Activate();
    }
	}

	public IEnumerator Deactivate()
	{
		foreach (var dot in Dots)
		{
			yield return dot.Deactivate();
		}
	}

	public void ActivateInstant()
  {
		foreach (var dot in Dots)
		{
			dot.ActivateInstant();
		}
	}

	public void DeactivateInstant()
  {
		foreach(var dot in Dots)
    {
			dot.DeactivateInstant();
    }
  }
}

public class LevelMapNode : MonoBehaviour
{
	public string Id;
	public LevelMapNode[] Parents;
	public LevelMapNode[] Children;

	public IEnumerator Activate()
	{
		yield return null;
		ActivateInstant();
	}

	public IEnumerator Deactivate()
	{
		yield return null;
		DeactivateInstant();
	}

	public void ActivateInstant()
	{
		gameObject.SetActive(true);
	}

	public void DeactivateInstant()
	{
		gameObject.SetActive(true);
	}
}

public class LevelMap
{
	LevelMapNode _firstNode;
	Dictionary<string, LevelMapNode> _allNodes = new Dictionary<string, LevelMapNode>();

	bool IsLevelCleared(SaveRecord record, LevelMapNode node) => record.IsLevelCleared(node.Id);


	void RefreshNodesInstant(SaveRecord record)
  {
		var stack = new Stack<LevelMapNode>();
		stack.Push(_firstNode);

		while(stack.Count > 0)
    {
			var curr = stack.Pop();

			var unlocked = curr.Parents.All(p => IsLevelCleared(record, p));
			if(unlocked)
      {
				// TODO: maybe an animation when you first unlock a level
				curr.ActivateInstant();
      }
			else
      {
				curr.DeactivateInstant();
      }

			
			foreach(var child in curr.Children)
      {
				stack.Push(child);
      }
    }
  }
}

