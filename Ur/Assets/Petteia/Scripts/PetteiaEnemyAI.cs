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
using System.Threading.Tasks;
using DG.Tweening;
using MiniGameFramework;

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
        switch (difficulty)
        {
            case AIDifficulty.VeryEasy: return 1;
            case AIDifficulty.Easy: return 2;
            default:
            case AIDifficulty.Medium: return 3;
            case AIDifficulty.Hard: return 4;
            case AIDifficulty.VeryHard: return 5;
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

            var co = StartCoroutine(BarkWhileThinking());

            await new WaitForBackgroundThread();
            var solution = await solver.Solve(graph, graph.Start);
            await new WaitForUpdate();

            StopCoroutine(co);

            var (score, move) = solution;
            if (move == null)
            {
                Debug.Log("No good moves, AI player resigns.");
                _moveCompleted = true;
                _queuedMove = null;
            }
            else
            {
                Debug.Log("Move Computed in " + (Time.realtimeSinceStartup - startTime) + " s. Picked option with score " + score + " weight was " + move.MoveInfo.precompmutedWeight + " unweighted score was " + (score / move.MoveInfo.precompmutedWeight));
                _queuedMove = move.MoveInfo;
                _moveCompleted = true;

                Bark(score, move);
            }
        }
    }

    IEnumerator BarkWhileThinking()
    {
        yield return new WaitForSeconds(4);
        if (_moveCompleted) yield break;
        pController.enemyDialog.ShowBark("Thinking...");
        yield return new WaitForSeconds(5);
        if (_moveCompleted) yield break;
        pController.enemyDialog.ShowBark("Thinking...");
        yield return new WaitForSeconds(6);
        if (_moveCompleted) yield break;
        pController.enemyDialog.ShowBark("Thinking...");
        yield return new WaitForSeconds(7);
        if (_moveCompleted) yield break;
        pController.enemyDialog.ShowBark("Thinking...");
    }

    void Bark(int score, BoardStateNode move)
    {
        var optionsHappyEnemy = new string[]
        {
                    "As the tender dove, torn to bits by warring hawks.",
                    "Circling your foe like hunting dogs, with snarling jaws and gleaming eyes, encircle the cringing rabbit.",
                    "A counter of one color perishes, flanked by twin enemies.",
                    "Thus Ovid said, ‘one counter perishes by a twin foe’!",
                    //"You have captured the enemy piece: so falls a citadel to a surrounding army!",
                    //"You have outflanked your enemy, like the twin sons of Zeus, flashing lightning from their helms, make the storm gods flee",
                    //"Wars are won by troops of friends, not arrogant heroes: your two pieces have overpowered the solitary game piece of your over confident foe!",
                    "As a warrior without companions is overtaken by enemy, right and left, and perishes like a man beaten into the dust",
                    "Your piece is lost, gobbled up by the surrounding enemy like gold looted from a fallen citadel",
                    "Like an army in the pass, a navy in the straits, an army reduced to a single file by well fortified mazes in a mighty city wall!",
                    "Like a fox in the den, cowering in fear, surrounded by hounds eager for the kill",
                    "Like an unskilled recruit, tricked into false turns by battle-skilled troops",
                    "Like a calf in a cattle pen, dogs at his heels, shame at his side, far from its mother!",
                    "Like a silvery catch in the fisherman’s nets, soon to answer the call of his hungry belly!",
                    "Like a silly lamb, surrounded by wolves, bleating for its mother!",
        };
        var optionsSadEnemy = new string[]
        {
                    "Circling your foe like hunting dogs, with snarling jaws and gleaming eyes, encircle the cringing rabbit.",
                    "Like an army in the pass, a navy in the straits, an army reduced to a single file by well fortified mazes in a mighty city wall!",
                    "Like a fox in the den, cowering in fear, surrounded by hounds eager for the kill",
                    "Like an unskilled recruit, tricked into false turns by battle-skilled troops",
                    "Like a calf in a cattle pen, dogs at his heels, shame at his side, far from its mother!",
                    "Like a silvery catch in the fisherman’s nets, soon to answer the call of his hungry belly!",
                    "Like a silly lamb, surrounded by wolves, bleating for its mother!",
        };

        if (move.MoveInfo.numCaptured > 0 && move.LastMoveBy == _graph.SelfPlayer)
        {
            // this isn't really right. in this case, it's when the enemy has to take a negative move because the only valid moves are all negative
            //if (score < 0 && score > -2)
            //{
            //	pController.enemyDialog.ShowText("All part of the plan -1 through 0");
            //}
            //else if (score < 0 && score > -4)
            //{
            //	pController.enemyDialog.ShowText("I'll make a glorious comback soon -3 through -2");
            //}
            //else if (score < 0 && score > -6)
            //{
            //	pController.enemyDialog.ShowText("Ok now i'm really nervous -4 through -5");
            //}
            //else if (score < 0 && score > -8)
            //{
            //	pController.enemyDialog.ShowText("Don't look at me -6 through -7");
            //}

            if (Random.Range(0, 10) == 0)
            {
                if (score > 0 && score < 2)
                {
                    pController.enemyDialog.ShowBark(optionsHappyEnemy.RandomElement());
                }
                else if (score > 0 && score < 4)
                {
                    pController.enemyDialog.ShowBark(optionsHappyEnemy.RandomElement());
                }
                else if (score > 0 && score < 6)
                {
                    pController.enemyDialog.ShowBark(optionsHappyEnemy.RandomElement());
                }
                else if (score > 0 && score < 8)
                {
                    pController.enemyDialog.ShowBark(optionsHappyEnemy.RandomElement());
                }
            }
        }

        // only show this text if there's a very good chance of the player capturing
        var capturesEnabled = _graph.GetReachable(move).Any(node => node.MoveInfo.numCaptured > 0 && node.MoveInfo.precompmutedWeight > 0.8f);
        if (capturesEnabled)
        {
            if (Random.Range(0, 10) == 0)
            {
                pController.enemyDialog.ShowBark(optionsSadEnemy.RandomElement());
            }
        }
    }

    // game world's piece positions are all (row, col), but the board model is (x, y) or (col, row)
    public static Vector2Int BoardToView(Vector2Int pos) => new Vector2Int(pos.y, pos.x);
    public static Vector2Int ViewToBoard(Vector2Int pos) => new Vector2Int(pos.y, pos.x);

    int _enemyTurn;

    /// <summary>
    /// Chooses which piece to move and where to move it to
    /// </summary>
    /// <returns></returns>
    IEnumerator MakeMove()
    {

        var playerDefs = new PlayerDef[]
        {
        new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
        new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
        };

        var gameModel = new GameModel<RulesSet>(playerDefs, 8, 8, RulesFactory.MakeRulesSet(pController.Lastroll));

        foreach (var enemyPiece in pieces)
        {
            var piecePos = enemyPiece.GetComponent<PetteiaPosition>().Pos;
            piecePos = ViewToBoard(piecePos);
            gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], piecePos, enemyPiece.PieceType);
        }
        foreach (var playerPiece in pController.playerPieces)
        {
            var piecePos = playerPiece.pieceStartPos;
            piecePos = ViewToBoard(piecePos);
            gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], piecePos, playerPiece.PieceType);
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

        var origPos = piecePos.Pos;
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

        piece.transform.DOMove(pController.BoardSquares[x, y].transform.position, 0.5f);
        yield return new WaitForSeconds(0.5f);

        //pController.PlayMoveSound();
        pController.positions[x, y] = 1;
        pController.LastMove = (origPos, new Vector2Int(x, y));
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
