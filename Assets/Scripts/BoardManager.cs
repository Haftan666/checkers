using Unity.Properties;
using UnityEngine;

public enum Team
{
    WHITE,
    BLACK
}

public class BoardManager : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private GameObject whitePiecesParent;
    private GameObject blackPiecesParent;

    private void Start()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        // Utwórz pusty obiekt WhitePieces
        whitePiecesParent = new GameObject("WhitePieces");
        whitePiecesParent.transform.SetParent(transform);
        whitePiecesParent.transform.localPosition = new Vector3(-3.507f, 0.3571f, -3.485f);

        // Utwórz pusty obiekt BlackPieces
        blackPiecesParent = new GameObject("BlackPieces");
        blackPiecesParent.transform.SetParent(transform);
        blackPiecesParent.transform.localPosition = new Vector3(-3.507f, 0.3571f, -3.485f);

        GenerateWhiteTeam();
        GenerateBlackTeam();
    }

    private void GenerateWhiteTeam()
    {
        for (int x = 0; x < 8; x += 2)
        {
            for (int z = 0; z < 3; z++)
            {
                if (z % 2 == 0)
                {
                    GeneratePiece(x, z, Team.WHITE);
                }
                else
                {
                    GeneratePiece(x + 1, z, Team.WHITE);
                }
            }
        }
    }

    private void GenerateBlackTeam()
    {
        for (int x = 0; x < 8; x += 2)
        {
            for (int z = 5; z < 8; z++)
            {
                if (z % 2 == 0)
                {
                    GeneratePiece(x, z, Team.BLACK);
                }
                else
                {
                    GeneratePiece(x + 1, z, Team.BLACK);
                }
            }
        }
    }

    private void GeneratePiece(int x, int z, Team team)
    {
        GameObject piecePrefab = team == Team.WHITE ? whitePiecePrefab : blackPiecePrefab;
        GameObject pieceObject = Instantiate(piecePrefab) as GameObject;

        Vector3 correctedPosition = new Vector3(x, 0, z);
        pieceObject.transform.localPosition = correctedPosition;
        pieceObject.transform.localRotation = team == Team.WHITE ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(90, 180, 0);

        // Ustawienie rodzica dla białych i czarnych pionków
        if (team == Team.WHITE)
        {
            pieceObject.transform.SetParent(whitePiecesParent.transform, false);
        }
        else
        {
            pieceObject.transform.SetParent(blackPiecesParent.transform, false);
        }

        Piece p = pieceObject.GetComponent<Piece>();
        pieces[x, z] = p;
    }
}
