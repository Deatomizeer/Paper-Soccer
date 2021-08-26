using UnityEngine;

public class Cell : MonoBehaviour
{
    // The logic behind this cell.
    public InterjectionAbstract thisInterjection;
    // This cell's sprite renderer.
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        // Walls are black, blanks are white.
        if(thisInterjection is Interjection)
        {
            if((thisInterjection as Interjection).status == InterjectionStatus.WALL)
            {
                spriteRenderer.color = Color.black;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
        // Player1 is red, Player2 is blue.
        if(thisInterjection is Goal)
        {
            Color goalColor = ((thisInterjection as Goal).affiliation == GoalAffiliation.PLAYER1 ? Color.red : Color.blue);
            spriteRenderer.color = goalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
