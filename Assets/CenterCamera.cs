using UnityEngine;

public class CenterCamera : MonoBehaviour
{
    public void CenterCameraOnBoard(int width, int height)
    {
        transform.position = new Vector3(width / 2, height / 2, -1);
        // Setting width much bigger than height may leave some parts of the board out of sight.
        Camera.main.orthographicSize = height / 2f;
    }
}
