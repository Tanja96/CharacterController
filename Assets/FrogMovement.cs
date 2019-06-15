using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogMovement : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    private bool jumpPressed;
    private bool dashPresssed;
    public bool isGrounded;
    private bool hittingRoof;
    private Transform cameraObject;
    private Collider[] results;
    private List<bool> groundCollidersValues;
    private Rigidbody rigi;
    private Collider ownCollider;
    private Vector3 velocity;
    private Vector3 targetPos;
    private Vector3 parentMovement;

    public float speed;
    public float jumpHeight;
    public LayerMask ground;
    public Transform groundChecker;
    public float groundDistance;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        results = new Collider[5];
        cameraObject = Camera.main.transform;
        groundCollidersValues = new List<bool>();
    }

    void ComputePenetration()
    {
        int numOverlaps = Physics.OverlapBoxNonAlloc(targetPos, ownCollider.bounds.extents, results, rigi.rotation, ground, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numOverlaps; i++)
        {
            if (Physics.ComputePenetration(ownCollider, targetPos, transform.rotation, results[i], results[i].transform.position, results[i].transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 penetrationVector = direction * distance;
                targetPos += penetrationVector;
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
        CheckGrounded();

        //Movement from inputs
        Vector3 forward = new Vector3(cameraObject.forward.x, 0, cameraObject.forward.z).normalized;
        Vector3 right = new Vector3(cameraObject.right.x, 0, cameraObject.right.z).normalized;
        Vector3 direction = right * horizontal + forward * vertical;
        Vector3 movement = direction.normalized * Mathf.Min(direction.magnitude, 1) * Time.deltaTime * speed;
        if(movement != Vector3.zero)
        {
            rigi.MoveRotation(Quaternion.LookRotation(movement));
        }

        //Adding gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }
        else if(velocity.y < 0)
        {
            velocity.y *= 1.05f;
        }

        //jumping
        if(jumpPressed && isGrounded)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            jumpPressed = false;
        }

        //dash
        if(dashPresssed)
        {
            
        }

        //Test move -> Roof check -> Collision check -> Real move
        targetPos = transform.position + movement + velocity * Time.deltaTime + parentMovement;
        
        CheckRoof();
        if(!hittingRoof)
        {
            ComputePenetration();
            rigi.MovePosition(targetPos);
        }
    }

    private void CheckGrounded()
    {
        foreach(bool value in groundCollidersValues)
        {
            if(value)
            {
                isGrounded = true;
                break;
            }
            else
            {
                isGrounded = false;
            }
        }
        groundCollidersValues.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.Raycast(new Ray(groundChecker.position, (other.transform.position - groundChecker.position).normalized), out RaycastHit hit, 1))
        {
            if (Vector3.Dot(hit.normal, new Vector3(0, 1, 0)) <= 0.5f)
            {
                groundCollidersValues.Add(false);
                return;
            }
        }
        groundCollidersValues.Add(true);
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
        dashPresssed = press;
    }

    public void SetMovementAxis(float h, float v)
    {
        horizontal = h;
        vertical = v;
    }

    public void GiveParentVelocity(Vector3 v)
    {
        parentMovement = v;
    }
}
