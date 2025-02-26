using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShieldScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float health;
    public float maxhealth;
    public float minSize;
    public float maxSize;
    public ParticleSystem breakParticles;
    public ParticleSystem hitParticles;
    SphereCollider sc;
    MeshRenderer mr;
    void Start()
    {
        sc = GetComponent<SphereCollider>();
        mr = GetComponent<MeshRenderer>();

        transform.localScale = Vector3.one * maxSize;
    }

    public void TakeDamage(float damage, Vector3 target)
    {
        GameObject hitparts = Instantiate(hitParticles, target, Quaternion.identity).gameObject;
        Destroy(hitparts, 2);
        health -= damage;
        transform.localScale = Vector3.one * (minSize + (health/maxhealth * maxSize));
        if (health <= 0)
        {
            BreakShield();
        }
    }

    void BreakShield()
    {
        GameObject breakParts = Instantiate(breakParticles, transform.position, Quaternion.identity).gameObject;
        breakParts.transform.localScale = transform.localScale;
        sc.enabled = false;
        mr.enabled = false;
        Destroy(breakParts, 1);
        Destroy(gameObject, 1.5f);
        
        //particle
    }
}
