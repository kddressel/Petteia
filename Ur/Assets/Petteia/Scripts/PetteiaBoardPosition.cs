using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetteiaBoardPosition : MonoBehaviour
{
	public Vector2Int position;
	public GameObject highlight;
	public PetteiaGameController pController;
	
	[HideInInspector] public bool occupied;

	private PetteiaPlayerPiece currentPiece;

    void Start()
    {
		highlight.SetActive(false);
    }

	void OnTriggerEnter(Collider other) 
	{
		if (other.CompareTag(pController.enemyTag) || other.CompareTag(pController.playerTag)) 
		{
			occupied = true;
			currentPiece = other.GetComponent<PetteiaPlayerPiece>();
		}
	}

	private void OnTriggerExit(Collider other) 
	{
		if (other.CompareTag(pController.enemyTag) || other.CompareTag(pController.playerTag)) 
		{
			occupied = false;
			currentPiece = null;
		}
	}

	/// <summary>
	/// Destroys the piece on this board position
	/// </summary>
	public void DestroyPiece() 
	{
		//Null check probably isn't necessary, but just in case
		if (currentPiece != null) 
		{
			//If this piece is part of the player piece array, it's a player piece and you have to do some special stuff
			if (pController.playerPieces.Contains(currentPiece)) 
			{
				currentPiece.GetComponent<PetteiaPlayerPiece>().DestroyDummy();
				pController.playerPieces.Remove(currentPiece);
			}
			Destroy(currentPiece.gameObject);
			pController.enemyAI.CheckPieces();
			currentPiece = null;
			occupied = false;
		}
		else {
			Debug.Log("CurrentPiece null");
		}
	}

	/// <summary>
	/// Toggles the visual highlight on this board position
	/// </summary>
	/// <param name="toggle"></param>
	public void HighlightSpace(bool toggle) 
	{
		highlight.SetActive(toggle);
	}
}

