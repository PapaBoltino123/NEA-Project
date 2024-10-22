using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Transform target;
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
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}