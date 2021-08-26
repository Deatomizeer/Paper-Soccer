using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Prefabs.
    public GameObject cellPrefab;
    public GameObject edgePrefab;
    // Board size.
    const int boardWidth = 9;
    const int boardHeight = 13;
    // The board logic (line interjections) and view (sprites).
    InterjectionAbstract[,] boardLogic = new InterjectionAbstract[boardHeight, boardWidth];
    GameObject[,] boardView = new GameObject[boardHeight, boardWidth];
    // Ball position.
    int xBall = (boardWidth-1)/2;
    int yBall = (boardHeight-1)/2;
    // Moves that have already been made in this game.
    public class GameMove
    {
        public GameObject firstNode;
        public GameObject secondNode;
        public GameMove(GameObject firstNode, GameObject secondNode)
        {
            this.firstNode = firstNode;
            this.secondNode = secondNode;
        }
    }
    List<GameMove> pastMoves = new List<GameMove>();
    List<GameMove> boardBoundaries = new List<GameMove>();   // Moving the ball along the boundary line is not allowed.    
    // Set of nodes that the ball can be moves into on the next move.
    List<GameObject> legalMoveDestinations;

    // Start is called before the first frame update
    void Start()
    {
        // Create the board.
        for(int i = 0; i < boardHeight; i++)
        {
            for(int j = 0; j < boardWidth; j++ )
            {
                boardLogic[i, j] = CreateNextCell(i, j);
            }
        }
        // Instantiate the "view" side of the board.
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                GameObject currentCell = Instantiate(cellPrefab, new Vector3(0 + j, 0 + i, 0), Quaternion.identity);
                currentCell.GetComponent<Cell>().thisInterjection = boardLogic[i, j];
                boardView[i, j] = currentCell;
            }
        }
        // Correct the camera's position.
        Camera.main.gameObject.GetComponent<CenterCamera>().CenterCameraOnBoard(boardWidth, boardHeight);
        // Create field boundaries.
        for(int j=0; j < boardWidth-1; j++)
        {
            // Horizontal boundaries.
            boardBoundaries.Add(new GameMove(boardView[0, j], boardView[0, j + 1]));
            boardBoundaries.Add(new GameMove(boardView[boardHeight-1, j], boardView[boardHeight-1, j + 1]));
            if (j != (boardWidth - 1) / 2)
            {
                // Vertical boundaries between the two horizontal ones, except where goal is.
                boardBoundaries.Add(new GameMove(boardView[0, j], boardView[1, j]));
                boardBoundaries.Add(new GameMove(boardView[boardHeight-1, j], boardView[boardHeight-2, j]));
                if ( j != ((boardWidth - 1) / 2) - 1)
                {
                    // Horizontal boundaries, but closer to the middle.
                    boardBoundaries.Add(new GameMove(boardView[1, j], boardView[1, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[boardHeight - 2, j], boardView[boardHeight - 2, j + 1]));
                    // Diagonal boundaries between the two horizontal rows.
                    boardBoundaries.Add(new GameMove(boardView[0, j], boardView[1, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[boardHeight-1, j], boardView[boardHeight-2, j + 1]));
                }
            }
        }
        for(int i=1; i<boardHeight-1; i++)
        {
            // Vertical boundaries on left and right.
            boardBoundaries.Add(new GameMove(boardView[i, 0], boardView[i+1, 0]));
            boardBoundaries.Add(new GameMove(boardView[i, boardWidth-1], boardView[i + 1, boardWidth - 1]));
        }
        // Render the boundaries as game objects.
        for(int i = 0; i < boardBoundaries.Count; i++)
        {
            GameMove edge = boardBoundaries[i];
            float edgeX = (edge.firstNode.transform.position.x + edge.secondNode.transform.position.x) * .5f;
            float edgeY = (edge.firstNode.transform.position.y + edge.secondNode.transform.position.y) * .5f;
            Vector3 edgePos = new Vector3(edgeX, edgeY, 0);
            Instantiate(edgePrefab, edgePos, Quaternion.identity);
        }
    }

    // Determine what should the cell be based on its position.
    InterjectionAbstract CreateNextCell(int i, int j)
    {
        // Top or bottom side.
        if (i == 0 || i == boardHeight - 1)
        {
            // Put a goal in the middle, otherwise put wall.
            if (j == (boardWidth - 1) / 2)
            {
                GoalAffiliation aff = (i == 0 ? GoalAffiliation.PLAYER1 : GoalAffiliation.PLAYER2);
                return new Goal(aff);
            }
            else
            {
                return new Interjection(InterjectionStatus.WALL, false);
            }
        }
        // Almost top or bottom.
        if (i == 1 || i == boardHeight - 2)
        {
            // Put an empty space in the middle, otherwise put wall.
            if (j == (boardWidth - 1) / 2)
            {
                return new Interjection(InterjectionStatus.FREE, false);
            }
            else
            {
                return new Interjection(InterjectionStatus.WALL, false);
            }
        }
        // The large, middle part of the board.
        else
        {
            if (j == 0 || j == boardWidth - 1)
            {
                return new Interjection(InterjectionStatus.WALL, false);
            }
            if (j == xBall && i == yBall)
            {
                return new Interjection(InterjectionStatus.FREE, true);
            }
            else
            {
                return new Interjection(InterjectionStatus.FREE, false);
            }
        }
        
        

    }
    
    // Find all allowed moves from current ball position.
    List<GameObject> GetLegalMoves()
    {
        List<GameObject> moves = new List<GameObject>();
        // Ensure the search scope won't try reaching out of bounds.
        int iterStartX = xBall > 0 ? xBall - 1 : 0;
        int iterEndX = xBall < boardWidth - 1 ? xBall + 1 : boardWidth;
        int iterStartY = yBall > 0 ? yBall - 1 : 0;
        int iterEndY = yBall < boardHeight - 1 ? yBall + 1 : boardHeight;

        return moves;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
