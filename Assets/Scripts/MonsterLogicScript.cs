using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterLogicScript : MonoBehaviour
{
    public float maxHealth = 10f;
    public float health = 10f;

    public GameObject billboard;
    GameObject cam;
    public ParticleSystem hitParticles;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newRotation = cam.transform.eulerAngles;

        newRotation.x = 0;
        newRotation.z = 0;

        billboard.transform.eulerAngles = newRotation;
    }

    public void InflictHit(float damage, Vector3 hitlocation)
    {
        GameObject hitPart = Instantiate(hitParticles, hitlocation, transform.rotation).gameObject;
        Destroy(hitPart, 1f);
        billboard.GetComponent<SpriteRenderer>().color = new Color(1, health/maxHealth, health/maxHealth);
        if (health <= 0)
        {
            Debug.Log("Dead");
            billboard.GetComponent<SpriteRenderer>().color = new Color(1, 0.8f, 0.8f);
        }
        {
            health -= damage;
        }
    }


}
