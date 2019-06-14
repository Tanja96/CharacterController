using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Rigidbody rigi;
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 target;
    private Vector3 lastPos;
    private FrogMovement child = null;

    public float speed;
    public Vector3 moveVector;
    public Vector3 velocity;

    void Start()
    {
        rigi = GetComponent<Rigidbody>();
        startPos = transform.position;
        endPos = startPos + moveVector;
        target = endPos;
        lastPos = transform.position;
    }
    void FixedUpdate()
    {
        rigi.MovePosition(Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime));
        if(Vector3.Distance(transform.position, target) < 0.1f)
        {
            if(target == startPos)
            {
                target = endPos;
            }
            else
            {
                target = startPos;
            }
        }

        velocity = transform.position - lastPos;

        lastPos = transform.position;

        if(child != null)
        {
            child.GiveParentVelocity(velocity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            child = other.GetComponent<FrogMovement>();
            rigi.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            child.GiveParentVelocity(new Vector3(0, 0, 0));
            child = null;
            rigi.interpolation = RigidbodyInterpolation.None;
        }
    }
}
