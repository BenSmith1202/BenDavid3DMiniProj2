using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControllerScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed;
    float runSpeed;
    public float groundDrag;
    public float airDrag;
    public float airMultiplier;
    public float airSpeedCap;
    public MovementState movementState;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpDelay;
    bool readyToJump = true;

    [Header("Sprinting")]
    public GameObject staminaBar;
    UIBarScript stamBarScript;
    public float sprintSpeed;
    public bool sprinting;
    public float maxStamina;
    public float stamina;
    public float sprintCost;
    public float staminaRegen;

    [Header("Health")]
    public GameObject healthBar;
    UIBarScript healthBarScript;
    public float maxHealth;
    public float health;
    public float iframes;
    float iframesLeft = 0;
    public float healingDelay;
    float healingDelayLeft;
    bool invulnerable;
    bool regenerating;
    public float regenRate;

    [Header("Slope Movement")]
    public float maxSlopeAngle;
    RaycastHit slopeCast;
    public float slopeCling = 5;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded = false;

    [Header("References")]

    private Rigidbody _rbody;
    private CapsuleCollider _collider;
    private Vector2 moveInput;
    Vector3 moveDirection;
    public Transform orientation;
    public GameObject cam;
    CameraScript camScript;
    AudioSource audioSource;
    SpriteRenderer sp;


    [Header("Visuals")]
    public AudioClip gunshot;
    Animator animator;
    public ParticleSystem muzzleFlash;
    public ParticleSystem muzzleSmoke;
    public GameObject bulletTrailPrefab;
    public TMP_Text clipText;
    public TMP_Text ammoStockText;
    public Image reloadRing;

    [Header("Shooting")]
    public float shootCooldownTime;
    public float shootCooldown = 0;
    public int clipSize;
    public int clip;
    public int ammoStock;
    public float reloadTime;
    public bool isReloading;


    public enum MovementState
    {
        running,
        freefall,
    }

    void Start()
    {
        runSpeed = baseSpeed;
        healthBarScript = healthBar.GetComponent<UIBarScript>();
        stamBarScript = staminaBar.GetComponent<UIBarScript>();
        animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _rbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        camScript = cam.GetComponent<CameraScript>();
        sp = GetComponent<SpriteRenderer>();
        reloadRing.fillAmount = 0;

    }

    private void Update()
    {
        StateHandler();
        moveDirection = orientation.right * moveInput.x + orientation.forward * moveInput.y;
        if (iframes > 0)
        {
            invulnerable = true;
            iframes -= Time.deltaTime;
        } else
        {
            invulnerable = false;
        }

        if (healingDelay > 0)
        {
            regenerating = false;
            healingDelay -= Time.deltaTime;
        } else
        {
            regenerating = true;
        }
        if (regenerating && health < maxHealth)
        {
            ChangeHealth(regenRate * Time.deltaTime);
        }
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
        animator.SetFloat("speed", moveInput.magnitude);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && stamina > 1)
        {
            sprinting = true;
            animator.SetBool("sprinting", true);
            runSpeed = sprintSpeed;
        }
        else if (context.canceled)
        {
            sprinting = false;
            animator.SetBool("sprinting", false);
            runSpeed = baseSpeed;
        }
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

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed && !camScript.isShooting && !isReloading)
        {
            StartCoroutine(ReloadCoroutine(reloadTime));
        }
    }


    private void HandleCooldowns()
    {
        shootCooldown -= Time.deltaTime; // Not Yet Implemented
        
        if (sprinting && stamina > 0)
        {
            stamina -= sprintCost * Time.deltaTime;
            stamBarScript.SetSliderValue(stamina);
        } else if (stamina < maxStamina)
        {
            stamina += staminaRegen * Time.deltaTime;
            stamBarScript.SetSliderValue(stamina);
        }
        if (stamina < 1)
        {
            sprinting = false;
            animator.SetBool("sprinting", false);
            runSpeed = baseSpeed;
        }

    }

    // depending on what movment state the player is in, apply different movement
    //Activates every fixed update
    private void HandleMovement()
    {
        // Apply forces first
        if (OnSlopeCheck())
        {
            _rbody.AddForce(10f * runSpeed * GetSlopeDirection(), ForceMode.Force);
            _rbody.AddForce(slopeCling * -slopeCast.normal, ForceMode.Force);
            _rbody.useGravity = false;
        }
        else
        {
            // Movement force application based on current state
            Vector3 movementForce = 10f * moveDirection.normalized;
            switch (movementState)
            {
                case MovementState.running:
                    _rbody.AddForce(movementForce * runSpeed, ForceMode.Force);
                    break;
                case MovementState.freefall:
                    HandleFreefallMovement(movementForce);
                    break;
            }
            _rbody.useGravity = true;
        }

        // Always control speed regardless of slope or ground
        ControlMovementSpeed();
    }



    private void HandleFreefallMovement(Vector3 movementForce)
    {
        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > runSpeed)
        {
            _rbody.AddForce(RemovePositiveParallelComponent(0.8f * airMultiplier * runSpeed * movementForce, xzVel), ForceMode.Force);
        }
        else
        {
            _rbody.AddForce(0.8f * airMultiplier * runSpeed * movementForce, ForceMode.Force);
        }

    }


    private void ControlMovementSpeed()
    {

        Vector3 xzVel = new(_rbody.velocity.x, 0f, _rbody.velocity.z);

        if (xzVel.magnitude > runSpeed && movementState != MovementState.freefall)
        {
            Vector3 cappedVel = xzVel.normalized * runSpeed;
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
        if (grounded)
        {
            if (movementState != MovementState.running)
            {
                movementState = MovementState.running;
                
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

    public void ShootGun(Vector3 target)
    {

        muzzleFlash.Play();
        muzzleSmoke.Play();
        audioSource.PlayOneShot(gunshot);
        Vector3 bulletVector = target - muzzleFlash.transform.position;
        bulletVector = RemovePositiveParallelComponent(bulletVector, -transform.forward);
        target = muzzleFlash.transform.position + bulletVector;
        GameObject bTrail = Instantiate(bulletTrailPrefab, muzzleFlash.transform.position, Quaternion.identity);
        bTrail.GetComponent<BulletTrailScript>().StartTrail(bTrail.transform.position, target);
        shootCooldown = shootCooldownTime;
        ChangeClipAmmo(-1);
        Debug.Log("Clip: " + clip);
        if (clip <= 0)
        {
            StartCoroutine(ReloadCoroutine(reloadTime));
        }
    }

    public void ChangeAmmoStock(int deltaAmmo)
    {
        ammoStock += deltaAmmo;
        ammoStockText.SetText("" + ammoStock);
    }

    public void ChangeClipAmmo(int deltaAmmo)
    {
        clip += deltaAmmo;
        clipText.SetText("" + clip);
    }

    IEnumerator ReloadCoroutine(float duration)
    {
        if (clip < clipSize)
        {
            isReloading = true;
            if (ammoStock == 0)
            {
                Debug.Log("Out of Ammo");
            }
            else
            {
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    reloadRing.fillAmount = elapsedTime / duration;
                    yield return new WaitForEndOfFrame();
                    elapsedTime += Time.deltaTime;
                }

                int amountToLoad = Mathf.Min(clipSize - clip, ammoStock);
                ChangeClipAmmo(amountToLoad);
                ChangeAmmoStock(-amountToLoad);
                Debug.Log("Loaded " + amountToLoad + " rounds.");
                Debug.Log("Clip: " + clip);
                Debug.Log("Ammo Supply: " + ammoStock);

            }
            isReloading = false;
            reloadRing.fillAmount = 0;
        }
    }


    public void TakeDamage(float damage)
    {
        if (!invulnerable)
        {
            //particles and sound maybe;
            ChangeHealth(-damage);
            healingDelayLeft = healingDelay;
            iframesLeft = iframes;
        }
        
    }
    public void ChangeHealth(float deltaHealth)
    {
        health += deltaHealth;
        healthBarScript.SetSliderValue(health);
        if (health < 0.6 * maxHealth)
        {
            sp.color = new Color(1, health / (0.6f * maxHealth), health / (0.6f * maxHealth));
        }
    }

}