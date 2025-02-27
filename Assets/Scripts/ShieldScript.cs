using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        health = 0;
        StartCoroutine(Deploy());
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

    public void BreakShield()
    {
        GameObject breakParts = Instantiate(breakParticles, transform.position, Quaternion.identity).gameObject;
        breakParts.transform.localScale = transform.localScale;
        sc.enabled = false;
        mr.enabled = false;
        Destroy(breakParts, 1);
        Destroy(gameObject, 1.5f);
        
        //particle
    }

    IEnumerator Deploy()
    {
        health = 0;
        transform.localScale = Vector3.one;
        float deployTime = 2f;
        while (deployTime > 0)
        {
            yield return new WaitForSeconds(0.02f);
            deployTime -= 0.02f;
            health += maxhealth / 100;
            transform.localScale = Vector3.one * (minSize + (health / maxhealth * maxSize));
        }

    }


}
