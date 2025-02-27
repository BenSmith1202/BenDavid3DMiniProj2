using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMovementScript : MonoBehaviour
{

    public float acceleration;
    public float maxSpeed;
    GameObject player;
    bool canMove = true;
    Rigidbody rb;
    Vector3 target;
    ZombieScript zs;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        zs = GetComponent<ZombieScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!zs.dead && canMove)
        {
            //most zombies target the player
            target = player.transform.position;
            Vector3 movedir = (target - transform.position).normalized;
            rb.AddForce(new Vector3(movedir.x, 0, movedir.z) * acceleration, ForceMode.Force);
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.normalized.x, 0, rb.velocity.normalized.z) * maxSpeed + new Vector3(0, rb.velocity.y, 0);
            }
        } else
        {
            if (zs.dead)
            {
                rb.velocity = Vector3.zero;
            }
            
        }
        
        if (rb.velocity.magnitude > 0.1f)
        {
            zs.animator.SetFloat("speed", 1f);
        } else
        {
            zs.animator.SetFloat("speed", 0f);
        }
    }

    public void Stun()
    {
        canMove = false;
        Invoke("UnStun", 1.5f);
    }

    void UnStun()
    {
        canMove = true;
    }
}
