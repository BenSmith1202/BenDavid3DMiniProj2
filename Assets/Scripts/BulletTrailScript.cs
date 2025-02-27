using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrailScript : MonoBehaviour
{
    Vector3 start;
    Vector3 end;
    Vector3 direction;
    public float speed;

    public void StartTrail(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
        transform.position = start;
        direction = (end - start);
        StartCoroutine(BulletTrailCoroutine());

    }

    IEnumerator BulletTrailCoroutine()
    {
        float lifetime = 0f;
        yield return new WaitForFixedUpdate();
        while (lifetime < 1.0f)
        {
            transform.position = transform.position + direction.normalized * speed/50f;
            yield return new WaitForFixedUpdate();
            lifetime += 0.02f;
            if ((direction + (end - transform.position)).magnitude < direction.magnitude) //if passed the target
            {
                Destroy(gameObject);
            }
        }
        yield return null;
        Destroy(gameObject);
    }
}
