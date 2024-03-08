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
            var adjacentEnemySpaces = board.GetAdjacentSpaces(to).Where(space => !space.IsEmpty && space.Piece.Owner != currentTurn);

            // it is supposed to multi-capture if you surround pieces in multiple directions
            // moving your piece between two enemy pieces isn't a capture. enemy has to move to capture, so we need to check who's turn it is
            // TODO: Is it supposed to capture every piece in a line if there are multiple?
            foreach (var enemySpace in adjacentEnemySpaces)
            {
                var capturingSpace = board.GetNextInSameDirection(to, enemySpace.Pos);
                if (capturingSpace != null && capturingSpace.Piece?.Owner == currentTurn)
                {
                    yield return enemySpace.Pos;
                }
            }

            // TODO: Kennesaw students added a special rule for capturing in the 4 corners of the board, since those would be safe spaces otherwise
        }

        virtual public bool IsValidMove(BoardStateModel board, Vector2Int from, Vector2Int to)
        {
            var isStraightLine = board.IsStraightLine(from, to);
            var isNotOccupied = board.IsSpaceEmpty(to);
            var isNotBlocked = board.GetLineOfSpaces(from, to).All(space => space.IsEmpty);

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

    public class SpaceModel
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
        // TODO: Consider representing this as lists of pieces instead for performance in AI move space searching
        readonly SpaceModel[,] _spaces;

        public Vector2Int Size => new Vector2Int(_spaces.GetLength(0), _spaces.GetLength(1));

        public BoardStateModel(int width, int height)
        {
            _spaces = new SpaceModel[width, height];
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    _spaces[x, y] = new SpaceModel(new Vector2Int(x, y)) { Piece = null };
                }
            }
        }

        public BoardStateModel Clone()
        {
            var clone = new BoardStateModel(Size.x, Size.y);
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    clone._spaces[x, y] = _spaces[x, y].Clone();
                }
            }
            return clone;
        }

        public BoardStateModel PlaceNewPiece(PlayerModel forPlayer, SpaceModel onSpace)
        {
            Debug.Assert(onSpace.Piece == null);

            var clone = Clone();
            clone.GetSpaceAt(onSpace.Pos).Piece = new PieceModel(forPlayer);
            return clone;
        }

        public BoardStateModel RemovePiece(SpaceModel onSpace)
        {
            var clone = Clone();
            clone.GetSpaceAt(onSpace.Pos).Piece = null;
            return clone;
        }

        public BoardStateModel MovePiece(SpaceModel fromSpace, SpaceModel toSpace)
        {
            var clone = Clone();
            var movePiece = fromSpace.Piece;
            clone.GetSpaceAt(fromSpace.Pos).Piece = null;

            Debug.Assert(movePiece != null);

            clone.GetSpaceAt(toSpace.Pos).Piece = movePiece;

            return clone;
        }

        public SpaceModel GetSpaceAt(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= Size.x || pos.y >= Size.y) return null;
            else return _spaces[pos.x, pos.y];
        }

        public int GetNumPieces()
        {
            var count = 0;
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    count++;
                }
            }
            return count;
        }

        public IEnumerable<SpaceModel> GetPiecesForPlayer(PlayerModel owner)
        {
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    var space = _spaces[x, y];
                    var piece = space.Piece;
                    if (piece != null && piece.Owner == owner)
                    {
                        yield return space;
                    }
                }
            }
        }

        public int GetNumPiecesForPlayer(PlayerModel owner)
        {
            var count = 0;
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    var piece = _spaces[x, y].Piece;
                    if (piece != null && piece.Owner == owner)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public IEnumerable<SpaceModel> GetColumnOfSpaces(int xColumn)
        {
            for (var y = 0; y < Size.y; y++)
            {
                yield return _spaces[xColumn, y];
            }
        }

        public IEnumerable<SpaceModel> GetRowOfSpaces(int yRow)
        {
            for (var x = 0; x < Size.x; x++)
            {
                yield return _spaces[x, yRow];
            }
        }

        public bool IsStraightLine(Vector2Int from, Vector2Int to) => IsVerticalStraightLine(from, to) ^ IsHorizontalStraightLine(from, to);
        public bool IsVerticalStraightLine(Vector2Int from, Vector2Int to) => from.x == to.x;
        public bool IsHorizontalStraightLine(Vector2Int from, Vector2Int to) => from.y == to.y;

        public IEnumerable<SpaceModel> GetLineOfSpaces(Vector2Int from, Vector2Int to)
        {
            if (IsHorizontalStraightLine(from, to))
            {
                var row = GetRowOfSpaces(from.y);
                if (from.x < to.x)
                {
                    // left to right
                    return row.Where(space => space.Pos.x > from.x && space.Pos.x < to.x);
                }
                else
                {
                    // right to left
                    return row.Where(space => space.Pos.x > to.x && space.Pos.x < from.x);
                }
            }
            else if (IsVerticalStraightLine(from, to))
            {
                var col = GetColumnOfSpaces(from.x);
                if (from.y > to.y)
                {
                    // bottom to top
                    return col.Where(space => space.Pos.y < from.y && space.Pos.y > to.y);
                }
                else
                {
                    // top to bottom
                    return col.Where(space => space.Pos.y < to.y && space.Pos.y > from.y);
                }
            }
            else
            {
                return Enumerable.Empty<SpaceModel>();
            }
        }

        public IEnumerable<SpaceModel> GetAdjacentSpaces(Vector2Int pos)
        {
            var left = GetSpaceAt(pos - new Vector2Int(1, 0));
            var right = GetSpaceAt(pos + new Vector2Int(1, 0));
            var below = GetSpaceAt(pos - new Vector2Int(0, 1));
            var above = GetSpaceAt(pos + new Vector2Int(0, 1));

            if (left != null) yield return left;
            if (right != null) yield return right;
            if (below != null) yield return below;
            if (above != null) yield return above;
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

        public SpaceModel GetNextInSameDirection(Vector2Int from, Vector2Int to)
        {
            var dir = GetDir(from, to);
            return GetSpaceAt(to + dir);
        }

        public PieceModel GetPieceAt(Vector2Int pos)
        {
            var space = GetSpaceAt(pos);
            return space?.Piece;
        }

        public bool IsSpaceEmpty(Vector2Int pos)
        {
            var space = GetSpaceAt(pos);
            return space != null ? space.IsEmpty : true;
        }
    }
    #endregion
}
