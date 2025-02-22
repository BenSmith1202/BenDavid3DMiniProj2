using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed;
    public float groundDrag;
    public float airDrag;
    public float airMultiplier;
    public float airSpeedCap;
    float speed = 5f;
    public MovementState movementState;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpDelay;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float startYScale;
    public float crouchYScale;
    public float crouchForce;

    [Header("Slope Movement")]
    public float maxSlopeAngle;
    RaycastHit slopeCast;
    public float slopeCling = 5;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded = false;

    [Header("References")]
    public float shootCooldownTime;
    public float shootCooldown = 0;
    private Rigidbody _rbody;
    private CapsuleCollider _collider;
    private Vector2 moveInput;
    private bool isCrouching;
    Vector3 moveDirection;
    public Transform orientation;
    public GameObject cam;
    AudioSource audioSource;

    public enum MovementState
    {
        running,
        sprinting,
        crouching,
        freefall,
    }

    void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
        _rbody = GetComponent<Rigidbody>();
        startYScale = transform.localScale.y;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        StateHandler();
        moveDirection = orientation.right * moveInput.x + orientation.forward * moveInput.y;
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        Debug.DrawLine(transform.position, transform.position - new Vector3(0, playerHeight * 0.5f + 0.2f, 0), Color.red, 0.01f);

        if (grounded)
        {
            _rbody.drag = groundDrag;
        }
        else
        {
            _rbody.drag = airDrag;
        }

        HandleMovement();
        HandleCooldowns();
    }

    // New Input System callbacks using Unity Events
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && readyToJump && grounded)
        {
            PlayerJump();
            readyToJump = false;
            Invoke(nameof(ResetJump), jumpDelay);
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = true;
        }
        else if (context.canceled)
        {
            isCrouching = false;
        }
    }

    private void HandleCooldowns()
    {
        shootCooldown -= Time.deltaTime; // Not Yet Implemented

    }

    // depending on what movment state the player is in, apply different movement
    //Activates every fixed update
    private void HandleMovement()
    {
        // PRIORITY 1: Slope Movement - Special physics handling
        if (OnSlopeCheck())
        {
            _rbody.AddForce(10f * speed * GetSlopeDirection(), ForceMode.Force);
            _rbody.AddForce(slopeCling * -slopeCast.normal, ForceMode.Force);
            _rbody.useGravity = false;
            return;
        }

        // Movement force application based on current state
        Vector3 movementForce = 10f * moveDirection.normalized;
        switch (movementState)
        {
            case MovementState.crouching:
                _rbody.AddForce(movementForce * crouchSpeed, ForceMode.Force);
                break;

            case MovementState.running:
                _rbody.AddForce(movementForce * speed, ForceMode.Force);
                break;

            case MovementState.freefall:
                HandleFreefallMovement(movementForce);
                break;
        }

        // Speed control
        ControlMovementSpeed();

        // Restore gravity when not on slope
        _rbody.useGravity = true;
    }



    private void HandleFreefallMovement(Vector3 movementForce)
    {
        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > speed)
        {
            _rbody.AddForce(RemovePositiveParallelComponent(0.8f * airMultiplier * speed * movementForce, xzVel), ForceMode.Force);
        }
        else
        {
            _rbody.AddForce(0.8f * airMultiplier * speed * movementForce, ForceMode.Force);
        }

    }


    private void ControlMovementSpeed()
    {

        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > speed && movementState != MovementState.freefall)
        {
            Vector3 cappedVel = xzVel.normalized * speed;
            _rbody.velocity = new Vector3(cappedVel.x, _rbody.velocity.y, cappedVel.z);
        }
    }
    private void PlayerJump()
    {
        _rbody.velocity = new Vector3(_rbody.velocity.x, 0, _rbody.velocity.z); // Reset vertical velocity before jumping
        _rbody.AddForce(orientation.up * jumpForce, ForceMode.Impulse);           // Apply jump force
    }

    void ResetJump()
    {
        readyToJump = true; // Allow the player to jump again
    }


    private bool OnSlopeCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeCast, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeCast.normal); // Calculate the slope angle relative to the upward direction
            return angle < maxSlopeAngle && angle > 2f;                // Check if the slope is walkable
        }
        return false;
    }


    private Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeCast.normal).normalized; // Project movement direction onto the slope's plane
    }


    //UTILITIES


    Vector3 RemovePositiveParallelComponent(Vector3 vectorIn, Vector3 referenceVector)
    /**Takes a vector input and a reference vector, and returns the input vector minus any positive 
     ** parallel component to the reference vector.**/
    {
        // normalize the reference direction into a unit vector
        Vector3 normalizedReference = referenceVector.normalized;

        // project vectorToModify onto the reference direction
        // this gives us the component of vectorToModify that is parallel to the reference
        Vector3 parallelComponent = Vector3.Dot(vectorIn, normalizedReference) * normalizedReference;

        // figure out if the parallel component is in the same or opposite direction as the reference
        float parallelSign = Mathf.Sign(Vector3.Dot(parallelComponent, referenceVector));


        if (parallelSign > 0) //if there is a positive component parallel to the reference
        {
            return vectorIn - parallelComponent; //remove it
        }
        else //otherwise
        {
            return vectorIn; //return original
        }
    }

    private void StateHandler()
    {
        if (isCrouching && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (movementState != MovementState.crouching)
            {
                _rbody.AddForce(new Vector3(0, -crouchForce, 0), ForceMode.Impulse);
            }
            movementState = MovementState.crouching;
            speed = crouchSpeed;
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            if (grounded)
            {
                if (movementState != MovementState.running)
                {
                    movementState = MovementState.running;
                    speed = runSpeed;
                }
            }
            else
            {
                if (movementState != MovementState.freefall)
                {
                    movementState = MovementState.freefall;
                }
            }
        }
    }
}