using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Cell prefab.
    public GameObject cellPrefab;
    // Board size.
    const int boardWidth = 9;
    const int boardHeight = 13;
    // The board logic (line interjections) and view (sprites).
    InterjectionAbstract[,] boardLogic = new InterjectionAbstract[boardHeight, boardWidth];
    GameObject[,] boardView = new GameObject[boardHeight, boardWidth];

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
                return new Interjection(InterjectionStatus.WALL);
            }
        }
        // Almost top or bottom.
        if (i == 1 || i == boardHeight - 2)
        {
            // Put an empty space in the middle, otherwise put wall.
            if (j == (boardWidth - 1) / 2)
            {
                return new Interjection(InterjectionStatus.FREE);
            }
            else
            {
                return new Interjection(InterjectionStatus.WALL);
            }
        }
        // The large, middle part of the board.
        else
        {
            if(j == 0 || j == boardWidth - 1)
            {
                return new Interjection(InterjectionStatus.WALL);
            }
            else
            {
                return new Interjection(InterjectionStatus.FREE);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
