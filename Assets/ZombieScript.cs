using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    public float speed;
    MonsterLogicScript monsterLogicScript;
    Animator animator;
    CapsuleCollider myCollider;
    Rigidbody rb;
    public bool dead;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        monsterLogicScript = GetComponent<MonsterLogicScript>();
        animator = monsterLogicScript.billboard.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead && monsterLogicScript.health < 0)
        {
            dead = true;
            animator.SetBool("dead", true);
            myCollider.enabled = false;
            rb.useGravity = false;
        }
    }

}
