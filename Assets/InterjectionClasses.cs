// Whether the cell has been passed through.
public enum InterjectionStatus
{
    FREE, WALL
}
// Marks the goal's owner.
public enum GoalAffiliation
{
    PLAYER1, PLAYER2
}
public abstract class InterjectionAbstract
{

}
// A regular cell, through which a player can move the ball.
public class Interjection : InterjectionAbstract
{
    public InterjectionStatus status;
    public bool hasBall;
    public Interjection(InterjectionStatus status, bool hasBall)
    {
        this.status = status;
        this.hasBall = hasBall;
    }
}

// One of two goals on the opposite sides of the board.
public class Goal : InterjectionAbstract
{
    public Goal(GoalAffiliation affiliation)
    {
        this.affiliation = affiliation;
    }
    public GoalAffiliation affiliation;
}
