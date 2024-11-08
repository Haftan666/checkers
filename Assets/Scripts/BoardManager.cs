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

    public GameObject whitePiecesParent { get; set; }
    public GameObject blackPiecesParent { get; set; }

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    private Piece selectedPiece;
    private bool isWhiteTurn = true;

    public Piece SelectedPiece
    {
        get { return selectedPiece; }
        set { selectedPiece = value; }
    }

    public Vector2 StartDrag
    {
        get { return startDrag; }
        set { startDrag = value; }
    }

    public bool IsWhiteTurn
    {
        get { return isWhiteTurn; }
        set { isWhiteTurn = value; }
    }

    private void Start()
    {
        BoardGenerator.GenerateBoard(this);
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
            PieceMovement.DragPiece(selectedPiece);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectedPiece != null)
            {
                PieceMovement.TryMove(this, (int)startDrag.x, (int)startDrag.y, x, y, selectedPiece.rank);
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
        if (x < 0 || x >= pieces.GetLength(0) || y < 0 || y >= pieces.GetLength(1))
            return;

        Piece p = pieces[x, y];
        if (p != null && ((p.team == Team.WHITE && isWhiteTurn) || p.team == Team.BLACK && !isWhiteTurn))
        {
            selectedPiece = p;
            startDrag = mouseOver;
            Debug.Log(selectedPiece.name);
        }
    }

    public void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        StartCoroutine(CameraController.RotateCamera(1.0f, isWhiteTurn));
    }

    public void CheckGameOver()
    {
        bool whiteHasPieces = false;
        bool blackHasPieces = false;

        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                if (piece.team == Team.WHITE)
                    whiteHasPieces = true;
                else if (piece.team == Team.BLACK)
                    blackHasPieces = true;
            }
        }

        if (!whiteHasPieces || !blackHasPieces)
        {
            Team winningTeam = whiteHasPieces ? Team.WHITE : Team.BLACK;
            GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
            gameOverManager.ShowGameOverScreen(winningTeam);
        }
    }

}
