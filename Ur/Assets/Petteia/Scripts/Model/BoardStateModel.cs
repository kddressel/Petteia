using Shiny.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Petteia.Scripts.Model
{
    public class GameModel<T> where T : RulesSet
    {
        public RulesSet Rules;
        public PlayerModel[] Players;
        public BoardStateModel Board;

        public GameModel(PlayerDef[] players, int boardWidth, int boardHeight)
        {
            var playerModels = new List<PlayerModel>();
            foreach (var player in players)
            {
                var playerModel = new PlayerModel
                {
                    Def = player,
                    Agent = (IPlayerAgent)Activator.CreateInstance(player.AgentType)
                };
                playerModels.Add(playerModel);
            }
            Players = playerModels.ToArray();

            Board = new BoardStateModel(boardWidth, boardHeight);
            Rules = new RulesSet();
        }
    }

    #region Parameters for setting up a new game
    public class PlayerDef
    {
        public string Name { get; set; }
        public Type AgentType { get; set; }
    }
    #endregion

    #region Agents
    public interface IPlayerAgent
    {

    }

    public class AIPlayerAgent : IPlayerAgent
    {

    }

    public class HumanPlayerAgent : IPlayerAgent
    {

    }
    #endregion

    #region Needs categorization

    public class PlayerModel : IGamePlayer
    {
        public PlayerDef Def { get; internal set; }
        public IPlayerAgent Agent { get; internal set; }
    }

    /// <summary>
    /// Most basic ruleset, to be extended and overridden with various custom rulesets
    /// </summary>
    public class RulesSet
    {
        virtual public PlayerModel AdvanceToNextTurn(PlayerModel[] players, PlayerModel currentTurn)
        {
            if (currentTurn == players[0]) return players[1];
            else if (currentTurn == players[1]) return players[0];
            else return null;
        }

        virtual public IEnumerable<Vector2Int> GetSpacesToCaptureInMove(BoardStateModel board, PlayerModel currentTurn, Vector2Int from, Vector2Int to)
        {
            var isValid = IsValidMove(board, from, to);
            var adjacentEnemySpaces = board.GetAdjacentSpaces(to).Where(space => !board.IsSpaceEmpty(space) && board.GetPieceAt(space).Owner != currentTurn);

            // it is supposed to multi-capture if you surround pieces in multiple directions
            // moving your piece between two enemy pieces isn't a capture. enemy has to move to capture, so we need to check who's turn it is
            // TODO: Is it supposed to capture every piece in a line if there are multiple?
            foreach (var enemySpace in adjacentEnemySpaces)
            {
                var capturingSpace = board.GetNextInSameDirection(to, enemySpace);
                if (capturingSpace != null && board.GetPieceAt(capturingSpace)?.Owner == currentTurn)
                {
                    yield return enemySpace;
                }
            }

            // Kennesaw students added a special rule for capturing in the 4 corners of the board, since those would be safe spaces otherwise
            var cornerSpaces = board.GetCornerSpaces().Where(space => !board.IsSpaceEmpty(space) && board.GetPieceAt(space).Owner != currentTurn);
            foreach (var corner in cornerSpaces)
            {
                var capturingSpaces = board.GetAdjacentSpaces(corner);
                if(capturingSpaces.All(pos => board.GetPieceAt(pos)?.Owner == currentTurn))
                {
                    yield return corner;
                }
            }
        }

        virtual public bool IsValidMove(BoardStateModel board, Vector2Int from, Vector2Int to)
        {
            var isStraightLine = board.IsStraightLine(from, to);
            var isNotOccupied = board.IsSpaceEmpty(to);
            var isNotBlocked = board.IsWholeLineEmpty(from, to);

            return isStraightLine && isNotOccupied && isNotBlocked;
        }
    }

    #endregion

    #region Board
    public class PieceModel
    {
        public PlayerModel Owner { get; private set; }

        public PieceModel(PlayerModel owner)
        {
            Owner = owner;
        }
    }

    class SpaceModel
    {
        public Vector2Int Pos { get; private set; }
        public PieceModel Piece { get; internal set; }
        public bool IsEmpty => Piece == null;

        public SpaceModel Clone()
        {
            return new SpaceModel(Pos) { Piece = Piece };
        }

        public SpaceModel(Vector2Int pos)
        {
            Pos = pos;
        }
    }

    public class BoardStateModel
    {
        int _width;
        int _height;

        // PERF: cache counts for performance
        int _numPieces;
        Dictionary<PlayerModel, int> _numPiecesPerPlayer;

        Dictionary<Vector2Int, SpaceModel> _spaces;

        public Vector2Int Size => new Vector2Int(_width, _height);

        public BoardStateModel(int width, int height)
        {
            _width = width;
            _height = height;
            _spaces = new Dictionary<Vector2Int, SpaceModel>();

            // PERF: cache counts for performance
            _numPieces = 0;
            _numPiecesPerPlayer = new Dictionary<PlayerModel, int>();
        }

        public BoardStateModel Clone()
        {
            var clone = new BoardStateModel(Size.x, Size.y);
            foreach(var kvp in _spaces)
            {
                clone._spaces.Add(kvp.Key, kvp.Value.Clone());
            }
            foreach(var kvp in _numPiecesPerPlayer)
            {
                clone._numPiecesPerPlayer.Add(kvp.Key, kvp.Value);
            }

            clone._width = _width;
            clone._height = _height;
            clone._numPieces = _numPieces;

            return clone;
        }

        static void IncrementPieceCounts(BoardStateModel board, PlayerModel forPlayer, int incBy)
        {
            board._numPieces += incBy;
            if (!board._numPiecesPerPlayer.ContainsKey(forPlayer))
            {
                board._numPiecesPerPlayer.Add(forPlayer, 0);
            }
            board._numPiecesPerPlayer[forPlayer] += incBy;
        }

        public BoardStateModel PlaceNewPiece(PlayerModel forPlayer, Vector2Int onSpacePos)
        {
            var onSpace = GetOrCreateSpaceAt(onSpacePos);

            Debug.Assert(onSpace.Piece == null);

            var clone = Clone();
            clone.GetOrCreateSpaceAt(onSpace.Pos).Piece = new PieceModel(forPlayer);
            IncrementPieceCounts(clone, forPlayer, 1);
            return clone;
        }

        public BoardStateModel RemovePiece(Vector2Int onSpacePos)
        {
            var clone = Clone();
            clone.RemovePieceInPlace(onSpacePos);
            return clone;
        }

        // PERF: Removing a piece always happens as part of a move, so we can reuse the move's clone to avoid unnecessary cloning
        public void RemovePieceInPlace(Vector2Int onSpacePos)
        {
            var space = GetOrCreateSpaceAt(onSpacePos);
            if (space.Piece != null)
            {
                IncrementPieceCounts(this, space.Piece.Owner, -1);
                space.Piece = null;
            }
        }

        public BoardStateModel MovePiece(Vector2Int fromSpacePos, Vector2Int toSpacePos)
        {
            var clone = Clone();
            var fromSpace = GetOrCreateSpaceAt(fromSpacePos);
            var movePiece = fromSpace.Piece;
            clone.GetOrCreateSpaceAt(fromSpace.Pos).Piece = null;

            Debug.Assert(movePiece != null);

            clone.GetOrCreateSpaceAt(toSpacePos).Piece = movePiece;

            return clone;
        }

        bool IsInBounds(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= Size.x || pos.y >= Size.y) return false;
            else return true;
        }

        SpaceModel GetOrCreateSpaceAt(Vector2Int pos)
        {
            if (!IsInBounds(pos)) return null;
            else if (!_spaces.ContainsKey(pos))
            {
                var space = new SpaceModel(pos) { Piece = null };
                _spaces.Add(pos, space);
                return space;
            }
            else return _spaces[pos];
        }

        public int GetNumPieces() => _numPieces;
        public IEnumerable<Vector2Int> GetPiecesForPlayer(PlayerModel owner)
        {
            foreach (var kvp in _spaces)
            {
                var space = kvp.Value;
                var piece = space.Piece;
                if (piece != null && piece.Owner == owner)
                {
                    yield return space.Pos;
                }
            }
        }

        public int GetNumPiecesForPlayer(PlayerModel owner) => _numPiecesPerPlayer.ContainsKey(owner) ? _numPiecesPerPlayer[owner] : 0;

        public IEnumerable<Vector2Int> GetColumnOfSpaces(int xColumn)
        {
            for (var y = 0; y < Size.y; y++)
            {
                yield return new Vector2Int(xColumn, y);
            }
        }

        public IEnumerable<Vector2Int> GetRowOfSpaces(int yRow)
        {
            for (var x = 0; x < Size.x; x++)
            {
                yield return new Vector2Int(x, yRow);
            }
        }

        public bool IsStraightLine(Vector2Int from, Vector2Int to) => IsVerticalStraightLine(from, to) ^ IsHorizontalStraightLine(from, to);
        public bool IsVerticalStraightLine(Vector2Int from, Vector2Int to) => from.x == to.x;
        public bool IsHorizontalStraightLine(Vector2Int from, Vector2Int to) => from.y == to.y;

        public bool IsWholeLineEmpty(Vector2Int from, Vector2Int to)
        {
            if (IsVerticalStraightLine(from, to))
            {
                // column
                var minY = Mathf.Min(from.y, to.y);
                var maxY = Mathf.Max(from.y, to.y);
                for (var y = minY + 1; y < maxY; y++)
                {
                    if (!IsSpaceEmpty(new Vector2Int(from.x, y)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // row
                var minX = Mathf.Min(from.x, to.x);
                var maxX = Mathf.Max(from.x, to.x);
                for (var x = minX + 1; x < maxX; x++)
                {
                    if (!IsSpaceEmpty(new Vector2Int(x, from.y)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IEnumerable<Vector2Int> GetAdjacentSpaces(Vector2Int pos)
        {
            var left = pos - new Vector2Int(1, 0);
            var right = pos + new Vector2Int(1, 0);
            var below = pos - new Vector2Int(0, 1);
            var above = pos + new Vector2Int(0, 1);

            if (IsInBounds(left)) yield return left;
            if (IsInBounds(right)) yield return right;
            if (IsInBounds(below)) yield return below;
            if (IsInBounds(above)) yield return above;
        }

        public IEnumerable<Vector2Int> GetCornerSpaces()
        {
            yield return new Vector2Int(0, 0);
            yield return new Vector2Int(0, _height - 1);
            yield return new Vector2Int(_width - 1, 0);
            yield return new Vector2Int(_width - 1, _height - 1);
        }

        public Vector2Int GetDir(Vector2Int from, Vector2Int to)
        {
            var diff = to - from;
            var dir = new Vector2Int
            (
                diff.x != 0 ? Math.Sign(diff.x) : 0,
                diff.y != 0 ? Math.Sign(diff.y) : 0
            );

            return dir;
        }

        public Vector2Int GetNextInSameDirection(Vector2Int from, Vector2Int to)
        {
            var dir = GetDir(from, to);
            return to + dir;
        }

        public PieceModel GetPieceAt(Vector2Int pos)
        {
            if (!IsInBounds(pos)) return null;
            if (!_spaces.ContainsKey(pos)) return null;

            var space = GetOrCreateSpaceAt(pos);
            return space.Piece;
        }

        public bool IsSpaceEmpty(Vector2Int pos)
        {
            if (!IsInBounds(pos)) return true;
            if (!_spaces.ContainsKey(pos)) return true;

            var space = GetOrCreateSpaceAt(pos);
            return space.IsEmpty;
        }
    }
    #endregion
}
