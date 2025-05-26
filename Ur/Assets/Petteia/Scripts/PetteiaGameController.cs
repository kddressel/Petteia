//Paul Reichling
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MiniGameFramework;
using Assets.Petteia.Scripts.Model;
using Shiny.Threads;
using System;
using System.Linq;
using Shiny.Solver.Petteia;
using Shiny.Solver;

public class PetteiaGameController : MonoBehaviour
{
    public static LevelDef LevelDef;

    [Header("Petteia Variables")]
    public Vector2Int rewardAmts;

    public string playerTag;
    public string enemyTag;

    public AudioClip playerCaptureSound;
    public AudioClip enemyCaptureSound;
    public AudioSource captureSounds;

    [Header("Game Pieces")]
    public List<PetteiaPlayerPiece> playerPieces;
    public PetteiaEnemyAI enemyAI;

    public int Lastroll { get; private set; }

    [Header("Board Positions")]
    public PetteiaBoardPosition[] squaresRow0 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow1 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow2 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow3 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow4 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow5 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow6 = new PetteiaBoardPosition[8];
    public PetteiaBoardPosition[] squaresRow7 = new PetteiaBoardPosition[8];
    [HideInInspector] public int[,] positions = new int[8, 8];

    [Header("Debug")]

    [ReadOnly][TextArea(0, 8)] public string debugPiecePositions;

    private bool playerTurn;
    private bool gameOver = false;
    private List<string> flavor;
    private List<string> winFlavor;
    private List<string> loseFlavor;
    private List<string> blockedFlavor;

    // TODO: copy pasted from UrGameController
    private bool gamePaused = false;
    private bool settingsInitialized = false;
    private bool pauseBuffer = false;
    private MenuButtons menuButtons;
    public GameObject modalBlocker;
    public Animator pauseMenuAnim;

    [SerializeField] Button _hintButton;
    [SerializeField] TextMeshProUGUI _rollUI;

    public TavernaMiniGameDialog playerDialog;
    public TavernaEnemyDialog enemyDialog;

    long StartTime;

    void Awake()
    {
        menuButtons = GetComponent<MenuButtons>();
    }

    void Start()
    {
        StartTime = DateTimeUtils.GetNowInUnixUTCSeconds();

        // TODO: Bring back flavor
        //if (Globals.Database != null) 
        //{
        //	flavor = Globals.Database.petteiaGameFlavor;
        //	winFlavor = Globals.Database.petteiaGameWin;
        //	loseFlavor = Globals.Database.petteiaGameLost;
        //	blockedFlavor = Globals.Database.petteiaGameBlocked;
        //}
        //else 
        //{
        //	flavor = new List<string> { "Petteia flavor 1", "Petteia flavor 2", "Petteia flavor 3" };
        //	winFlavor = new List<string> { "Petteia win flavor 1", "Petteia win flavor 2", "Petteia win flavor 3" };
        //	loseFlavor = new List<string> { "Petteia lose flavor 1", "Petteia lose flavor 2", "Petteia lose flavor 3" };
        //	blockedFlavor = new List<string> { "Petteia blocked flavor 1", "Petteia blocked flavor 2", "Petteia blocked flavor 3" };
        //}

        // TODO: Switch to kylie's TextDisplayBox
        //mgScreen.gameObject.SetActive(true);
        //string text = introText + "\n\n" + instructions + "\n\n" + flavor.RandomElement();
        //mgScreen.DisplayText("Petteia: An Introduction", "Taverna game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaStart);

        Debug.Log("Petteia Introduction!");
        EnableAllPlayerPieces();

        enemyAI = GetComponent<PetteiaEnemyAI>();
        playerTurn = true;

        if (RulesFactory.UseDiceRoll)
        {
            Roll();
        }
        else
        {
            _rollUI.transform.parent.gameObject.SetActive(false);
        }

        //Turns the various rows of board squares into one 2D array
        //Since public 2D arrays don't show in the inspector, we have to do it this way
        for (int i = 0; i < 8; i++)
        {
            BoardSquares[0, i] = squaresRow0[i];
            BoardSquares[1, i] = squaresRow1[i];
            BoardSquares[2, i] = squaresRow2[i];
            BoardSquares[3, i] = squaresRow3[i];
            BoardSquares[4, i] = squaresRow4[i];
            BoardSquares[5, i] = squaresRow5[i];
            BoardSquares[6, i] = squaresRow6[i];
            BoardSquares[7, i] = squaresRow7[i];
        }
        InitalStateSetup();

        //Player goes first, so their pieces are highlighted and the enemy's are not
        HighlightPlayerPieces(true);
        enemyAI.ToggleEnemyHighlight(false);

        _hintButton.gameObject.SetActive(true);
        _hintButton.onClick.AddListener(OnHintClick);
    }

    public void EnableAllPlayerPieces()
    {
        foreach (PetteiaPlayerPiece p in playerPieces)
        {
            p.StartGame();
        }
    }

    // TODO: copy pasted from UrGameController
    public void PauseMinigame()
    {
        if (!pauseBuffer)
        {
            pauseBuffer = true;
            StartCoroutine(RefreshPause());
            gamePaused = !gamePaused;
            if (!gamePaused)
            {
                GameManager.PlayMenuOpen();
                Time.timeScale = 1f;
            }
            else
            {
                GameManager.PlayMenuClosed();
            }
            pauseMenuAnim.SetTrigger("PauseMenuisActive");
            //modalBlocker.SetActive(gamePaused);
            if (!settingsInitialized)
            {
                settingsInitialized = true;
                menuButtons.InitializeSettings();
            }
        }
    }

    private IEnumerator RefreshPause()
    {
        yield return new WaitForSeconds(0.5f);
        pauseBuffer = false;
    }

    public void SetGameSpeed(float speed)
    {
        Time.timeScale = speed;
    }
    // TODO: end copy paste block

    public void UnpauseMinigame()
    {
        Time.timeScale = 1f;
        pauseMenuAnim.SetBool("PauseMenuisActive", false);
        modalBlocker.SetActive(false);
    }

    // TODO: Bring these back
    public void RestartMinigame()
    {
        Debug.Log("Restart!");
        GameManager.LoadGamePlay();
    }

    public void ExitGameplay()
    {
        GameManager.LoadMainMenu();
    }

    void Roll()
    {
        Lastroll = ThreadsafeUtils.Random.Next(1, RulesFactory.MaxRoll);
        _rollUI.text = Lastroll.ToString();
        _rollUI.color = PlayerTurn ? Color.black : Color.blue;
        Debug.Log("rolled a " + Lastroll);
    }

    /// <summary>
    /// Checks for captures and game over, and switches the control between the player/AI
    /// </summary>
    public void SwitchTurn()
    {
        UpdateDebugText();

        playerDialog.EnableHighlight(playerTurn);
        enemyDialog.EnableHighlight(!playerTurn);


        if (playerTurn)
        {
            //Switching from player turn to enemy turn
            CheckCapture();
            CheckPlayerBlocked();
            playerTurn = false;

            if (RulesFactory.UseDiceRoll)
            {
                Roll();
            }

            enemyAI.CheckPieces();
            StartCoroutine(CheckGameOver());

            HighlightPlayerPieces(false);
            enemyAI.ToggleEnemyHighlight(true);

            _hintButton.onClick.RemoveListener(OnHintClick);
            _hintButton.gameObject.SetActive(false);

            enemyAI.StartEnemyTurn();
        }
        else
        {
            //Switching from enemy turn to player turn
            CheckCapture();
            CheckPlayerBlocked();
            playerTurn = true;
            _playerTurn++;

            if (RulesFactory.UseDiceRoll)
            {
                Roll();
            }

            enemyAI.CheckPieces();
            StartCoroutine(CheckGameOver());

            HighlightPlayerPieces(true);
            enemyAI.ToggleEnemyHighlight(false);

            _hintButton.gameObject.SetActive(true);
            _hintButton.onClick.AddListener(OnHintClick);
        }
    }

    void OnHintClick()
    {
        Debug.Log("hint clicked");
        StartCoroutine(GenerateMoveHint(applyMove: false));
    }

    /// <summary>
    /// Toggles the highlight on all player pieces
    /// </summary>
    /// <param name="toggle"></param>
    public void HighlightPlayerPieces(bool toggle)
    {
        foreach (PetteiaPlayerPiece p in playerPieces)
        {
            p.ToggleHighlight(toggle);
        }
    }

    private void InitalStateSetup()
    {
        //1 - enemy
        //2 - player

        //We could roll these into one since enemy and player should always have the same number of pieces
        //But while testing, I made one or the other start with fewer so it would be easier to finish
        for (int i = 0; i < enemyAI.pieces.Count; i++)
        {
            positions[enemyAI.pieces[i].pieceStartPos.x, enemyAI.pieces[i].pieceStartPos.y] = 1;
            BoardSquares[enemyAI.pieces[i].pieceStartPos.x, enemyAI.pieces[i].pieceStartPos.y].occupied = true;
        }

        for (int i = 0; i < playerPieces.Count; i++)
        {
            positions[playerPieces[i].pieceStartPos.x, playerPieces[i].pieceStartPos.y] = 2;
            BoardSquares[playerPieces[i].pieceStartPos.x, playerPieces[i].pieceStartPos.y].occupied = true;
        }
    }

    /// <summary>
    /// Checks if any pieces have been captured
    /// </summary>
    public void CheckCapture()
    {
        var playerDefs = new PlayerDef[]
        {
                new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
                new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
        };

        var gameModel = new GameModel<RulesSet>(playerDefs, 8, 8, RulesFactory.MakeRulesSet(Lastroll));
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var boardPos = BoardSquares[x, y];
                if (positions[x, y] == 1)
                {
                    gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], new Vector2Int(x, y), boardPos.CurrentPiece?.PieceType);
                }
                else if (positions[x, y] == 2)
                {
                    gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], new Vector2Int(x, y), boardPos.CurrentPiece?.PieceType);
                }
            }
        }

        var captures = gameModel.Rules.GetSpacesToCaptureInMove(gameModel.Board, !PlayerTurn ? gameModel.Players[0] : gameModel.Players[1], LastMove.Item1, LastMove.Item2);
        foreach (var capture in captures)
        {
            Debug.Log("Piece captured");
            StartCoroutine(CapturePiece(capture.x, capture.y));
        }

        /*
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                //Checks for vertical captures - not possible on the top or bottom rows
                if (x != 0 && x != 7)
                {
                    if (positions[x, y] == 1
                        && positions[x + 1, y] == 2
                        && positions[x - 1, y] == 2
                        && PlayerTurn && (LastMove == new Vector2Int(x + 1, y) || LastMove == new Vector2Int(x - 1, y)))            // KD: Added because the rules we referenced say that going between two pieces on your own turn doesn't capture
                    {
                        Debug.Log("Enemy captured by player vertically");
                        StartCoroutine(CapturePiece(x, y));
                    }

                    if (positions[x, y] == 2
                        && positions[x + 1, y] == 1
                        && positions[x - 1, y] == 1
                        && !PlayerTurn && (LastMove == new Vector2Int(x + 1, y) || LastMove == new Vector2Int(x - 1, y)))         // KD: Added because the rules we referenced say that going between two pieces on your own turn doesn't capture 
                    {
                        Debug.Log("Player captured by enemy vertically");
                        StartCoroutine(CapturePiece(x, y));
                    }

                }
                //Checks for horizontal captures - not possible on the left- or rightmost columns
                if (y != 0 && y != 7)
                {
                    if (positions[x, y] == 1
                        && positions[x, y + 1] == 2
                        && positions[x, y - 1] == 2
                        && PlayerTurn && (LastMove == new Vector2Int(x, y - 1) || LastMove == new Vector2Int(x, y + 1)))         // KD: Added because the rules we referenced say that going between two pieces on your own turn doesn't capture
                    {
                        Debug.Log("Enemy captured by player horizontally");
                        StartCoroutine(CapturePiece(x, y));
                    }

                    if (positions[x, y] == 2
                        && positions[x, y + 1] == 1
                        && positions[x, y - 1] == 1
                        && !PlayerTurn && (LastMove == new Vector2Int(x, y - 1) || LastMove == new Vector2Int(x, y + 1)))         // KD: Added because the rules we referenced say that going between two pieces on your own turn doesn't capture 
                    {
                        Debug.Log("Player captured by enemy horizontally");
                        StartCoroutine(CapturePiece(x, y));
                    }
                }
            }
        }
        */
    }

    /// <summary>
    /// Checks if either the player or opponent is down to 1 piece left
    /// </summary>
    public IEnumerator CheckGameOver()
    {
        //Debug.Log($"Players: {playerPieces.Count} | Enemies: {enemyAI.pieces.Count}");
        yield return null;
        enemyAI.CheckPieces();
        yield return null;

        if (RulesFactory.UseKing)
        {
            //Player win
            if (enemyAI.pieces.Count(piece => piece.PieceType == "King") == 0 || Input.GetKey(KeyCode.W))
            {
                WinGame();
            }

            //Player loss
            if (playerPieces.Count(piece => piece.PieceType == "King") == 0)
            {
                LoseGame();
            }
        }
        else
        {
            //Player win
            if (enemyAI.pieces.Count <= 1 || Input.GetKey(KeyCode.W))
            {
                WinGame();
            }

            //Player loss
            if (playerPieces.Count <= 1)
            {
                LoseGame();
            }
        }
    }

    private void WinGame()
    {
        gameOver = true;
        //mgScreen.gameObject.SetActive(true);

        //Minimum pieces left to win is 2, maximum is all 8
        //So we need to map [rewardAmt.x rewardAmt.y] to [2, 8]
        float oldRange = 6f;
        float newRange = rewardAmts.y - rewardAmts.x;
        int reward = Mathf.CeilToInt(((playerPieces.Count - 2) * (newRange * 1.0f) / oldRange) + rewardAmts.x);

        //string text = winText + "\n\n" + $"For your victory, you win {reward} food and water!" + "\n\n" + winFlavor.RandomElement();

        //if (Globals.Game.Session != null) 
        //{
        //	Globals.Game.Session.playerShipVariables.ship.AddToFoodAndWater(reward);
        //}

        Debug.Log("Victory!");

        GameManager.Instance.PlayerRecord.Rounds.Add(new RoundRecord
        {
            Cleared = true,
            Draw = false,
            Duration = DateTimeUtils.GetNowInUnixUTCSeconds() - StartTime,
            EndTime = DateTimeUtils.GetNowInUnixUTCSeconds(),
            LevelId = LevelDef.Id,
            NumPiecesLeft = playerPieces.Count,
        });

        GameManager.Instance.PlayerRecord.CurrentLevel = LevelDef.Id;
        GameManager.Instance.PlayerRecord.Modified = DateTimeUtils.GetNowInUnixUTCSeconds();
        GameManager.Instance?.Save();
        CameraSlider.StartPosition = CameraSlider.Position.Game;
        GameManager.LoadMainMenu(MenuScreen.ScreenType.LevelSelect);

        //mgScreen.DisplayText("Petteia: Victory!", "Taverna Game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);

    }

    private void LoseGame()
    {
        gameOver = true;

        Debug.Log("Lose Game!");

        GameManager.Instance.PlayerRecord.Rounds.Add(new RoundRecord
        {
            Cleared = false,
            Draw = false,
            Duration = DateTimeUtils.GetNowInUnixUTCSeconds() - StartTime,
            EndTime = DateTimeUtils.GetNowInUnixUTCSeconds(),
            LevelId = LevelDef.Id,
            NumPiecesLeft = playerPieces.Count,
        });
        GameManager.Instance?.Save();
        CameraSlider.StartPosition = CameraSlider.Position.Game;
        GameManager.LoadMainMenu(MenuScreen.ScreenType.LevelSelect);

        //mgScreen.gameObject.SetActive(true);
        //string text = loseText + "\n\n" + "Although you have lost this round, you can always find a willing opponent to try again!" + "\n\n" + loseFlavor.RandomElement();
        //mgScreen.DisplayText("Petteia: Defeat!", "Taverna Game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);

    }

    public void BlockingGameOver(bool playerBlocked)
    {
        if (playerBlocked)
        {
            //player can't move, has therefore lost
            gameOver = true;
            Debug.Log("Player is blocked in");
            //mgScreen.gameObject.SetActive(true);
            //string text = blockedFlavor.RandomElement() + "\n\n" + "Although you have lost this round, you can always find a willing opponent to try again!" + "\n\n" + loseFlavor.RandomElement();
            //mgScreen.DisplayText("Petteia: Defeat!", "Taverna Game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);

        }
        else
        {
            //enemy can't move, player has therefore won
            //Won't have special text, so can just trick the game into thinking the player won normally
            Debug.Log("Enemy is blocked in");
            enemyAI.pieces.Clear();
            StartCoroutine(CheckGameOver());
        }
    }

    private void CheckPlayerBlocked()
    {
        for (int i = 0; i < playerPieces.Count; i++)
        {
            List<PetteiaBoardPosition> validMoves = playerPieces[i].PopulateValidMovesList(playerPieces[i].pieceStartPos);

            //If any one player piece can still move, you're not blocked
            //We check if it's more than 1 because the square the piece is currently on is always counted
            if (validMoves.Count > 1)
            {
                return;
            }
        }

        BlockingGameOver(true);
    }

    /// <summary>
    /// Captures the piece located at [i, j]
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private IEnumerator CapturePiece(int i, int j)
    {
        BoardSquares[i, j].DestroyPiece();
        int enemyDone = 0;
        enemyDone = enemyAI.CheckPieces();
        int tries = 0;

        //if [i, j] == 1, it's the player capturing the enemy
        //otherwise, if [i, j] == 2, it's the enemy capturing the player
        if (positions[i, j] == 1)
        {
            captureSounds.clip = playerCaptureSound;
            captureSounds.Play();
        }
        else if (positions[i, j] == 2)
        {
            captureSounds.clip = enemyCaptureSound;
            captureSounds.Play();
        }

        //This is some really weird code that might not be necessary
        //Originally, I had this as a coroutine with yield return enemyAI.CheckPieces()
        //And that worked great except when the player intentionally moved into a capture
        //I have absolutely no idea what the problem was, but changing it to not be a coroutine fixed it?
        while (enemyDone != 1 && tries < 1000)
        {
            Debug.Log("wait...");
        }
        if (tries >= 200)
        {
            Debug.Log("Waited too long for the enemy to check its pieces");
        }

        yield return null;
        yield return null;

        // TODO: Restore barks
        //If this is the last move of the game and is going to end it, we don't want a bark to pop up underneath the ending UI
        //if (Random.Range(0f, 1f) < barkChance && (enemyAI.pieces.Count > 1 && playerPieces.Count > 1)) 
        //{
        //	if (Random.Range(0f, 1f) > 0.5f) 
        //	{
        //		//player captures enemy - player brags
        //		if (positions[i, j] == 1) {
        //			playerBarks.DisplayBragging();
        //		}
        //		//enemy captures player - player insults
        //		else if (positions[i, j] == 2) {
        //			playerBarks.DisplayInsult();
        //		}
        //	}
        //	else {
        //		//player captures enemy - enemy insults
        //		if (positions[i, j] == 1) {
        //			enemyBarks.DisplayInsult();
        //		}
        //		//enemy captures player - enemy brags
        //		else if (positions[i, j] == 2) {
        //			enemyBarks.DisplayBragging();
        //		}
        //	}
        //
        //}

        positions[i, j] = 0;
        UpdateDebugText();
    }

    /// <summary>
    /// Prints the board pieces to debugPiecePositions
    /// Player pieces are O, enemy pieces are X, and empty spaces are -
    /// </summary>
    private void UpdateDebugText()
    {
        string s = "";
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                switch (positions[i, j])
                {
                    case 0:
                        s += "-";
                        break;
                    case 1:
                        s += "X";
                        break;
                    case 2:
                        s += "O";
                        break;
                }
                s += " ";
            }
            s += "\n";
        }
        debugPiecePositions = s;
    }

    /// <summary>
    /// Moves a piece tagged for player/enemy from oldPos to newPos
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    /// <param name="tag"></param>
    public void MovePiece(Vector2Int oldPos, Vector2Int newPos, string tag)
    {
        positions[oldPos.x, oldPos.y] = 0;
        positions[newPos.x, newPos.y] = tag == playerTag ? 2 : 1;
        LastMove = (oldPos, newPos);
    }

    public (Vector2Int, Vector2Int) LastMove { get; set; }


    private Vector2Int PosToArray(float y, float x)
    {
        return new Vector2Int(Mathf.RoundToInt((y + 3.25f) / -6.25f), Mathf.RoundToInt((x - 3) / 6.25f));
        //converts the real world cordinates of the pieces to the value of the array that stores where the pieces are
    }

    public PetteiaBoardPosition[,] BoardSquares { get; } = new PetteiaBoardPosition[8, 8];

    public bool GameOver
    {
        get { return gameOver; }
    }

    public bool PlayerTurn
    {
        get { return playerTurn; }
    }

    bool _aiMoveRequested;

    BoardStateGraph _graph;
    MoveInfo _queuedMove;
    bool _moveCompleted;
    int _playerTurn;

    IEnumerator GenerateMoveHint(bool applyMove)
    {
        var playerDefs = new PlayerDef[]
        {
        new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
        new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
        };

        Debug.Log("Generating move hint with roll " + Lastroll);
        var gameModel = new GameModel<RulesSet>(playerDefs, 8, 8, RulesFactory.MakeRulesSet(Lastroll));

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (positions[x, y] == 1)
                {
                    gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], new Vector2Int(x, y), BoardSquares[x, y]?.CurrentPiece?.PieceType);
                }
                else if (positions[x, y] == 2)
                {
                    gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], new Vector2Int(x, y), BoardSquares[x, y]?.CurrentPiece?.PieceType);
                }
            }
        }
        var startNode = new BoardStateNode(null, gameModel.Board, gameModel.Players[1], gameModel.Players[0], null);
        _graph = new BoardStateGraph(gameModel.Rules, gameModel.Players, startNode, gameModel.Players[0]);
        _graph.Turn = _playerTurn;

        Debug.Log("Generating move hint");
        _aiMoveRequested = true;
        while (!_moveCompleted) yield return new WaitForEndOfFrame();

        var moveInfo = _queuedMove;

        Debug.Log("best move would be to move from " + moveInfo.from + " to " + moveInfo.to);
        var fromView = moveInfo.from;
        var toView = moveInfo.to;
        BoardSquares[fromView.x, fromView.y].BoldHighlightSpace(true);
        BoardSquares[toView.x, toView.y].BoldHighlightSpace(true);
        yield return new WaitForSeconds(0.75f);
        BoardSquares[fromView.x, fromView.y].BoldHighlightSpace(false);
        BoardSquares[toView.x, toView.y].BoldHighlightSpace(false);

        if (applyMove)
        {
            var piece = playerPieces.FirstOrDefault(piece => piece.pieceStartPos == fromView);
            if (piece != null)
            {
                var boardPos = BoardSquares[fromView.x, fromView.y];
                piece.transform.position = boardPos.transform.position;
                MovePiece(fromView, toView, playerTag);
                piece.pieceStartPos = toView;
                SwitchTurn();
            }
        }

        _queuedMove = null;
        _moveCompleted = false;
    }

    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(GenerateMoveHint(applyMove: true));
        }

        var graph = _graph;
        if (_aiMoveRequested && graph != null)
        {
            _aiMoveRequested = false;

            var startTime = Time.realtimeSinceStartup;
            var solver = new MinimaxGameSolver(2);

            await new WaitForBackgroundThread();
            var solution = await solver.Solve(graph, graph.Start);
            await new WaitForUpdate();

            var (score, move) = solution;
            if (move == null)
            {
                Debug.Log("No good moves, Cannot give a hint.");
                _moveCompleted = true;
                _queuedMove = null;
            }
            else
            {
                Debug.Log("Move Computed in " + (Time.realtimeSinceStartup - startTime) + " s. Picked option with score " + score + " weight was " + move.MoveInfo.precompmutedWeight + " unweighted score was " + (score / move.MoveInfo.precompmutedWeight));
                _queuedMove = move.MoveInfo;
                _moveCompleted = true;
            }
        }
    }
}
