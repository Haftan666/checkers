using UnityEngine;

public static class BoardGenerator
{
    public static void GenerateBoard(BoardManager boardManager)
    {
        boardManager.whitePiecesParent = new GameObject("WhitePieces");
        boardManager.whitePiecesParent.transform.SetParent(boardManager.transform);
        boardManager.whitePiecesParent.transform.localPosition = boardManager.boardOffset;

        boardManager.blackPiecesParent = new GameObject("BlackPieces");
        boardManager.blackPiecesParent.transform.SetParent(boardManager.transform);
        boardManager.blackPiecesParent.transform.localPosition = boardManager.boardOffset;

        GenerateWhiteTeam(boardManager);
        GenerateBlackTeam(boardManager);
    }

    private static void GenerateWhiteTeam(BoardManager boardManager)
    {
        for (int x = 0; x < 8; x += 2)
        {
            for (int z = 0; z < 3; z++)
            {
                if (z % 2 == 0)
                {
                    GeneratePiece(boardManager, x, z, Team.WHITE);
                }
                else
                {
                    GeneratePiece(boardManager, x + 1, z, Team.WHITE);
                }
            }
        }
    }

    private static void GenerateBlackTeam(BoardManager boardManager)
    {
        for (int x = 0; x < 8; x += 2)
        {
            for (int z = 5; z < 8; z++)
            {
                if (z % 2 == 0)
                {
                    GeneratePiece(boardManager, x, z, Team.BLACK);
                }
                else
                {
                    GeneratePiece(boardManager, x + 1, z, Team.BLACK);
                }
            }
        }
    }

    private static void GeneratePiece(BoardManager boardManager, int x, int z, Team team)
    {
        GameObject piecePrefab = team == Team.WHITE ? boardManager.whitePiecePrefab : boardManager.blackPiecePrefab;
        GameObject pieceObject = Object.Instantiate(piecePrefab) as GameObject;

        Vector3 correctedPosition = new Vector3(x, 0, z);
        pieceObject.transform.localPosition = correctedPosition;
        pieceObject.transform.localRotation = team == Team.WHITE ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(-90, 180, 0);

        pieceObject.transform.SetParent(team == Team.WHITE ? boardManager.whitePiecesParent.transform : boardManager.blackPiecesParent.transform, false);

        Piece p = pieceObject.GetComponent<Piece>();
        boardManager.pieces[x, z] = p;
    }
}
