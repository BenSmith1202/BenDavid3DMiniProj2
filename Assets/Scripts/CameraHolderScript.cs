using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolderScript : MonoBehaviour
{
    public Transform targetPos;
    public float lockingForce = 5f;  
    public float smoothTime = 0.1f;

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        //using smooth damp for physics-based smoothing
        transform.position = Vector3.SmoothDamp(transform.position, targetPos.position, ref velocity, smoothTime);
    }

    void LateUpdate()
    {
        // lerp to smooth out jerkiness

        transform.position = Vector3.Lerp(transform.position, targetPos.position, lockingForce * Time.deltaTime);
    }
}
