using Assets.Petteia.Scripts.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PetteiaPlayerPiece : MonoBehaviour
{
  public Vector2Int pieceStartPos;
  public Camera cam;
  public PetteiaGameController pController;
  public MeshRenderer real;
  public GameObject highlight;
  public GameObject dummyParent;
  public GameObject dummy;

  private GameObject dummySpawned;
  private int mask;
  private bool active = false;

  private Vector2Int potentialPos;
  private List<PetteiaBoardPosition> validMoves = new List<PetteiaBoardPosition>();
  private bool gameStarted = false;

  void Start()
  {
    if (real != null)
    {
      real.enabled = true;
    }


    mask = LayerMask.GetMask("GameSquare");

    potentialPos = pieceStartPos;
    _goalPos = transform.position;
  }

  Vector3 _goalPos;

  void FixedUpdate()
  {
    //Active turns on when the piece is clicked and off on mouse up
    //This makes sure only the one piece is moved via raycast and not every piece
    if (pController.PlayerTurn && active)
    {
      RaycastHit hit;
      Ray ray = cam.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit, 10000f, mask, QueryTriggerInteraction.Collide))
      {
        PetteiaBoardPosition pcm = hit.collider.GetComponent<PetteiaBoardPosition>();
        if (validMoves.Contains(pcm))
        {
          potentialPos = pcm.position;
          _goalPos = hit.transform.position;
        }
      }
    }

    transform.position = Vector3.Lerp(transform.position, _goalPos, 0.1f);
  }

  void OnMouseDown()
  {
    //Apparently OnMouseDown and similar still fire on a disabled script
    //This means the player could click on an enemy piece and cause problems
    //which is why we check if the script is enabled first
    if (enabled && gameStarted)
    {
      //OnMouseDown will still register through UI, so this checks if it's doing that
      //It doesn't work 100% of the time, but often enough to fix the majority of issues
      if (EventSystem.current.IsPointerOverGameObject())
      {
        return;
      }

      if (pController.PlayerTurn)
      {
        active = true;
        if (real != null)
        {
          validMoves = PopulateValidMovesList(pieceStartPos);
          foreach (PetteiaBoardPosition p in validMoves)
          {
            p.HighlightSpace(true);
          }
          SpawnDummy();
        }
      }
    }
  }

  private void SpawnDummy()
  {
    dummySpawned = Instantiate(dummy, dummyParent.transform);
    dummySpawned.transform.position = transform.position;
  }

  public void DestroyDummy()
  {
    if (dummySpawned != null)
    {
      Destroy(dummySpawned);
    }
  }

  void OnMouseUp()
  {
    if (enabled)
    {
      if (EventSystem.current.IsPointerOverGameObject())
      {
        return;
      }

      active = false;
      if (real != null)
      {
        //Only advances the turn if the piece was moved
        if (!(pieceStartPos.x == potentialPos.x && pieceStartPos.y == potentialPos.y))
        {
          pController.MovePiece(pieceStartPos, potentialPos, pController.playerTag);
          pieceStartPos = potentialPos;
          pController.SwitchTurn();
          //pController.PlayMoveSound();
        }

        //Valid moves depend on which piece is being moved, so if the player
        //drops one piece, they don't count anymore
        foreach (PetteiaBoardPosition p in validMoves)
        {
          p.HighlightSpace(false);
        }
        real.enabled = true;
        DestroyDummy();
      }
    }

  }

  /// <summary>
  /// Gathers a list of every valid move a given piece can make
  /// </summary>
  /// <param name="startPos"></param>
  /// <returns></returns>
  public List<PetteiaBoardPosition> PopulateValidMovesList(Vector2Int startPos)
  {
    List<PetteiaBoardPosition> possibleMoves = new List<PetteiaBoardPosition>();
    possibleMoves.Add(pController.BoardSquares[startPos.x, startPos.y]);

    // TODO: copy pasted 3 times now at least
    var playerDefs = new PlayerDef[]
    {
                new PlayerDef { Name = "You (X)", AgentType = typeof(HumanPlayerAgent) },
                new PlayerDef { Name = "Other (O)", AgentType = typeof(HumanPlayerAgent) }
    };

    var gameModel = new GameModel<RulesSet>(playerDefs, 8, 8, RulesFactory.MakeRulesSet());
    for (int y = 0; y < 8; y++)
    {
      for (int x = 0; x < 8; x++)
      {
        if (pController.positions[x, y] == 1)
        {
          gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[0], new Vector2Int(x, y));
        }
        else if (pController.positions[x, y] == 2)
        {
          gameModel.Board = gameModel.Board.PlaceNewPiece(gameModel.Players[1], new Vector2Int(x, y));
        }
      }
    }

    for (int y = 0; y < pController.BoardSquares.GetLength(1); y++)
    {
      for (int x = 0; x < pController.BoardSquares.GetLength(0); x++)
      {
        if (gameModel.Rules.IsValidMove(gameModel.Board, startPos, new Vector2Int(x, y)))
        {
          possibleMoves.Add(pController.BoardSquares[x, y]);
        }
      }
    }

    return possibleMoves;
  }

  /// <summary>
  /// Turns the player turn highlight on or off
  /// </summary>
  /// <param name="toggle"></param>
  public void ToggleHighlight(bool toggle)
  {
    highlight.SetActive(toggle);
  }

  public void StartGame()
  {
    gameStarted = true;
  }
}
