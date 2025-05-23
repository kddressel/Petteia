using Assets.Petteia.Scripts.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPiece
{
    string PieceType { get; }
}

public class PetteiaEnemyPiece : MonoBehaviour, IPiece
{
	public Vector2Int pieceStartPos;
	public GameObject highlight;

    public string pieceType;
    public string PieceType => pieceType;

    void Start()
    {
        if (pieceType == "King" && RulesFactory.UseKing)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
        }
    }
}
