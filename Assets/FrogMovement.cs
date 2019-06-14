using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogMovement : MonoBehaviour
{
    private Vector2 move;
    private bool jumpPressed;
    private Rigidbody rigi;
    private Collider[] results;
    private Collider ownCollider;
    private Vector3 velocity;
    private Vector3 targetPos;
    public bool isGrounded;
    private Vector3 parentMovement;
    private bool hittingRoof;
    private Transform cameraObject;
    private Vector3 lastPos;

    public float speed;
    public float jumpHeight;
    public LayerMask layerMask;
    public LayerMask ground;
    public Transform groundChecker;
    public float groundDistance;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        results = new Collider[5];
        cameraObject = Camera.main.transform;
        lastPos = transform.position;
        Debug.Log(ownCollider.bounds.extents);
    }

    void ComputePenetration()
    {
        int numOverlaps = Physics.OverlapBoxNonAlloc(targetPos, ownCollider.bounds.extents, results, rigi.rotation, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numOverlaps; i++)
        {
            if (Physics.ComputePenetration(ownCollider, targetPos, transform.rotation, results[i], results[i].transform.position, results[i].transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 penetrationVector = direction * distance;
                targetPos += penetrationVector;
                Debug.Log(i + ": " + results[i].transform.name + " " + penetrationVector);
            }
        }
    }

    private void CheckRoof()
    {
        if (Physics.Raycast(targetPos, Vector3.up, out RaycastHit hit, 1.15f, ground, QueryTriggerInteraction.Ignore))
        {
            hittingRoof = true;
            rigi.MovePosition(transform.position + Vector3.Project(targetPos - transform.position, Vector3.Cross(hit.normal, Vector3.up)));
        }
        else
        {
            hittingRoof = false;
        }
    }


    private void FixedUpdate()
    {
        //Movement from inputs
        Vector3 forward = new Vector3(cameraObject.forward.x, 0, cameraObject.forward.z).normalized;
        Vector3 right = new Vector3(cameraObject.right.x, 0, cameraObject.right.z).normalized;
        Vector3 direction = right * move.x + forward * move.y;
        Vector3 movement = direction.normalized * Mathf.Min(direction.magnitude, 1) * Time.deltaTime * speed;
        if(movement != Vector3.zero)
        {
            rigi.MoveRotation(Quaternion.LookRotation(movement));
        }

        //Gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }
        else if(velocity.y < 0)
        {
            velocity.y *= 1.05f;
        }

        if(jumpPressed && isGrounded)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            jumpPressed = false;
        }

        //Test move -> Check collision -> Real move
        targetPos = transform.position + movement + velocity * Time.deltaTime + parentMovement;
        ComputePenetration();
        CheckRoof();
        if(hittingRoof)
        {

        }
        else
        {
            rigi.MovePosition(targetPos);
        }
        //Debug.Log((transform.position - lastPos).magnitude);
        lastPos = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        isGrounded = true;
        if (other.Raycast(new Ray(groundChecker.position, (other.transform.position - groundChecker.position).normalized), out RaycastHit hit, 1))
        {
            if (Vector3.Dot(hit.normal, new Vector3(0, 1, 0)) <= 0.5f)
            {
                isGrounded = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isGrounded = false;
    }

    public void SetJump(bool press)
    {
        jumpPressed = press;
    }

    public void SetDash(bool press)
    {

    }

    public void SetMovementAxis(float h, float v)
    {
        move = new Vector2(h, v);
    }

    public void GiveParentVelocity(Vector3 v)
    {
        parentMovement = v;
    }
}
