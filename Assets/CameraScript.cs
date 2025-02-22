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
    public void OnLook(InputAction.CallbackContext context)
    {
        mouseX = context.ReadValue<Vector2>().x;
        mouseY = context.ReadValue<Vector2>().y;

        print(mouseX);
        print(mouseY);

    }


}