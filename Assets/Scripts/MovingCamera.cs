using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCamera : MonoBehaviour
{
    Transform target;
    Vector3 lastPos;
    private void Start()
    {
        target = FindObjectOfType<CustomController>().transform;
    }
    private void Update()
    {
        transform.position += target.position - lastPos;
        lastPos = target.position;
    }
}
