namespace Shiny.Solver
{
    using Assets.Petteia.Scripts.Model;
    using Shiny.Threads;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace Petteia
    {
        public class MoveInfo
        {
            public UnityEngine.Vector2Int from;
            public UnityEngine.Vector2Int to;
            public int numCaptured;
        }

        public class BoardStateNode : GameNode
        {
            public BoardStateNode Parent { get; set; }
            public BoardStateModel BoardState;
            public PlayerModel LastMoveBy { get; set; }
            public MoveInfo MoveInfo { get; set; }

            public BoardStateNode(BoardStateNode parent, BoardStateModel boardState, PlayerModel lastMoveBy, PlayerModel activePlayer, MoveInfo moveInfo) : base()
            {
                Parent = parent;
                BoardState = boardState;
                LastMoveBy = lastMoveBy;
                ActivePlayer = activePlayer;
                MoveInfo = moveInfo;
            }
        }

        public class BoardStateGraph : IGameGraph<BoardStateNode>
        {
            public RulesSet Rules { get; set; }
            public PlayerModel[] Players { get; set; }
            public IGamePlayer SelfPlayer { get; set; }
            public PlayerModel OpponentPlayer { get; set; }
            public int Turn { get; set; }

            public BoardStateGraph(RulesSet rules, PlayerModel[] players, BoardStateNode startNode, PlayerModel selfPlayer)
            {
                Start = startNode;
                Rules = rules;
                Players = players;
                SelfPlayer = selfPlayer;
                OpponentPlayer = Players.FirstOrDefault(player => player != selfPlayer);
            }

            public BoardStateNode Start { get; set; }

            virtual protected int EvaluateMove(MoveInfo move, BoardStateModel newBoard, PlayerModel lastMoveBy)
            {
                // PERF: comment out things not being used to save time evaluating the leaves
                var piecesForOpponent = newBoard.GetNumPiecesForPlayer(OpponentPlayer);
                var piecesForPlayer = newBoard.GetNumPiecesForPlayer(SelfPlayer as PlayerModel);

                var pieceCountScore = piecesForPlayer - piecesForOpponent;
                //var favorMiddleScore = newBoard.GetPiecesForPlayer(SelfPlayer as PlayerModel).Count(space => space.Pos.y > 2 && space.Pos.y < 6);
                //var favorSidesScore = newBoard.GetPiecesForPlayer(SelfPlayer as PlayerModel).Count(space => space.Pos.x < 1 && space.Pos.x > 6);
                //var favorDestructionScore = lastMoveBy == SelfPlayer ? move.numCaptured : 0;
                var favorDefenseScore = lastMoveBy == OpponentPlayer ? -move.numCaptured : 0;
                //var favorAdvancingScore = (int)newBoard.GetPiecesForPlayer(SelfPlayer as PlayerModel).Average(space => space.y); // this advancement thing only works when AI is at the top

                return pieceCountScore + favorDefenseScore;//(pieceCountScore * 100) + ThreadsafeUtils.Random.Next(0, 50);

                // gets more aggressive in the late game, to avoid stalemates due to being too defensive
                // willing to give up a piece if it means taking another
                //if(piecesForOpponent > _startingPieces / 2)
                //{
                //    return pieceCountScore + favorDefenseScore;
                //}
                //else
                //{
                //    return pieceCountScore + favorDestructionScore;
                //}

                // favor sides and middle for the beginning of the game for some noticable fun, but then flip towards the end to playing really smart
                //if(turn < 8)
                //{
                //    return favorSidesScore + (int)favorAdvancingScore;
                //}
                //else
                //{
                //    return pieceCountScore * 2 + favorDestructionScore * 4;
                //}

                // dominant play is to advance every turn, but will defend and capture when convenient
                //return pieceCountScore + (int)favorAdvancingScore * 2;

                // tends to advance, but plays with mix of offense and defense otherwise
                //return pieceCountScore + (int)favorAdvancingScore;

                // advances no matter what, very easy, even on depth 3
                //return favorDestructionScore + (int)favorAdvancingScore;

                // this one plays super defensively on depth 2
                //return favorSidesScore + favorDefenseScore;

                // this one plays in the middle and is really good on depth 2
                // return pieceCountScore * 4 + favorMiddleScore * 1 + favorDestructionScore * 4;

                // this is the one that plays most like a human so far. best on depth 2 and up
                //return pieceCountScore * 2 + favorDestructionScore * 4;
            }

            public int GetScore(BoardStateNode node)
            {
                // if there's at least one valid move in the manual move list for this turn, we'll throw out all other possible moves and just use one of the matching manual moves
                // if all of the moves in the manual move list were invalid (eg, a piece is not in a place that would make one of these rules work, just use normal AI)
                if (HasValidManualTurn(node.BoardState, Turn))
                {
                    return IsMatchingMoveForManualTurn(node.BoardState, Turn, node.MoveInfo) ? 1 : 0;
                }

                return EvaluateMove(node.MoveInfo, node.BoardState, node.LastMoveBy);
            }

            public int GetCost(BoardStateNode from, BoardStateNode to)
            {
                var oldScore = GetScore(from);
                var newScore = GetScore(to);

                // uses a cost not a score, so reverse it. cost is negative if it improves things
                return oldScore - newScore;
            }

            const int _startingPieces = 8;

            // eek towards paths that reduce the number of pieces on the board to reduce search space
            public int GetHeuristicCost(BoardStateNode node) => 0;//(_startingPieces * 2) - node.BoardState.GetNumPieces();

            public BoardStateNode GetRandomEndNode() => throw new NotImplementedException();

            // TODO: Probably there should be a GetPreferredReachable that adds "suggested" options to give different characters more personality
            public IEnumerable<BoardStateNode> GetReachable(BoardStateNode node)
            {
                var currTurnPlayer = node.ActivePlayer as PlayerModel;
                foreach (var spaceWithPiece in node.BoardState.GetPiecesForPlayer(currTurnPlayer).Shuffle())
                {
                    var validMovesRow = node.BoardState.GetRowOfSpaces(spaceWithPiece.y).Where(move => Rules.IsValidMove(node.BoardState, spaceWithPiece, move));
                    var validMovesCol = node.BoardState.GetColumnOfSpaces(spaceWithPiece.x).Where(move => Rules.IsValidMove(node.BoardState, spaceWithPiece, move));

                    var validMoves = validMovesRow.Concat(validMovesCol).Shuffle();
                    foreach (var move in validMoves)
                    {
                        var numCaptures = 0;
                        var newBoard = node.BoardState.MovePiece(spaceWithPiece, move);
                        foreach (var pieceToRemove in Rules.GetSpacesToCaptureInMove(newBoard, currTurnPlayer, spaceWithPiece, move))
                        {
                            newBoard.RemovePieceInPlace(pieceToRemove);
                            numCaptures++;
                        }

                        //UnityEngine.Debug.Log("possible next state - from: " + spaceWithPiece.Pos + " to " + move.Pos);
                        yield return new BoardStateNode(node, newBoard, currTurnPlayer, node.LastMoveBy, new MoveInfo { from = spaceWithPiece, to = move, numCaptured = numCaptures });
                    }

                    //UnityEngine.Debug.Log("Space with piece for player " + currTurnPlayer.Def.Name + " : " + spaceWithPiece.Pos);
                }
            }

            protected class ManualTurn
            {
                public int Turn;
                public UnityEngine.Vector2Int From;
                public UnityEngine.Vector2Int To;

                public ManualTurn(int turn, int fromX, int fromY, int toX, int toY)
                {
                    Turn = turn;
                    From = new UnityEngine.Vector2Int(fromX, fromY);
                    To = new UnityEngine.Vector2Int(toX, toY);
                }
            }

            virtual protected ManualTurn[] ManualTurns => new ManualTurn[]
            {
                //new ManualTurn(0, 4, 0, 4, 4),
                //new ManualTurn(0, 5, 0, 5, 5),
                //new ManualTurn(1, 5, 5, 5, 6),
            };

            bool HasValidManualTurn(BoardStateModel board, int turn) => ManualTurns.Any(manual => manual.Turn == turn && Rules.IsValidMove(board, manual.From, manual.To));

            virtual protected bool IsMatchingMoveForManualTurn(BoardStateModel board, int turn, MoveInfo move)
            {
                var possibleMoves = ManualTurns.Where(manual => manual.Turn == turn);
                return possibleMoves.Any(manual => manual.From == move.from && manual.To == move.to && Rules.IsValidMove(board, manual.From, manual.To));
            }

            // we have a forced choice for turn 0
            // game ends when you're down to only one piece
            public bool IsEnd(BoardStateNode node) => (HasValidManualTurn(node.BoardState, Turn) && node.Parent != null) || 
                node.BoardState.GetNumPiecesForPlayer(Players.First()) == 1 || node.BoardState.GetNumPiecesForPlayer(Players.Last()) == 1;
        }
    }
}
