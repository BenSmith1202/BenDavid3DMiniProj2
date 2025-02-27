using System.Collections;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public float radius = 5f;
    public float baseForce = 10f;
    public float forceScaling = 5f;
    public float forceOriginDepth = 0.5f;
    public float zombieDamage = 3f;
    public float playerDamage = 20f;
    public ParticleSystem explosionEffectPrefab;

    private bool hasExploded = false;


    public void ExplodeAfterDelay(float delay)
    {
        if (hasExploded) return; // Prevent multiple explosions
        StartCoroutine(ExplodeCoroutine(delay));
    }

    private IEnumerator ExplodeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    public void Explode()
    {
        if (hasExploded) return; // Prevent multiple explosions
        hasExploded = true;

        // particle effect
        GameObject explosionPart = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity).gameObject;
        Destroy(explosionPart, 0.8f);

        //TODO: Sound effect


        // loop through all colliders that get hit by this sphere
        Collider[] affected = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider c in affected)
        {
            Rigidbody rb = c.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //calculate force direction, with an  origin set into the ground for more upward bias
                Vector3 forceOrigin = transform.position - new Vector3(0, forceOriginDepth, 0);
                Vector3 forceDir = (c.transform.position - forceOrigin).normalized;

                //force magnitude based on distance
                float distance = Vector3.Distance(transform.position, c.transform.position);
                float forceMagnitude = baseForce + (forceScaling * (1 - (distance / radius)));

                rb.AddForce(forceDir * forceMagnitude, ForceMode.Impulse);
            }

            MonsterLogicScript monster = c.gameObject.GetComponent<MonsterLogicScript>();
            if (monster != null)
            {
                // Damage zombies
                monster.InflictHit(zombieDamage, c.gameObject.transform.position);

                //stunning
                ZombieMovementScript zms = c.gameObject.GetComponent<ZombieMovementScript>();
                ShielderMovementScript sms = c.gameObject.GetComponent<ShielderMovementScript>();
                if (zms != null)
                {
                    zms.Stun();
                }
                if (sms != null)
                {
                    sms.Stun();
                }
            }

            PlayerControllerScript player = c.gameObject.GetComponent<PlayerControllerScript>();
            if (player != null)
            {
                // Damage player
                player.TakeDamage(playerDamage);
            }

            ShieldScript shield = c.gameObject.GetComponent<ShieldScript>();
            if (shield != null)
            {
                // Damage player
                shield.TakeDamage(zombieDamage, c.gameObject.transform.position);
            }
        }

    }
}