using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShielderMovementScript : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float calculationFrequency;
    bool canMove = true;
    Rigidbody rb;
    WaveManager waveManager;
    Vector3 target;
    ZombieScript zs;
    public GameObject shield;
    public GameObject shieldPrefab;
    bool regeneratingShield;
    public float shieldRechargeTime = 10f;
    MonsterLogicScript monsterLogicScript;
    // Start is called before the first frame update
    void Start()
    {
        zs = GetComponent<ZombieScript>();
        target = transform.position;
        waveManager = GameObject.FindWithTag("WMan").GetComponent<WaveManager>();
        rb = GetComponent<Rigidbody>();
        shield = Instantiate(shieldPrefab, transform);
        monsterLogicScript = GetComponent<MonsterLogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!zs.dead && canMove)
        {
            //Shield Zombies target the center of the horde
            // the horde centroid is updated every half a second
            target = waveManager.hordeCentroid;

            Vector3 movedir = (target - transform.position).normalized;
            rb.AddForce(new Vector3(movedir.x, 0, movedir.z) * acceleration, ForceMode.Force);
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }

            //if shield broken for  long enough, generate a new shield.
            if (shield == null && !regeneratingShield)
            {
                StartCoroutine(RechargeShield(shieldRechargeTime));
            }

        } else
        {
            if (zs.dead)
            {
                rb.velocity = Vector3.zero;

                if (shield != null) //break shield
                {
                    shield.GetComponent<ShieldScript>().BreakShield();
                }
                //dont make a new shield
                regeneratingShield = true;

                //corpses dont move
                rb.velocity = Vector3.zero;
            }

            
        }

        if (rb.velocity.magnitude > 0.1f)
        {
            zs.animator.SetFloat("speed", 1f);
        }
        else
        {
            zs.animator.SetFloat("speed", 0f);
        }
    }

    IEnumerator RechargeShield(float delay)
    {
        regeneratingShield = true;
        yield return new WaitForSeconds(delay);
        if (!zs.dead)
        {
            shield = Instantiate(shieldPrefab, transform);
        }
        regeneratingShield = false;
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
