using System;
using UnityEngine;

public enum  Rank
{
    Pawn,
    Knight
}

public class Piece : MonoBehaviour
{
    public Team team;
    public Rank rank;
}
