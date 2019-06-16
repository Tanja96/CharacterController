using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    CustomController movement;
    private void Start()
    {
        movement = FindObjectOfType<CustomController>();
    }

    private void Update()
    {
        movement.SetMovementAxis(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            movement.SetJump(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            movement.SetJump(false);
        }
        if (Input.GetButtonDown("Dash"))
        {
            movement.SetDash(true);
        }
        else if (Input.GetButtonUp("Dash"))
        {
            movement.SetDash(false);
        }
    }
}
