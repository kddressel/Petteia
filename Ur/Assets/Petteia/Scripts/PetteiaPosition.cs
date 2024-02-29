using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetteiaPosition : MonoBehaviour
{
	private Vector2Int PosToArray(float y, float x) 
	{
		//todo: don't link this to constant numbers so resizing the board doesn't break everything
		return new Vector2Int(Mathf.RoundToInt((y + 3.25f) / -6.25f),  Mathf.RoundToInt((x - 3f) / 6.25f));
		
	}

	public Vector2Int Pos {
		get {
			return PosToArray(transform.position.z, transform.position.x);
		}
	}
}
