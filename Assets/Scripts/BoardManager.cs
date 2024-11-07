using System.Collections;
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
    public Vector3 boardOffset = new Vector3(-3.507f, 0.3571f, -3.485f);
    public Vector3 pieceOffset = new Vector3(3.23f - 3.507f, 0, 3.37f - 3.485f);

    private GameObject whitePiecesParent;
    private GameObject blackPiecesParent;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    private Piece selectedPiece;
    private bool isWhiteTurn = true;


    private void Start()
    {
        GenerateBoard();
    }

    private void Update()
    {
        UpdateMouseOver();

        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;

        if (Input.GetMouseButtonDown(0))
        {
            SelectPiece(x, y);
        }

        if (selectedPiece != null)
        {
            DragPiece(selectedPiece);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectedPiece != null)
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y, selectedPiece.rank);
                selectedPiece = null;
            }
        }
    }

    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            float boardSquareSize = 1.0f;
            mouseOver.x = Mathf.FloorToInt(hit.point.x + boardSquareSize / 2);
            mouseOver.y = Mathf.FloorToInt(hit.point.z + boardSquareSize / 2);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private void SelectPiece(int x, int y)
    {
        if (x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length)
            return;

        Piece p = pieces[x, y];
        if (p != null && ((p.team == Team.WHITE && isWhiteTurn) || p.team == Team.BLACK && !isWhiteTurn))
        {
            selectedPiece = p;
            startDrag = mouseOver;
            Debug.Log(selectedPiece.name);
        }
    }

    private void DragPiece(Piece piece)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            piece.transform.position = hit.point + Vector3.up * 0.5f;
        }
    }

    private void TryMove(int startX, int startY, int endX, int endY, Rank rank)
    {
        if (IsValidMove(startX, startY, endX, endY, rank))
        {
            Piece piece = pieces[startX, startY];
            pieces[endX, endY] = piece;
            pieces[startX, startY] = null;
            piece.transform.position = new Vector3(endX - 0.277f, 0, endY - 0.115f);

            PromotePieceIfNeeded(piece, endY);

            bool captured = false;
            if (rank == Rank.Knight)
            {
                captured = CapturePawnsPassedByKnight(startX, startY, endX, endY);
            }
            else
            {
                captured = CapturePawnIfNeeded(startX, startY, endX, endY);
            }

            // Check if the piece can continue capturing
            if (captured && CanCapture(endX, endY, piece.rank))
            {
                selectedPiece = piece;
                startDrag = new Vector2(endX, endY);
                return; // Do not end the turn
            }

            isWhiteTurn = !isWhiteTurn;
            StartCoroutine(RotateCamera(1.0f)); // Rotate over 1 second
        }
        else
        {
            // Invalid move, reset the piece position
            selectedPiece.transform.position = new Vector3(startX - 0.277f, 0, startY - 0.115f);
        }
    }

    private void PromotePieceIfNeeded(Piece piece, int endY)
    {
        if (piece.team == Team.WHITE && endY == 7)
        {
            piece.transform.rotation = Quaternion.Euler(90, 0, 0);
            piece.rank = Rank.Knight;
        }
        else if (piece.team == Team.BLACK && endY == 0)
        {
            piece.transform.rotation = Quaternion.Euler(90, 0, 180);
            piece.rank = Rank.Knight;
        }
    }

    private bool CapturePawnsPassedByKnight(int startX, int startY, int endX, int endY)
    {
        bool captured = false;
        int stepX = (endX - startX) / Mathf.Abs(endX - startX);
        int stepY = (endY - startY) / Mathf.Abs(endY - startY);
        int x = startX + stepX;
        int y = startY + stepY;

        while (x != endX && y != endY)
        {
            Piece midPiece = pieces[x, y];
            if (midPiece != null && midPiece.team != selectedPiece.team && midPiece.rank == Rank.Pawn)
            {
                pieces[x, y] = null;
                Destroy(midPiece.gameObject);
                captured = true;
            }
            x += stepX;
            y += stepY;
        }

        return captured;
    }

    private bool CapturePawnIfNeeded(int startX, int startY, int endX, int endY)
    {
        bool captured = Mathf.Abs(endX - startX) == 2 && Mathf.Abs(endY - startY) == 2;
        if (captured)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece capturedPiece = pieces[midX, midY];
            if (capturedPiece != null)
            {
                pieces[midX, midY] = null;
                Destroy(capturedPiece.gameObject);
            }
        }

        return captured;
    }



    private bool CanCapture(int x, int y, Rank rank)
    {
        int[] dx = { 2, 2, -2, -2 };
        int[] dy = { 2, -2, 2, -2 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];

            if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                int midX = (x + newX) / 2;
                int midY = (y + newY) / 2;
                Piece midPiece = pieces[midX, midY];

                if (midPiece != null && midPiece.team != selectedPiece.team && pieces[newX, newY] == null)
                {
                    return true;
                }
            }
        }

        return false;
    }



    private bool IsValidMove(int startX, int startY, int endX, int endY, Rank rank)
    {
        if (IsOutOfBounds(endX, endY) || IsOccupied(endX, endY))
        {
            return false;
        }

        int deltaX = Mathf.Abs(endX - startX);
        int deltaY = Mathf.Abs(endY - startY);

        bool mustCapture = MustCapture();

        if (rank == Rank.Pawn)
        {
            return IsValidPawnMove(startX, startY, endX, endY, deltaX, deltaY, mustCapture);
        }
        else if (rank == Rank.Knight)
        {
            return IsValidKnightMove(startX, startY, endX, endY, deltaX, deltaY, mustCapture);
        }

        return false;
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || x >= 8 || y < 0 || y >= 8;
    }

    private bool IsOccupied(int x, int y)
    {
        return pieces[x, y] != null;
    }

    private bool MustCapture()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (pieces[x, y] != null && pieces[x, y].team == selectedPiece.team && CanCapture(x, y, pieces[x, y].rank))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsValidPawnMove(int startX, int startY, int endX, int endY, int deltaX, int deltaY, bool mustCapture)
    {
        if (deltaX == 1 && deltaY == 1)
        {
            if ((selectedPiece.team == Team.WHITE && endY > startY) || (selectedPiece.team == Team.BLACK && endY < startY))
            {
                return !mustCapture;
            }
        }

        if (deltaX == 2 && deltaY == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece midPiece = pieces[midX, midY];
            if (midPiece != null && midPiece.team != selectedPiece.team)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidKnightMove(int startX, int startY, int endX, int endY, int deltaX, int deltaY, bool mustCapture)
    {
        if (deltaX == deltaY)
        {
            if (mustCapture && !CanCapture(startX, startY, Rank.Knight))
            {
                return false;
            }

            if (deltaX == 2 && deltaY == 2)
            {
                int midX = (startX + endX) / 2;
                int midY = (startY + endY) / 2;
                Piece midPiece = pieces[midX, midY];
                if (midPiece != null && midPiece.team != selectedPiece.team)
                {
                    return true;
                }
            }

            return true;
        }

        return false;
    }






    private void GenerateBoard()
    {
        whitePiecesParent = new GameObject("WhitePieces");
        whitePiecesParent.transform.SetParent(transform);
        whitePiecesParent.transform.localPosition = boardOffset;

        blackPiecesParent = new GameObject("BlackPieces");
        blackPiecesParent.transform.SetParent(transform);
        blackPiecesParent.transform.localPosition = boardOffset;

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
        pieceObject.transform.localRotation = team == Team.WHITE ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(-90, 180, 0);

        pieceObject.transform.SetParent(team == Team.WHITE ? whitePiecesParent.transform : blackPiecesParent.transform, false);


        Piece p = pieceObject.GetComponent<Piece>();
        pieces[x, z] = p;
    }

    private IEnumerator RotateCamera(float duration)
    {
        Quaternion startRotation = Camera.main.transform.rotation;
        Quaternion endRotation = isWhiteTurn ? startRotation * Quaternion.Euler(0, 0, 180) : startRotation * Quaternion.Euler(0, 0, -180);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Camera.main.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.rotation = endRotation;
    }

}