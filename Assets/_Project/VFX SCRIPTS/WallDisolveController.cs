using System.Collections;
using UnityEngine;

public class WallDissolveController : MonoBehaviour
{
    [Header("Dissolve settings")]
    public float dissolveDuration = 2f;

    [Header("Disable collider when done?")]
    public bool disableColliderWhenDone = true;

    private Material material;
    private Collider wallCollider;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        wallCollider = GetComponent<Collider>();
    }

    public void StartDissolve()
    {
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);

            material.SetFloat("_Dissolve", t);
            material.SetFloat("_EdgeColorIntensity", Mathf.Sin(t * Mathf.PI) * 3f);

            yield return null;
        }

        material.SetFloat("_Dissolve", 1f);

        if (disableColliderWhenDone && wallCollider != null)
            wallCollider.enabled = false;
    }
}