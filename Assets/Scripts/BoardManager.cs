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
        if (p != null)
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

            if(piece.team == Team.WHITE && endY == 7)
            {
                piece.transform.rotation = Quaternion.Euler(90, 0, 0);
                piece.rank = Rank.Knight;
            }
            else if(piece.team == Team.BLACK && endY == 0)
            {
                piece.transform.rotation = Quaternion.Euler(90, 0, 0);
                piece.rank = Rank.Knight;
            }
        }
        else
        {
            // Invalid move, reset the piece position
            selectedPiece.transform.position = new Vector3(startX - 0.277f, 0, startY - 0.115f);
        }
    }


    private bool IsValidMove(int startX, int startY, int endX, int endY, Rank rank)
    {
        if (endX < 0 || endX >= 8 || endY < 0 || endY >= 8)
        {
            return false;
        }

        if (pieces[endX, endY] != null)
        {
            return false;
        }

        if ((Mathf.Abs(endX - startX) != 1 || Mathf.Abs(endY - startY) != 1) && rank == Rank.Pawn)
        {
            return false;
        }

        if ((Mathf.Abs(endX - startX) !=  Mathf.Abs(endY - startY)) && rank == Rank.Knight)
        {
            return false;
        }

        if (endY > startY && selectedPiece.team == Team.BLACK && rank != Rank.Knight)
        {
            return false;
        }

        if (endY < startY && selectedPiece.team == Team.WHITE && rank != Rank.Knight)
        {
            return false;
        }



        return true;
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
}