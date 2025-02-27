using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveZombieScript : MonoBehaviour
{
    ExplosionScript explosion;
    ZombieScript zs;
    GameObject player;
    public float detonationTriggerRadius = 3f;
    public float detonationDelay = 1f;
    void Start()
    {
        zs = GetComponent<ZombieScript>();
        player = GameObject.FindWithTag("Player");
        explosion = GetComponent<ExplosionScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((player.transform.position - transform.position).magnitude < detonationTriggerRadius)
        {
            explosion.Explode();
        }
        if (zs.dead)
        {
            explosion.Explode();
        }
    }
}
