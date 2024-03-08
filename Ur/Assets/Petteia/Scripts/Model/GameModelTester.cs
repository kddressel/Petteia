using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Shiny.Solver;
using Shiny.Solver.Petteia;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Petteia.Scripts.Model
{
    public class GameModelTester : MonoBehaviour
    {
        GameModel<RulesSet> gameModel;
        PlayerModel _currentTurn;

        BoardStateGraph _player1AI;
        BoardStateGraph _player2AI;
        MinimaxGameSolver _solver;

        private void Awake()
        {
            var playerDefs = new PlayerDef[]
            {
                new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
                new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
            };

            gameModel = new GameModel<RulesSet>(playerDefs, 8, 8);

            for (var i = 0; i < 8; i++)
            {
                gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], gameModel.Board.GetSpaceAt(new Vector2Int(i, 0)));
                gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], gameModel.Board.GetSpaceAt(new Vector2Int(i, 7)));
            }
        }

        void Start()
        {
            // TODO: Should probably use null for lastMoveBy
            _player1AI = new BoardStateGraph(gameModel.Rules, gameModel.Players, new BoardStateNode(null, gameModel.Board.Clone(), gameModel.Players[1], gameModel.Players[0], null), gameModel.Players[0]);
            _player2AI = new BoardStateGraph(gameModel.Rules, gameModel.Players, new BoardStateNode(null, gameModel.Board.Clone(), gameModel.Players[1], gameModel.Players[0], null), gameModel.Players[1]);
            _solver = new MinimaxGameSolver(3);
        }

        bool _aiMoveRequestedP1;
        bool _aiMoveRequestedP2;
        bool _playWholeAiGame;

        async void Update()
        {
            BoardStateGraph graph = null;
            if (_aiMoveRequestedP1)
            {
                graph = _player1AI;
                _player1AI.Start = new BoardStateNode(null, gameModel.Board, gameModel.Players[1], gameModel.Players[0], null);
            }
            else if (_aiMoveRequestedP2)
            {
                graph = _player2AI;
                _player2AI.Start = new BoardStateNode(null, gameModel.Board, gameModel.Players[0], gameModel.Players[1], null);
            }

            if (graph != null)
            {
                var nextP1Request = _playWholeAiGame ? _aiMoveRequestedP2 : false;
                var nextP2Request = _playWholeAiGame ? _aiMoveRequestedP1 : false;

                _aiMoveRequestedP1 = false;
                _aiMoveRequestedP2 = false;

                var startTime = Time.realtimeSinceStartup;

                await new WaitForBackgroundThread();
                var solution = await _solver.Solve(graph, graph.Start);
                await new WaitForUpdate();

                var (score, move) = solution;
                if(move == null)
                {
                    Debug.Log("No good moves, AI player resigns.");
                    _playWholeAiGame = false;
                    _aiMoveRequestedP1 = false;
                    _aiMoveRequestedP2 = false;
                }
                else
                {
                    Debug.Log("Move Computed in " + (Time.realtimeSinceStartup - startTime) + " s. Picked option with score " + score);
                    gameModel.Board = move.BoardState.Clone();
                }

                if (_playWholeAiGame)
                {
                    await Task.Delay(1000);
                }

                _aiMoveRequestedP1 = nextP1Request;
                _aiMoveRequestedP2 = nextP2Request;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(GameModelTester))]
        public class LookAtPointEditor : Editor
        {
            string _player;
            string _toXPos;
            string _toYPos;
            string _fromXPos;
            string _fromYPos;

            private void OnEnable()
            {
                var tester = target as GameModelTester;
                var gameModel = tester.gameModel;

                // everything underneath here requires play mode
                if (!Application.isPlaying)
                {
                    return;
                }

                tester._currentTurn = gameModel.Players.FirstOrDefault();
            }

            public override void OnInspectorGUI()
            {
                var tester = target as GameModelTester;
                var gameModel = tester.gameModel;

                // everything underneath here requires play mode
                if (!Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Enter playmode to see more controls.");
                    return;
                }

                var rules = gameModel.Rules;

                EditorGUILayout.LabelField("Turn: " + tester._currentTurn?.Def.Name);

                EditorGUILayout.LabelField("----Place Pieces----");
                _player = EditorGUILayout.TextField("Player", _player);
                _toXPos = EditorGUILayout.TextField("XPos", _toXPos);
                _toYPos = EditorGUILayout.TextField("YPos", _toYPos);
                if (GUILayout.Button("Place new Piece"))
                {
                    var player = gameModel.Players[int.Parse(_player)];
                    var pos = new Vector2Int(int.Parse(_toXPos), int.Parse(_toYPos));

                    gameModel.Board = gameModel.Board.PlaceNewPiece(player, gameModel.Board.GetSpaceAt(pos));
                }

                EditorGUILayout.LabelField("----Move Pieces----");
                _fromXPos = EditorGUILayout.TextField("From XPos", _fromXPos);
                _fromYPos = EditorGUILayout.TextField("From YPos", _fromYPos);
                _toXPos = EditorGUILayout.TextField("To XPos", _toXPos);
                _toYPos = EditorGUILayout.TextField("To YPos", _toYPos);
                if (GUILayout.Button("Check Valid Move"))
                {
                    var fromPos = new Vector2Int(int.Parse(_fromXPos), int.Parse(_fromYPos));
                    var toPos = new Vector2Int(int.Parse(_toXPos), int.Parse(_toYPos));

                    Debug.Log("IS VALID MOVE: " + rules.IsValidMove(gameModel.Board, fromPos, toPos));
                }
                if (GUILayout.Button("Move Piece"))
                {
                    var fromPos = new Vector2Int(int.Parse(_fromXPos), int.Parse(_fromYPos));
                    var toPos = new Vector2Int(int.Parse(_toXPos), int.Parse(_toYPos));

                    gameModel.Board = gameModel.Board.MovePiece(gameModel.Board.GetSpaceAt(fromPos), gameModel.Board.GetSpaceAt(toPos));

                    // do captures
                    var captures = gameModel.Rules.GetSpacesToCaptureInMove(gameModel.Board, tester._currentTurn, fromPos, toPos);
                    foreach (var capture in captures)
                    {
                        gameModel.Board = gameModel.Board.RemovePiece(gameModel.Board.GetSpaceAt(capture));
                    }

                    // usually you want to be able to move the piece again, so swap them each move
                    _fromXPos = _toXPos;
                    _fromYPos = _toYPos;
                }

                if (GUILayout.Button("AI Move"))
                {
                    if (tester._currentTurn == tester._player1AI.SelfPlayer)
                    {
                        tester._aiMoveRequestedP1 = true;
                    }
                    else
                    {
                        tester._aiMoveRequestedP2 = true;
                    }
                }

                if (GUILayout.Button("Play out AI Game"))
                {
                    tester._playWholeAiGame = true;

                    if (tester._currentTurn == tester._player1AI.SelfPlayer)
                    {
                        tester._aiMoveRequestedP1 = true;
                    }
                    else
                    {
                        tester._aiMoveRequestedP2 = true;
                    }
                }

                if (GUILayout.Button("End Turn"))
                {
                    tester._currentTurn = gameModel.Rules.AdvanceToNextTurn(gameModel.Players, tester._currentTurn);
                }

                // draw the board
                for (var y = 0; y < gameModel.Board.Size.y; y++)
                {
                    var row = "";
                    for (var x = 0; x < gameModel.Board.Size.x; x++)
                    {
                        var space = gameModel.Board.GetSpaceAt(new Vector2Int(x, y));
                        if (space.IsEmpty)
                        {
                            row += " _ ";
                        }
                        else if (space.Piece.Owner == gameModel.Players[0])
                        {
                            row += " X ";
                        }
                        else if (space.Piece.Owner == gameModel.Players[1])
                        {
                            row += " O ";
                        }
                        else
                        {
                            row += "err";
                        }
                    }
                    EditorGUILayout.LabelField(row);
                }
            }
        }
#endif
    }
}
