using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera gameCam;

    private void Start()
    {
        gameCam.orthographicSize = GameManager.Instance.zoom;
    }
    private void Update()
    {
        gameCam.orthographicSize = GameManager.Instance.zoom;
    }

    private void LateUpdate()
    {
        try
        {
            transform.position = new Vector3(Player.Instance.transform.position.x, Player.Instance.transform.position.y, transform.position.z);
        }
        catch
        {
            transform.position = new Vector3(0, 0, transform.position.z);
        }
    }
}