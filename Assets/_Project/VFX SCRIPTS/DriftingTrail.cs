using UnityEngine;
using System.Collections;

public class DriftingTrail : MonoBehaviour
{
    private TrailRenderer trail;
    private float fadeTime;
    private TankController tank;
    private Coroutine fadeRoutine;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        tank = GetComponentInParent<TankController>();
        if (tank == null)
        {
            Debug.LogError($"[DriftingTrail] No TankController found in parent hierarchy of '{gameObject.name}'. " +
                           "Make sure this object is a child of the tank.", gameObject);
            enabled = false;
            return;
        }
        tank.DriftStarted.AddListener(OnDriftStart);
        tank.DriftStopped.AddListener(OnDriftStop);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeTime = tank.trailFadeTime;
    }

    void OnDriftStart()
    {
        /*
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }*/
        Debug.Log($"Drift on trail");
        trail.emitting = true;
    }
    void OnDriftStop()
    {
        DetachTrail(trail);
    }

    private void DetachTrail(TrailRenderer originalTrail)
    {
        // Create a temporary clone of the trail
        GameObject trailClone = new GameObject("TrailClone");
        trailClone.transform.position = originalTrail.transform.position;
        trailClone.transform.rotation = originalTrail.transform.rotation;

        TrailRenderer cloneRenderer = trailClone.AddComponent<TrailRenderer>();
        cloneRenderer.material = originalTrail.material;
        cloneRenderer.startWidth = originalTrail.startWidth;
        cloneRenderer.endWidth = originalTrail.endWidth;
        cloneRenderer.time = originalTrail.time;
        cloneRenderer.startColor = originalTrail.startColor;
        cloneRenderer.endColor = originalTrail.endColor;
        cloneRenderer.numCapVertices = originalTrail.numCapVertices;
        cloneRenderer.numCornerVertices = originalTrail.numCornerVertices;

        // Stop the original from emitting
        originalTrail.emitting = false;

        // Start fading the clone
        fadeRoutine = StartCoroutine(FadeOutAndDestroyTrail(cloneRenderer));
    }

    private IEnumerator FadeOutAndDestroyTrail(TrailRenderer trail)
    {
        float elapsed = 0f;
        Color startColor = trail.startColor;
        Color endColor = trail.endColor;

        while (elapsed < fadeTime)
        {
            float t = elapsed / fadeTime;
            Color newStart = startColor;
            Color newEnd = endColor;
            newStart.a = Mathf.Lerp(1f, 0f, t);
            newEnd.a = Mathf.Lerp(1f, 0f, t);
            trail.startColor = newStart;
            trail.endColor = newEnd;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(trail.gameObject);
    }
}
