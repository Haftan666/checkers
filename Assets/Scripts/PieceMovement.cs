using UnityEngine;

public static class PieceMovement
{
    public static void DragPiece(Piece piece)
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

    public static void TryMove(BoardManager boardManager, int startX, int startY, int endX, int endY, Rank rank)
    {
        if (IsValidMove(boardManager, startX, startY, endX, endY, rank))
        {
            Piece piece = boardManager.pieces[startX, startY];
            boardManager.pieces[endX, endY] = piece;
            boardManager.pieces[startX, startY] = null;
            piece.transform.position = new Vector3(endX - 0.277f, 0, endY - 0.115f);

            bool captured = false;
            if (rank == Rank.Knight)
            {
                captured = CapturePawnsPassedByKnight(boardManager, startX, startY, endX, endY);
            }
            else
            {
                captured = CapturePawnIfNeeded(boardManager, startX, startY, endX, endY);
            }

            PromotePieceIfNeeded(boardManager, piece, endX, endY);

            if (captured && CanCapture(boardManager, endX, endY, piece.rank))
            {
                boardManager.SelectedPiece = piece;
                boardManager.StartDrag = new Vector2(endX, endY);
                boardManager.PlayPiecePlacedSound();
                return;
            }

            boardManager.IsWhiteTurn = !boardManager.IsWhiteTurn;
            boardManager.CheckGameOver();
            if (boardManager.isGameOver)
            {
                return;
            }
            boardManager.StartCoroutine(CameraController.RotateCamera(1.0f, boardManager.IsWhiteTurn));
            boardManager.PlayPiecePlacedSound();


        }
        else
        {
            boardManager.SelectedPiece.transform.position = new Vector3(startX - 0.277f, 0, startY - 0.115f);
        }
    }




    public static void PromotePieceIfNeeded(BoardManager boardManager, Piece piece, int endX, int endY)
    {
        bool canCapture = CanCapture(boardManager, endX, endY, piece.rank);

        if (!canCapture)
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
        else
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
    }



    public static bool CapturePawnsPassedByKnight(BoardManager boardManager, int startX, int startY, int endX, int endY)
    {
        bool captured = false;
        int stepX = (endX - startX) / Mathf.Abs(endX - startX);
        int stepY = (endY - startY) / Mathf.Abs(endY - startY);
        int x = startX + stepX;
        int y = startY + stepY;

        while (x != endX && y != endY)
        {
            Piece midPiece = boardManager.pieces[x, y];
            if (midPiece != null && midPiece.team != boardManager.SelectedPiece.team && midPiece.rank == Rank.Pawn)
            {
                boardManager.pieces[x, y] = null;
                AddPhysicsAndLaunch(midPiece);
                captured = true;
            }
            x += stepX;
            y += stepY;
        }

        return captured;
    }



    public static bool CapturePawnIfNeeded(BoardManager boardManager, int startX, int startY, int endX, int endY)
    {
        bool captured = Mathf.Abs(endX - startX) == 2 && Mathf.Abs(endY - startY) == 2;
        if (captured)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece capturedPiece = boardManager.pieces[midX, midY];
            if (capturedPiece != null)
            {
                boardManager.pieces[midX, midY] = null;
                AddPhysicsAndLaunch(capturedPiece);
            }
        }

        return captured;
    }


    public static bool CanCapture(BoardManager boardManager, int x, int y, Rank rank)
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
                Piece midPiece = boardManager.pieces[midX, midY];

                if (midPiece != null && midPiece.team != boardManager.SelectedPiece.team && boardManager.pieces[newX, newY] == null)
                {
                    return true;
                }
            }
        }

        return false;
    }


    public static bool IsValidMove(BoardManager boardManager, int startX, int startY, int endX, int endY, Rank rank)
    {
        if (IsOutOfBounds(endX, endY) || IsOccupied(boardManager, endX, endY))
        {
            return false;
        }

        int deltaX = Mathf.Abs(endX - startX);
        int deltaY = Mathf.Abs(endY - startY);

        bool mustCapture = MustCapture(boardManager);

        if (rank == Rank.Pawn)
        {
            return IsValidPawnMove(boardManager, startX, startY, endX, endY, deltaX, deltaY, mustCapture);
        }
        else if (rank == Rank.Knight)
        {
            return IsValidKnightMove(boardManager, startX, startY, endX, endY, deltaX, deltaY, mustCapture);
        }

        return false;
    }

    public static bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || x >= 8 || y < 0 || y >= 8;
    }

    public static bool IsOccupied(BoardManager boardManager, int x, int y)
    {
        return boardManager.pieces[x, y] != null;
    }

    public static bool MustCapture(BoardManager boardManager)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (boardManager.pieces[x, y] != null && boardManager.pieces[x, y].team == boardManager.SelectedPiece.team && CanCapture(boardManager, x, y, boardManager.pieces[x, y].rank))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsValidPawnMove(BoardManager boardManager, int startX, int startY, int endX, int endY, int deltaX, int deltaY, bool mustCapture)
    {
        if (deltaX == 1 && deltaY == 1)
        {
            if ((boardManager.SelectedPiece.team == Team.WHITE && endY > startY) || (boardManager.SelectedPiece.team == Team.BLACK && endY < startY))
            {
                return !mustCapture;
            }
        }

        if (deltaX == 2 && deltaY == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece midPiece = boardManager.pieces[midX, midY];
            if (midPiece != null && midPiece.team != boardManager.SelectedPiece.team)
            {
                return true;
            }
        }

        return false;
    }


    public static bool IsValidKnightMove(BoardManager boardManager, int startX, int startY, int endX, int endY, int deltaX, int deltaY, bool mustCapture)
    {
        if (deltaX == deltaY)
        {
            if (mustCapture && !CanCapture(boardManager, startX, startY, Rank.Knight))
            {
                return false;
            }

            if (deltaX == 2 && deltaY == 2)
            {
                int midX = (startX + endX) / 2;
                int midY = (startY + endY) / 2;
                Piece midPiece = boardManager.pieces[midX, midY];
                if (midPiece != null && midPiece.team != boardManager.SelectedPiece.team)
                {
                    return true;
                }
            }

            return true;
        }

        return false;
    }


    private static void AddPhysicsAndLaunch(Piece piece)
    {
        Rigidbody rb = piece.gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;

        Vector3 forceDirection = piece.team == Team.WHITE ? Vector3.right : Vector3.left;
        Vector3 force = (forceDirection + Vector3.up) * 10.0f; 

        rb.AddForce(force, ForceMode.Impulse);

        Vector3 torqueDirection = piece.team == Team.WHITE ? Vector3.forward : Vector3.back;
        float torqueMagnitude = 10.0f;

        rb.AddTorque(torqueDirection * torqueMagnitude, ForceMode.Impulse);
    }


}
