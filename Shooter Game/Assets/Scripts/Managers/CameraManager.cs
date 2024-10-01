using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Manager
{
    [SerializeField] Transform target;

    private void LateUpdate()
    {
        RunManager();
    }
    public override void RunManager()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}
