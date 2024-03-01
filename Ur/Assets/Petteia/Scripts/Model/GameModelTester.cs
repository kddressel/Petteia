using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Petteia.Scripts.Model
{
    public class GameModelTester : MonoBehaviour
    {
        GameModel<RulesSet> GameModel;

        private void Awake()
        {
            var playerDefs = new PlayerDef[]
            {
                new PlayerDef { Name = "You", AgentType = typeof(HumanPlayerAgent) },
                new PlayerDef { Name = "Other", AgentType = typeof(HumanPlayerAgent) }
            };

            GameModel = new GameModel<RulesSet>(playerDefs, 8, 8);
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
                var gameModel = tester.GameModel;

                // everything underneath here requires play mode
                if (!Application.isPlaying)
                {
                    return;
                }

                var rules = gameModel.Rules;
                rules.StartFirstTurn(gameModel.Players);
            }

            public override void OnInspectorGUI()
            {
                var tester = target as GameModelTester;
                var gameModel = tester.GameModel;

                // everything underneath here requires play mode
                if (!Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Enter playmode to see more controls.");
                    return;
                }

                var rules = gameModel.Rules;

                EditorGUILayout.LabelField("Turn: " + gameModel.Rules.PlayerTurn?.Def.Name);

                EditorGUILayout.LabelField("----Place Pieces----");
                _player = EditorGUILayout.TextField("Player", _player);
                _toXPos = EditorGUILayout.TextField("XPos", _toXPos);
                _toYPos = EditorGUILayout.TextField("YPos", _toYPos);
                if(GUILayout.Button("Place new Piece"))
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
                if(GUILayout.Button("Check Valid Move"))
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
                    var captures = gameModel.Rules.GetSpacesToCaptureInMove(gameModel.Board, fromPos, toPos);
                    foreach(var capture in captures)
                    {
                        gameModel.Board = gameModel.Board.RemovePiece(gameModel.Board.GetSpaceAt(capture));
                    }

                    // usually you want to be able to move the piece again, so swap them each move
                    _fromXPos = _toXPos;
                    _fromYPos = _toYPos;
                }

                if(GUILayout.Button("End Turn"))
                {
                    gameModel.Rules.AdvanceToNextTurn(gameModel.Players);
                }

                // draw the board
                for (var y = 0; y < gameModel.Board.Size.y; y++)
                {
                    var row = "";
                    for(var x = 0; x < gameModel.Board.Size.x; x++)
                    {
                        var space = gameModel.Board.GetSpaceAt(new Vector2Int(x, y));
                        if (space.IsEmpty)
                        {
                            row += " _ ";
                        }
                        else if(space.Piece.Owner == gameModel.Players[0])
                        {
                            row += " X ";
                        }
                        else if(space.Piece.Owner == gameModel.Players[1])
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
