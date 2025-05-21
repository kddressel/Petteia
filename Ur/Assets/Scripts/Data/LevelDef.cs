using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDef", menuName = "Petteia/LevelDef")]
public class LevelDef : ScriptableObject
{
  [Flags]
  public enum Rules
  {
    Basic = 0,
    Dice = 1,
    King = 2,
    PlacePieces = 4
  }

  public string Id => name;
  public AIDifficulty Difficulty;
  public Rules RuleSet;
}
