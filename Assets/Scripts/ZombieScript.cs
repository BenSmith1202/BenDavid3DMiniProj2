using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    public float contactDamage;
    MonsterLogicScript monsterLogicScript;
    public Animator animator;
    CapsuleCollider myCollider;
    Rigidbody rb;
    public bool dead;
    public ParticleSystem deathParts;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        monsterLogicScript = GetComponent<MonsterLogicScript>();
        animator = monsterLogicScript.billboard.GetComponent<Animator>();
    }


    public int zombCode;

    // Update is called once per frame
    void Update()
    {
        if (!dead && monsterLogicScript.health < 0)
        {

            StartCoroutine(DeathCoroutine());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerControllerScript>() != null)
        {
            collision.gameObject.GetComponent<PlayerControllerScript>().TakeDamage(contactDamage);
        }
    }

    IEnumerator DeathCoroutine()
    {

        dead = true;

        GameObject.FindWithTag("WMan").GetComponent<WaveManager>().RegisterKill(gameObject, zombCode);



        ExplosionScript potentialExplosion = GetComponent<ExplosionScript>();
        if (potentialExplosion != null)
        {
            potentialExplosion.Explode();
        }


        animator.SetBool("dead", true);
        myCollider.enabled = false;
        rb.useGravity = false;
        GameObject deathEffect = Instantiate(deathParts, transform.position, monsterLogicScript.billboard.transform.rotation).gameObject;
        Destroy(deathEffect, 2);

        float fadeTime = 2;
        while (fadeTime > 0)
        {
            yield return new WaitForSeconds(0.02f);
            monsterLogicScript.bbsp.color = new Color(monsterLogicScript.bbsp.color.r, monsterLogicScript.bbsp.color.g, monsterLogicScript.bbsp.color.b, fadeTime / 2f);
            fadeTime -= 0.02f;
        }


    }

}
