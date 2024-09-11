using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetteiaPosition : MonoBehaviour
{
	public Vector2Int Pos {
		get {
			var mask = LayerMask.GetMask("GameSquare");
			Physics.queriesHitTriggers = true;
			var colliders = Physics.OverlapBox(transform.position, Vector3.one * 2, Quaternion.identity, mask);
			Physics.queriesHitTriggers = false;
			if (colliders.Length > 0)
      {
				var hitPiece = colliders.FirstOrDefault(c => c.GetComponent<PetteiaBoardPosition>() != null);
				if(hitPiece != null)
        {
					var boardPos = hitPiece.GetComponent<PetteiaBoardPosition>();
					return boardPos.position;
        }
      }

			throw new System.Exception("Unknown board position for " + this);
		}
	}
}
