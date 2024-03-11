//Paul Reichling
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Petteia.Scripts.Model;
using Shiny.Solver.Petteia;
using Shiny.Solver;
using System.Linq;

public class PetteiaEnemyAI : MonoBehaviour
{
	public List<PetteiaEnemyPiece> pieces;
	
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

	bool _aiMoveRequested;

	BoardStateGraph _graph;
	MoveInfo _queuedMove;
	bool _moveCompleted;

	int GetMaxDepthForDifficulty(AIDifficulty difficulty)
    {
		switch(difficulty)
        {
			case AIDifficulty.Easy: return 1;
			default:
			case AIDifficulty.Medium: return 3;
			case AIDifficulty.Hard: return 5;
			// 5 takes too long to compute moves, but might be viable if we store just the piece positions and not a whole board grid for every state
        }
    }

	async void Update()
	{
		var graph = _graph;
		if (_aiMoveRequested && graph != null)
		{
			_aiMoveRequested = false;

			var startTime = Time.realtimeSinceStartup;
			var solver = new MinimaxGameSolver(GetMaxDepthForDifficulty(GameManager.SelectedDifficulty));

			await new WaitForBackgroundThread();
			var solution = await solver.Solve(graph, graph.Start);
			await new WaitForUpdate();

			var (score, move) = solution;
			if (move == null)
			{
				Debug.Log("No good moves, AI player resigns.");
				_moveCompleted = true;
				_queuedMove = null;
			}
			else
			{
				Debug.Log("Move Computed in " + (Time.realtimeSinceStartup - startTime) + " s. Picked option with score " + score);
				_queuedMove = move.MoveInfo;
				_moveCompleted = true;
			}
		}
	}

	// game world's piece positions are all (row, col), but the board model is (x, y) or (col, row)
	Vector2Int BoardToView(Vector2Int pos) => new Vector2Int(pos.y, pos.x);
	Vector2Int ViewToBoard(Vector2Int pos) => new Vector2Int(pos.y, pos.x);

	int _enemyTurn;

	/// <summary>
	/// Chooses which piece to move and where to move it to
	/// </summary>
	/// <returns></returns>
	IEnumerator MakeMove() {

		var playerDefs = new PlayerDef[]
		{
				new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
				new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
		};

		var gameModel = new GameModel<RulesSet>(playerDefs, 8, 8);

		foreach (var enemyPiece in pieces)
		{
			var piecePos = enemyPiece.GetComponent<PetteiaPosition>().Pos;
			piecePos = ViewToBoard(piecePos);
			gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], piecePos);
		}
		foreach(var playerPiece in pController.playerPieces)
		{
			var piecePos = playerPiece.pieceStartPos;
			piecePos = ViewToBoard(piecePos);
			gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], piecePos);
		}

		var startNode = new BoardStateNode(null, gameModel.Board, gameModel.Players[0], gameModel.Players[1], null);
		_graph = new BoardStateGraph(gameModel.Rules, gameModel.Players, startNode, gameModel.Players[1]);
		_graph.Turn = _enemyTurn;
		_enemyTurn++;

		_aiMoveRequested = true;
		_moveCompleted = false;

		while (!_moveCompleted) yield return new WaitForEndOfFrame();

		yield return new WaitForSeconds(1f);

		//In case it took that extra second to fully register that the game is over
		if (!pController.GameOver) 
		{
			var moveInfo = _queuedMove;
			_queuedMove = null;

			PetteiaEnemyPiece pieceToMove = pieces.FirstOrDefault(piece => piece.GetComponent<PetteiaPosition>().Pos == BoardToView(moveInfo.from));
			if (pieceToMove != null)
			{
				yield return StartCoroutine(MovePiece(pieceToMove.gameObject, GetDirFromMoveInfo(moveInfo), GetDistFromMoveInfo(moveInfo)));
			}
			else
			{
				pController.BlockingGameOver(false);
			}

			yield return null;
			pController.SwitchTurn();
		}
	
	}

	int GetDistFromMoveInfo(MoveInfo moveInfo)
    {
		// we know they always move in a straight line, so this Distance function is safe to use
		return Mathf.RoundToInt(Vector2Int.Distance(moveInfo.from, moveInfo.to));
    }

	string GetDirFromMoveInfo(MoveInfo moveInfo)
    {
		var diff = moveInfo.to - moveInfo.from;
		Debug.Log("move diff of " + diff);
		if (diff.x == 0 && diff.y > 0) return "down";
		else if (diff.x == 0 && diff.y < 0) return "up";
		else if (diff.y == 0 && diff.x > 0) return "right";
		else return "left";
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
		Debug.Log("Move piece " + piece + " in " + dir + " for " + dist);

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
		pController.LastMove = new Vector2Int(x, y);
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
