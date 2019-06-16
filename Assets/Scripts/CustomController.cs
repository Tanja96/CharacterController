using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    private bool jumpPressed;
    private bool isGrounded;
    private bool hittingRoof;
    private Transform cameraObject;
    private Collider[] results;
    private List<bool> groundCollidersValues;
    private float acceptedDot;
    private Rigidbody rigi;
    private Collider ownCollider;
    private Vector3 velocity;
    private Vector3 targetPos;
    private Vector3 parentMovement;
    private float lastDrag;
    private int jumpCount;
    private bool targetting;

    public float speed = 5f;
    public int numberOfJumps = 1;
    public float jumpHeight = 1.5f;
    public float lowJumpMultiplier = 1.75f;
    public float fallMultiplier = 1.5f;
    public float dashDistance = 5f;
    public float dashCoolDown = 0.4f;
    public Vector3 drag = new Vector3(5f, 0f, 5f);
    public float groundDistance = 0.49f;
    public float maxGroundAngle = 60f;
    public LayerMask ground;
    public Transform groundChecker;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        results = new Collider[5];
        cameraObject = Camera.main.transform;
        groundCollidersValues = new List<bool>();
        acceptedDot = Vector3.Dot(new Vector3(0, 1, 0), new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * maxGroundAngle), Mathf.Sin(Mathf.Deg2Rad * maxGroundAngle)));
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
            if (targetting)
            {
                rigi.MoveRotation(Quaternion.LookRotation(forward, transform.up));
            }
            else
            {
                rigi.MoveRotation(Quaternion.LookRotation(movement));
            }
        }

        //Adding gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }
        else if (velocity.y < 0)
        {
            velocity.y += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (velocity.y > 0 && !jumpPressed)
        {
            velocity.y += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //Test move -> Roof check -> Collision check -> Real move
        targetPos = transform.position + movement + velocity * Time.deltaTime + parentMovement;
        
        CheckRoof();
        if(!hittingRoof)
        {
            ComputePenetration();
            rigi.MovePosition(targetPos);
        }

        //adding drag
        velocity.x *= 1 - drag.x * Time.deltaTime;
        velocity.z *= 1 - drag.z * Time.deltaTime;
    }

    /// <summary>
    /// Prevents passing through objects
    /// </summary>
    private void ComputePenetration()
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

    /// <summary>
    /// Checks for roofs
    /// </summary>
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

    /// <summary>
    /// Grounded true if one of the colliders hitting the trigger is accepted as ground
    /// </summary>
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

    /// <summary>
    /// Adds bool to groundColliderValues according to ground angle.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.Raycast(new Ray(groundChecker.position, (other.transform.position - groundChecker.position).normalized), out RaycastHit hit, 1))
        {
            if (Vector3.Dot(hit.normal, new Vector3(0, 1, 0)) <= acceptedDot)
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
        if(isGrounded)
        {
            jumpCount = 0;
        }
        jumpPressed = press;
        if (jumpPressed && jumpCount < numberOfJumps)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            jumpCount++;
        }
    }

    public void SetDash(bool press)
    {
        if(press && Time.time - lastDrag > dashCoolDown)
        {
            velocity += Vector3.Scale(transform.forward, dashDistance * new Vector3(drag.x, 0, drag.z));
            lastDrag = Time.time;
        }
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

    public void SetTargetting(bool value)
    {
        targetting = value;
    }
}
