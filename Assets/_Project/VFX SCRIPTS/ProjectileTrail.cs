using System.Collections;
using UnityEngine;

public class ProjectileTrail : MonoBehaviour
{
    public void Init(Vector3 origin, Vector3 target, Vector3 gunForward, float speed)
    {
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(gunForward);
        StartCoroutine(Travel(target, speed));
    }

    IEnumerator Travel(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}