using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class CameraScript : MonoBehaviour
{
    public GameObject player;
    PlayerControllerScript playerControllerScript;
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    public float mouseX;
    public float mouseY;
    float yAngle;
    float xAngle;
    public Transform orientation;

    public float gunRange = 100f;
    public bool isShooting;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerControllerScript = player.GetComponent<PlayerControllerScript>();

    }
    private void Update()
    {
        // Apply mouse movement directly without deltaTime
        yAngle -= mouseY * sensitivityY; // Negative because mouse Y moves camera y in opposite direction
        xAngle += mouseX * sensitivityX;
        // Clamp the vertical rotation
        yAngle = Mathf.Clamp(yAngle, -90f, 90f);
        // Apply rotations
        player.transform.rotation = Quaternion.Euler(0, xAngle, 0);
        orientation.transform.rotation = Quaternion.Euler(0, xAngle, 0);
        transform.rotation = orientation.transform.rotation * Quaternion.Euler(yAngle, 0, 0);


    }

    private void FixedUpdate()
    {
        if (isShooting)
        {
            PullTrigger();
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        mouseX = context.ReadValue<Vector2>().x;
        mouseY = context.ReadValue<Vector2>().y;

    }

    public void PullTrigger()
    {
        if (playerControllerScript.shootCooldown < 0 && !playerControllerScript.isReloading && playerControllerScript.clip > 0)
        {
            Ray ray = new Ray(transform.position, transform.forward * gunRange);
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData);
            Vector3 target = hitData.point;
            if (target == Vector3.zero)
            {
                target = transform.forward * gunRange;
            }
            else
            {
                if (hitData.collider.gameObject.GetComponent<MonsterLogicScript>() != null)
                {
                    hitData.collider.gameObject.GetComponent<MonsterLogicScript>().InflictHit(1, target);
                }
            }

            playerControllerScript.ShootGun(target);
            playerControllerScript.shootCooldown = playerControllerScript.shootCooldownTime;
        }
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isShooting = true;
        } else if (context.canceled)
        {
            isShooting = false;
        }
    }


}