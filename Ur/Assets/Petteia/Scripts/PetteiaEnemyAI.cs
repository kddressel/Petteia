//Paul Reichling
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PetteiaEnemyAI : MonoBehaviour
{
	public List<PetteiaEnemyPiece> pieces;
	private PetteiaEnemyPiece currentPiece;
	private int movementDistance = 0;
	
	private PetteiaGameController pController;

	void Start()
    {
		pController = GetComponent<PetteiaGameController>();
		
	}

	/// <summary>
	/// Starts the enemy looking for a move to make
	/// </summary>
	public void StartEnemyTurn() 
	{
		if (!pController.GameOver) 
		{
			StartCoroutine(MakeMove());
		}
	}

	/// <summary>
	/// Checks if any pieces have been destroyed and need to be removed from the list
	/// </summary>
	/// <returns>Returns 1 once finished</returns>
	public int CheckPieces() 
	{
		for (int i = pieces.Count - 1; i >= 0; i--) 
		{
			if (pieces[i] == null) 
			{
				pieces.RemoveAt(i);
			}
		}
		return 1;
	}

	/// <summary>
	/// Chooses which piece to move and where to move it to
	/// </summary>
	/// <returns></returns>
	IEnumerator MakeMove() {
		//This method was written by Paul Reichling, and since I (Kylie Gilde) don't use goto, I don't really follow it
		//I'm not going to comment it because I'm not fulling sure what's going on and I don't want to be misleading
		//Anyway, it seems to work as-is
		//I have made a few changes - namely checking for null pieces and streamlining some redundant function calls with variables
		//but other than that it's untouched

		yield return new WaitForSeconds(1f);

		//In case it took that extra second to fully register that the game is over
		if (!pController.GameOver) 
		{
			string s = "";

			PetteiaEnemyPiece pieceToMove = null;
			//Checks for available captures to make
			foreach (PetteiaEnemyPiece p in pieces) {

				//If there's still a null piece, don't move it
				if (p == null) {
					break;
				}

				currentPiece = p;
				PetteiaPosition piecePos = p.GetComponent<PetteiaPosition>();

				//moving up loop
				for (int x = piecePos.Pos.x; x > 2; x--) {
					//If this position isn't empty we can't move to it
					if (pController.positions[x, piecePos.Pos.y] != 0 && x != piecePos.Pos.x) {
						break;
					}

					if (pController.positions[x - 1, piecePos.Pos.y] == 2
						&& pController.positions[x - 2, piecePos.Pos.y] == 1
						&& pController.positions[x, piecePos.Pos.y] == 0) {
						//look up while moving
						pieceToMove = p;
						s = "up";
						movementDistance = piecePos.Pos.x - x;

						goto End;
					}

					if (piecePos.Pos.y <= 5) {
						//look right while moving 
						if (pController.positions[x, 1 + piecePos.Pos.y] == 2
							&& pController.positions[x, 2 + piecePos.Pos.y] == 1
							&& pController.positions[x, piecePos.Pos.y] == 0) {
							pieceToMove = p;
							s = "up";
							movementDistance = piecePos.Pos.x - x;

							goto End;
						}
					}
					if (piecePos.Pos.y >= 2) {
						//look left while moving 
						if (pController.positions[x, piecePos.Pos.y - 1] == 2
							&& pController.positions[x, piecePos.Pos.y - 2] == 1
							&& pController.positions[x, piecePos.Pos.y] == 0) {
							pieceToMove = p;
							s = "up";
							movementDistance = piecePos.Pos.x - x;

							goto End;
						}

					}
				}

				//////////////////////////////////////////////////////////////////////////////////////////

				//moving down loop
				for (int x = piecePos.Pos.x; x < 5; x++) {
					if (pController.positions[x, piecePos.Pos.y] != 0 && x != piecePos.Pos.x) {
						break;
					}
					if (pController.positions[x + 1, piecePos.Pos.y] == 2
						&& pController.positions[x + 2, piecePos.Pos.y] == 1
						&& pController.positions[x, piecePos.Pos.y] == 0) {
						pieceToMove = p;
						s = "down";
						movementDistance = x - piecePos.Pos.x;

						goto End;
					}

					if (piecePos.Pos.y <= 5) {
						//look right while moving 
						if (pController.positions[x, 1 + piecePos.Pos.y] == 2
							&& pController.positions[x, 2 + piecePos.Pos.y] == 1
							&& pController.positions[x, piecePos.Pos.y] == 0) {
							pieceToMove = p;
							s = "down";
							movementDistance = x - piecePos.Pos.x;

							goto End;
						}
					}
					if (piecePos.Pos.y >= 2) {
						//look left while moving 
						if (pController.positions[x, piecePos.Pos.y - 1] == 2
							&& pController.positions[x, piecePos.Pos.y - 2] == 1
							&& pController.positions[x, piecePos.Pos.y] == 0) {
							pieceToMove = p;
							s = "down";
							movementDistance = x - piecePos.Pos.x;

							goto End;
						}
					}
				}

				////////////////////////////////////////////////////////////////////////////////////////

				//moving right loop
				for (int y = piecePos.Pos.y; y < 5; y++) {
					if (pController.positions[piecePos.Pos.x, y] != 0 && y != piecePos.Pos.y) {
						break;
					}

					if (pController.positions[piecePos.Pos.x, y + 1] == 2
						&& pController.positions[piecePos.Pos.x, y + 2] == 1
						&& pController.positions[piecePos.Pos.x, y] == 0) {
						pieceToMove = p;
						s = "right";
						movementDistance = y - piecePos.Pos.y;

						goto End;
					}

					if (piecePos.Pos.x >= 2) {
						//look up while moving 
						if (pController.positions[piecePos.Pos.x - 1, y] == 2
							&& pController.positions[piecePos.Pos.x - 2, y] == 1
							&& pController.positions[piecePos.Pos.x, y] == 0) {
							pieceToMove = p;
							s = "right";
							movementDistance = y - piecePos.Pos.y;

							goto End;
						}
					}
					if (piecePos.Pos.x <= 5) {
						//look down while moving 
						if (pController.positions[piecePos.Pos.x + 1, y] == 2
							&& pController.positions[piecePos.Pos.x + 2, y] == 1
							&& pController.positions[piecePos.Pos.x, y] == 0) {
							pieceToMove = p;
							s = "right";
							movementDistance = y - piecePos.Pos.y;

							goto End;
						}
					}
				}

				////////////////////////////////////////////////////////////////////////////////////////

				//moving left loop
				for (int y = piecePos.Pos.y; y > 2; y--) {
					if (pController.positions[piecePos.Pos.x, y] != 0 && y != piecePos.Pos.y) {
						break;
					}

					if (pController.positions[piecePos.Pos.x, y - 1] == 2
						&& pController.positions[piecePos.Pos.x, y - 2] == 1
						&& pController.positions[piecePos.Pos.x, y] == 0) {
						pieceToMove = p;
						s = "left";
						movementDistance = piecePos.Pos.y - y;

						goto End;
					}

					if (piecePos.Pos.x >= 2) {
						//look up while moving 
						if (pController.positions[piecePos.Pos.x - 1, y] == 2
							&& pController.positions[piecePos.Pos.x - 2, y] == 1
							&& pController.positions[piecePos.Pos.x, y] == 0) {
							pieceToMove = p;
							s = "left";
							movementDistance = piecePos.Pos.y - y;

							goto End;
						}
					}
					if (piecePos.Pos.x <= 5) {
						//look down while moving 
						if (pController.positions[piecePos.Pos.x + 1, y] == 2
							&& pController.positions[piecePos.Pos.x + 2, y] == 1
							&& pController.positions[piecePos.Pos.x, y] == 0) {
							pieceToMove = p;
							s = "left";
							movementDistance = piecePos.Pos.y - y;

							goto End;
						}
					}
				}
			}

		End:
			if (pieceToMove != null) {
				yield return StartCoroutine(MovePiece(pieceToMove.gameObject, s, movementDistance));
			}
			else {
				// Moves the piece randomly 1-3 spaces if it cannot find a capture. 
				int tries = 0;
			Rand:
				bool trying = false;
				movementDistance = 0;

				while (trying == false && tries < 50) {
					tries++;
					int findPieceTries = 0;
					int direction = Random.Range(0, 4);
					//If somehow there's still a null piece in here, make sure it doesn't get picked
					//100 is probably WAY higher than it needs to be, but just to be safe
					do {
						pieceToMove = pieces.RandomElement();
						findPieceTries++;
					} while (pieceToMove == null && findPieceTries < 100);

					if (findPieceTries >= 100) {
						Debug.Log("Found 100 null pieces in a row, uhoh!");
					}

					if (pieceToMove == null) {
						Debug.Log("Tried to move a null piece, whoopsies!");
					}
					currentPiece = pieceToMove;

					////////////////////////////////////////////////////////////////////////
					PetteiaPosition movingPiecePos = pieceToMove.GetComponent<PetteiaPosition>();

					if (direction == 0) {
						s = "up";
						if (movingPiecePos.Pos.x - 1 > 0) {
							if (pController.positions[movingPiecePos.Pos.x - 1, movingPiecePos.Pos.y] == 0) {
								trying = true;
								//can move 1 space
								movementDistance = 1;

								if (movingPiecePos.Pos.x - 2 > 0) {
									if (pController.positions[movingPiecePos.Pos.x - 2, movingPiecePos.Pos.y] == 0) {
										//can move 2 spaces
										movementDistance = Random.Range(1, 3);

										if (movingPiecePos.Pos.x - 3 > 0) {
											if (pController.positions[movingPiecePos.Pos.x - 3, movingPiecePos.Pos.y] == 0) {
												//can move 3 spaces
												movementDistance = Random.Range(1, 4);
											}
											else {
												trying = true;
												movementDistance = Random.Range(1, 3);
												break;
											}
										}
									}
									else {
										trying = true;
										movementDistance = 1;
										break;
									}
								}
							}
							else {
								trying = false;
								break;
							}
						}
					}


					if (direction == 1) {
						s = "left";
						if (movingPiecePos.Pos.y - 1 > 0) {
							if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y - 1] == 0) {
								trying = true;
								//can move 1
								movementDistance = 1;

								if (movingPiecePos.Pos.y - 2 > 0) {
									if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y - 2] == 0) {
										//can move 2
										movementDistance = Random.Range(1, 3);

										if (movingPiecePos.Pos.y - 3 > 0) {
											if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y - 3] == 0) {
												//can move 3
												movementDistance = Random.Range(1, 4);
											}
											else {
												trying = true;
												movementDistance = Random.Range(1, 3);
												break;
											}
										}
									}
									else {
										trying = true;
										movementDistance = 1;
										break;
									}
								}
							}
							else {
								trying = false;
								break;
							}
						}
					}

					if (direction == 2) {
						s = "right";
						if (movingPiecePos.Pos.y + 1 < 7) {
							if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y + 1] == 0) {
								trying = true;
								//can move 1
								movementDistance = 1;

								if (movingPiecePos.Pos.y + 2 < 7) {
									if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y + 2] == 0) {
										//can move 2
										movementDistance = Random.Range(1, 3);

										if (movingPiecePos.Pos.y + 3 < 7) {
											if (pController.positions[movingPiecePos.Pos.x, movingPiecePos.Pos.y + 3] == 0) {
												//can move 3
												movementDistance = Random.Range(1, 4);
											}
											else {
												trying = true;
												movementDistance = Random.Range(1, 3);
												break;
											}
										}
									}
									else {
										trying = true;
										movementDistance = 1;
										break;
									}
								}
							}
							else {
								trying = false;
								break;
							}
						}
					}

					if (direction == 3) {
						s = "down";

						if (movingPiecePos.Pos.x + 1 < 7) {
							if (pController.positions[movingPiecePos.Pos.x + 1, movingPiecePos.Pos.y] == 0) {
								trying = true;
								//can move 1
								movementDistance = 1;

								if (movingPiecePos.Pos.x + 2 < 7) {
									if (pController.positions[movingPiecePos.Pos.x + 2, movingPiecePos.Pos.y] == 0) {
										//can move 2
										movementDistance = Random.Range(1, 3);

										if (movingPiecePos.Pos.x + 3 < 7) {
											if (pController.positions[movingPiecePos.Pos.x + 3, movingPiecePos.Pos.y] == 0) {
												//can move 3
												movementDistance = Random.Range(1, 4);
											}
											else {
												trying = true;
												movementDistance = Random.Range(1, 3);
												break;
											}
										}
									}
									else {
										trying = true;
										movementDistance = 1;
										break;
									}
								}
							}
							else {
								trying = false;
								break;
							}
						}
					}
				}

				if (movementDistance == 0) {
					if (tries >= 50) {
						Debug.Log("Enemy can't find a move");
						pController.BlockingGameOver(false);
					}
					else {
						goto Rand; //Needs to make sure that the piece is not trying to move zero squares, since this isn't a legal move
					}
				}
				else {
					yield return StartCoroutine(MovePiece(pieceToMove.gameObject, s, movementDistance));
				}
			}

			yield return null;
			pController.SwitchTurn();
		}
	
	}

	/// <summary>
	/// Moves the piece to its new space
	/// </summary>
	/// <param name="piece">Piece to move</param>
	/// <param name="dir">The direction to move in</param>
	/// <param name="dist">How far to move the piece</param>
	/// <returns></returns>
	IEnumerator MovePiece(GameObject piece, string dir, int dist) 
	{
		int x, y;
		//Debug test

		PetteiaPosition piecePos = piece.GetComponent<PetteiaPosition>();

		x = piecePos.Pos.x;
		y = piecePos.Pos.y;
		pController.positions[x, y] = 0;

		if (dir == "up") 
		{
			x -= dist;
		}
		else if (dir == "left") 
		{
			y -= dist;
		}
		else if (dir == "right") 
		{
			y += dist;
		}
		else if (dir == "down") 
		{
			x += dist;
		}

		piece.transform.position = pController.BoardSquares[x, y].transform.position;
		yield return new WaitForSeconds(0.5f);


		//pController.PlayMoveSound();
		pController.positions[x, y] = 1;
	}

	/// <summary>
	/// Turns the enemy turn highlight on or off
	/// </summary>
	/// <param name="toggle"></param>
	public void ToggleEnemyHighlight(bool toggle) 
	{
		foreach (PetteiaEnemyPiece p in pieces) 
		{
			if (p != null) 
			{
				p.highlight.SetActive(toggle);
			}
		}
	}
	
}
