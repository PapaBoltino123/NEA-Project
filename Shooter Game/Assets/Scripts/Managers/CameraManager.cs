using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera gameCam;

    private void Start()
    {
        gameCam.orthographicSize = GameManager.Instance.zoom; //the camera zoom is immediately equal to the zoom changed by the slider in the menu
    }
    private void Update()
    {
        gameCam.orthographicSize = GameManager.Instance.zoom; //the camera zoom is always equal to the zoom changed by the slider in the menu
    }

    private void LateUpdate()
    {
        try
        {
            transform.position = new Vector3(Player.Instance.transform.position.x, Player.Instance.transform.position.y, transform.position.z); //the camera position is constantly equal to the player position
        }
        catch
        {
            transform.position = new Vector3(0, 0, transform.position.z); //if player position cannot be accessed, set the camera position to 0, 0
        }
    }
}