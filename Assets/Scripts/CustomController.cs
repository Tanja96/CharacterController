﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomController : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    private bool jumpPressed;
    private bool isGrounded;
    private Vector3 groundNormal;
    private float acceptedDot;
    private RaycastHit[] hits;
    private Transform cameraObject;
    private Collider[] results;
    private Rigidbody rigi;
    private Collider ownCollider;
    private Vector3 velocity;
    private Vector3 targetPos;
    private Vector3 parentMovement;
    private float lastDrag;
    private int jumpCount;
    
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
    public float maxGroundAngleSpeed = 2f;
    public float groundSphereRadius = 0.49f;
    public float groundDist = 0.06f;
    public bool moveAlongGround = true;
    public LayerMask ground;
    public LayerMask collidable;
    public Transform groundChecker;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        results = new Collider[5];
        hits = new RaycastHit[5];
        cameraObject = Camera.main.transform;
        acceptedDot = Vector3.Dot(new Vector3(0, 1, 0), new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * maxGroundAngle), Mathf.Sin(Mathf.Deg2Rad * maxGroundAngle)));
    }

    private void FixedUpdate()
    {
        GroundCheck();

        //Movement from inputs
        Vector3 forward = new Vector3(cameraObject.forward.x, 0, cameraObject.forward.z).normalized;
        Vector3 right = new Vector3(cameraObject.right.x, 0, cameraObject.right.z).normalized;
        Vector3 direction = right * horizontal + forward * vertical;
        Vector3 movement = direction.normalized * Mathf.Min(direction.magnitude, 1) * Time.deltaTime * speed;

        //Rotation
        if(movement != Vector3.zero)
        {
            rigi.MoveRotation(Quaternion.LookRotation(movement));
        }

        //Aligning movement with ground
        if (moveAlongGround && groundNormal != new Vector3(0, 0, 0))
        {
            Vector3 cross = Vector3.Cross(transform.right, groundNormal);
            direction = Vector3.RotateTowards(direction, cross, float.MaxValue, float.MaxValue).normalized * direction.magnitude;
            if (cross.y > 0.001f)
            {
                float lerpValue = (Vector3.Dot(groundNormal, new Vector3(0, 1, 0)) - acceptedDot) / acceptedDot;
                movement = direction.normalized * Mathf.Min(direction.magnitude, 1) * Time.deltaTime * Mathf.Lerp(maxGroundAngleSpeed, speed, lerpValue);
            }
            else
            {
                movement = direction.normalized * Mathf.Min(direction.magnitude, 1) * Time.deltaTime * speed;
            }
        }

        //Adding gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        if (isGrounded && velocity.y < 0)
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

        //Test move -> Collision check -> Real move
        targetPos = transform.position + movement + velocity * Time.deltaTime + parentMovement;
        ComputePenetration();
        rigi.MovePosition(targetPos);

        //Adding drag
        velocity.x *= 1 - drag.x * Time.deltaTime;
        velocity.z *= 1 - drag.z * Time.deltaTime;
    }

    /// <summary>
    /// Prevents passing through objects
    /// </summary>
    private void ComputePenetration()
    {
        //Get all colliders close by
        int numOverlaps = Physics.OverlapBoxNonAlloc(targetPos, ownCollider.bounds.extents, results, rigi.rotation, collidable, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numOverlaps; i++)
        {
            //Checks how much the controller should move so it doesn't go inside objects.
            if (Physics.ComputePenetration(ownCollider, targetPos, transform.rotation, results[i], results[i].transform.position, results[i].transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 penetrationVector = direction * distance;
                if (velocity.y > 0 && penetrationVector.y < -0.001) //when colliding with roofs
                {
                    velocity.y = 0;
                }
                else if (penetrationVector.y < -0.001 && isGrounded) //when object is pushed under ground
                {
                    float amount = penetrationVector.y;
                    penetrationVector.y = 0;
                    penetrationVector -= penetrationVector.normalized * amount;
                }
                targetPos += penetrationVector;
            }
        }
    }

    /// <summary>
    /// Uses a sphereCast to determine if controller is grounded
    /// </summary>
    private void GroundCheck()
    {
        int amount = Physics.SphereCastNonAlloc(groundChecker.position, groundSphereRadius, new Vector3(0, -1, 0), hits, groundDist, ground, QueryTriggerInteraction.Ignore);
        isGrounded = false;
        groundNormal = new Vector3(0, 0, 0);
        if (amount > 0)
        {
            for(int i = 0; i < amount; i++)
            {
                //Checking if surface angle is accepted as ground
                if(Vector3.Dot(hits[i].normal, new Vector3(0, 1, 0)) > acceptedDot)
                {
                    isGrounded = true;
                    groundNormal = hits[i].normal;
                    return;
                }
            }
        }
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
}
