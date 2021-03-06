using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    // Prefabs.
    public GameObject cellPrefab;
    public GameObject edgePrefab;
    // Object references.
    public Text endGameText;
    public Text scoreText;
    public GameObject scoreKeeper;
    // Board size.
    const int boardWidth = 9;
    const int boardHeight = 13;
    // The board logic (line interjections) and view (sprites).
    InterjectionAbstract[,] boardLogic = new InterjectionAbstract[boardHeight, boardWidth];
    GameObject[,] boardView = new GameObject[boardHeight, boardWidth];
    // Ball position.
    int xBall = (boardWidth - 1) / 2;
    int yBall = (boardHeight - 1) / 2;
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
    List<GameObject> pastMovesView = new List<GameObject>(); // For ease of restarting the game.
    List<GameMove> boardBoundaries = new List<GameMove>();   // Moving the ball along the boundary line is not allowed.
    // Current player, used to prevent unauthorised movement during the opponent's turn.
    GoalAffiliation currentPlayer;
    // Current score (kept in a separate, persistent object).
    ScoreKeep score;

    // Start is called before the first frame update
    void Start()
    {
        // Make the score object persistent.
        DontDestroyOnLoad(scoreKeeper);
        // Hide endGameText until the game ends.
        endGameText.gameObject.active = false;
        // Load score from a separate object.
        score = scoreKeeper.GetComponent<ScoreKeep>();
        // And show it to the player.
        scoreText.text = "Current score: " + score.playerOneScore + ":" + score.playerTwoScore;
        // Create the board.
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
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
        for (int j = 0; j < boardWidth - 1; j++)
        {
            // Horizontal boundaries.
            boardBoundaries.Add(new GameMove(boardView[0, j], boardView[0, j + 1]));
            boardBoundaries.Add(new GameMove(boardView[boardHeight - 1, j], boardView[boardHeight - 1, j + 1]));
            if (j != (boardWidth - 1) / 2)
            {
                // Vertical boundaries between the two horizontal ones, except where goal is.
                boardBoundaries.Add(new GameMove(boardView[0, j], boardView[1, j]));
                boardBoundaries.Add(new GameMove(boardView[boardHeight - 1, j], boardView[boardHeight - 2, j]));
                if (j != ((boardWidth - 1) / 2) - 1)
                {
                    // Horizontal boundaries, but closer to the middle.
                    boardBoundaries.Add(new GameMove(boardView[1, j], boardView[1, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[boardHeight - 2, j], boardView[boardHeight - 2, j + 1]));
                    // Diagonal boundaries between the two horizontal rows.
                    boardBoundaries.Add(new GameMove(boardView[0, j], boardView[1, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[1, j], boardView[0, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[boardHeight - 1, j], boardView[boardHeight - 2, j + 1]));
                    boardBoundaries.Add(new GameMove(boardView[boardHeight - 2, j], boardView[boardHeight - 1, j + 1]));
                }
            }
        }
        for (int i = 0; i < boardHeight - 1; i++)
        {
            // Vertical boundaries on left and right.
            boardBoundaries.Add(new GameMove(boardView[i, 0], boardView[i + 1, 0]));
            boardBoundaries.Add(new GameMove(boardView[i, boardWidth - 1], boardView[i + 1, boardWidth - 1]));
        }
        // Render the boundaries as game objects.
        for (int i = 0; i < boardBoundaries.Count; i++)
        {
            GameMove edge = boardBoundaries[i];
            float edgeX = (edge.firstNode.transform.position.x + edge.secondNode.transform.position.x) * .5f;
            float edgeY = (edge.firstNode.transform.position.y + edge.secondNode.transform.position.y) * .5f;
            Vector3 edgePos = new Vector3(edgeX, edgeY, 0);
            Instantiate(edgePrefab, edgePos, Quaternion.identity);
        }
        // Copy board boundaries into moves made for ease of checking.
        pastMoves = new List<GameMove>(boardBoundaries);
        // Player1 (red) starts the game.
        currentPlayer = GoalAffiliation.PLAYER1;
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

    // Find all allowed moves from current ball position. Will probably break if called
    List<GameObject> GetLegalMoves()
    {
        List<GameObject> destinations = new List<GameObject>();
        // Ensure the search scope won't try reaching out of bounds.
        int iterStartX = xBall > 0 ? xBall - 1 : 0;
        int iterEndX = xBall < boardWidth - 1 ? xBall + 2 : boardWidth;
        int iterStartY = yBall > 0 ? yBall - 1 : 0;
        int iterEndY = yBall < boardHeight - 1 ? yBall + 2 : boardHeight;
        // Iterate over all neighbors.
        for (int i = iterStartY; i < iterEndY; i++)
        {
            for (int j = iterStartX; j < iterEndX; j++)
            {
                // Scoring a goal is always an option (at least with the regular board layout).
                if (boardLogic[i, j] is Goal)
                {
                    destinations.Add(boardView[i, j]);
                }
                // Skip self to prevent stalling the game.
                else if ((boardLogic[i, j] as Interjection).hasBall)
                {
                    continue;
                }
                else
                {
                    // See if this move (or its reverse) has already been made. If not, they are valid.
                    GameMove move = new GameMove(boardView[yBall, xBall], boardView[i, j]);
                    GameMove reverseMove = new GameMove(boardView[i, j], boardView[yBall, xBall]);
                    bool hasBeenMade = false;
                    foreach (GameMove m in pastMoves)
                    {
                        if (
                            (m.firstNode == move.firstNode && m.secondNode == move.secondNode)
                            || (m.firstNode == reverseMove.firstNode && m.secondNode == reverseMove.secondNode)
                        )
                        {
                            hasBeenMade = true;
                            break;
                        }
                    }
                    if (!hasBeenMade)
                    {
                        destinations.Add(boardView[i, j]);
                    }
                }
            }
        }
        string d = "Allowed moves: ";
        foreach (GameObject dest in destinations)
        {
            string tmp = "(" + dest.transform.position.x + ", " + dest.transform.position.y + ") ";
            d += tmp;
        }
        Debug.Log(d);
        return destinations;
    }

    // Try to move the ball to the selected node.
    public void MakeMove(GameObject dest)
    {
        if (currentPlayer != GoalAffiliation.PLAYER1)
        {
            // It's not your turn yet!
            return;
        }
        List<GameObject> legalMoves = GetLegalMoves();
        if (legalMoves.Count == 0)
        {
            Stalemate();
            return;
        }
        if (legalMoves.Contains(dest))
        {
            // All good, make this move.
            Cell destCell = dest.GetComponent<Cell>();
            if (destCell.thisInterjection is Goal)
            {
                ScoreGoal((destCell.thisInterjection as Goal).affiliation);
                return;
            }
            // Create a new edge after this move.
            GameMove thisMove = new GameMove(boardView[yBall, xBall], dest);
            pastMoves.Add(thisMove);
            float edgeX = (thisMove.firstNode.transform.position.x + thisMove.secondNode.transform.position.x) * .5f;
            float edgeY = (thisMove.firstNode.transform.position.y + thisMove.secondNode.transform.position.y) * .5f;
            Vector3 edgePos = new Vector3(edgeX, edgeY, 0);
            pastMovesView.Add(
                Instantiate(edgePrefab, edgePos, Quaternion.identity)
            );
            // Change the previous cell state to wall for logic purposes.
            (boardLogic[yBall, xBall] as Interjection).status = InterjectionStatus.WALL;
            // Move the ball to the destination.
            (boardLogic[yBall, xBall] as Interjection).hasBall = false;
            boardView[yBall, xBall].GetComponent<Cell>().spriteRenderer.color = Color.black;    // Wall color.
            (destCell.thisInterjection as Interjection).hasBall = true;
            destCell.spriteRenderer.color = Color.yellow;
            xBall = (int)dest.transform.position.x;
            yBall = (int)dest.transform.position.y;
            // End turn and let the AI move... or bounce off a wall!
            if ((boardLogic[yBall, xBall] as Interjection).status == InterjectionStatus.FREE)
            {
                currentPlayer = GoalAffiliation.PLAYER2;
                Invoke("MakeMoveAI", 1);    // Force the program to wait to make the AI feel more "real".
            }
        }
        //Debug.Log("Ball on (" + xBall.ToString() + ", " + yBall.ToString() + ")");
    }

    // Score a goal and end the round.
    public void ScoreGoal(GoalAffiliation aff)
    {
        endGameText.gameObject.active = true;
        switch(aff)
        {
            case GoalAffiliation.PLAYER1:
                endGameText.text = GoalAffiliation.PLAYER2 + " scored a goal!";
                score.playerTwoScore++;
                break;
            default:
                endGameText.text = GoalAffiliation.PLAYER1 + " scored a goal!";
                score.playerOneScore++;
                break;
        }
        scoreText.text = "Current score: " + score.playerOneScore + ":" + score.playerTwoScore;
    }

    public void Stalemate()
    {
        endGameText.gameObject.active = true;
        endGameText.text = "It's a draw!";
        Debug.Log("Everyone loses!");
    }

    // Opponent move.
    public void MakeMoveAI()
    {
        //Debug.Log("I would have made a move right now!");
        GameObject dest = null;    // The move to make.
        while ( currentPlayer == GoalAffiliation.PLAYER2 )
        {
            List<GameObject> legalMoves = GetLegalMoves();
            if( legalMoves.Count == 0 )
            {
                Stalemate();
                return;
            }
            List<GameObject> goodMoves = new List<GameObject>();
            foreach (GameObject move in legalMoves)
            {
                // See which way is center.
                int horizontalMove = (int)((boardWidth - 1) / 2 - xBall);
                if (horizontalMove != 0)
                {
                    horizontalMove = horizontalMove / Mathf.Abs(horizontalMove);    // Because Mathf.Sign would return +1 when given a 0.
                }
                // Check if there's a move that would bring you closer to the goal (lower center of the board).
                if (move.transform.position.y < yBall)
                {
                    if (move.transform.position.x == xBall + horizontalMove)
                    {
                        // Probably the best (naive) move to make.
                        dest = move;
                        break;
                    }
                    goodMoves.Add(move);
                }
                if (goodMoves.Count > 0)
                {
                    // Make a random "okay" move.
                    dest = goodMoves[(int)(Random.value * (goodMoves.Count-1))];
                }
                else
                {
                    // Just make any legal move.
                    dest = legalMoves[(int)(Random.value * (legalMoves.Count - 1))];
                }
            }
            if (legalMoves.Contains(dest))
            {
                // All good, make this move.
                Cell destCell = dest.GetComponent<Cell>();
                if (destCell.thisInterjection is Goal)
                {
                    ScoreGoal((destCell.thisInterjection as Goal).affiliation);
                    return;
                }
                // Create a new edge after this move.
                GameMove thisMove = new GameMove(boardView[yBall, xBall], dest);
                pastMoves.Add(thisMove);
                float edgeX = (thisMove.firstNode.transform.position.x + thisMove.secondNode.transform.position.x) * .5f;
                float edgeY = (thisMove.firstNode.transform.position.y + thisMove.secondNode.transform.position.y) * .5f;
                Vector3 edgePos = new Vector3(edgeX, edgeY, 0);
                pastMovesView.Add(
                    Instantiate(edgePrefab, edgePos, Quaternion.identity)
                );
                // Change the previous cell state to wall for logic purposes.
                (boardLogic[yBall, xBall] as Interjection).status = InterjectionStatus.WALL;
                // Move the ball to the destination.
                (boardLogic[yBall, xBall] as Interjection).hasBall = false;
                boardView[yBall, xBall].GetComponent<Cell>().spriteRenderer.color = Color.black;    // Wall color.
                (destCell.thisInterjection as Interjection).hasBall = true;
                destCell.spriteRenderer.color = Color.yellow;
                xBall = (int)dest.transform.position.x;
                yBall = (int)dest.transform.position.y;

                // End the turn on the same condition as player.
                if ((boardLogic[yBall, xBall] as Interjection).status == InterjectionStatus.FREE)
                {
                    currentPlayer = GoalAffiliation.PLAYER1;
                }
                else
                {
                    IEnumerator e()
                    {
                        yield return new WaitForSeconds(1);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            Debug.Log("Resetting the game...");
            ResetGame();
        }
    }

    // Reset game.
    public void ResetGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("The game was reset!");
    }
}
