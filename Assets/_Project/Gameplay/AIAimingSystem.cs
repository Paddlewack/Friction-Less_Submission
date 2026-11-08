using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AIAimingSystem : AimingSystem
{
    AiTargetingSysData data;
    Vector3 forwardDirection;
    /// <summary>
    /// Event invoked every time a target is seen by this AI (up to once per scanRate period set in data if the target remains detectable)
    /// </summary>
    public UnityEvent onDetect = new();
    /// <summary>
    /// Point where the AI will cast rays from to detect targets. 
    /// May result in odd behaviour if this is not the centre of the centre of the game object which is rotated to face the target.
    /// </summary>
    Transform sightOrigin;
    /// <summary>
    /// Key: target game object. Value: List of targetable points on the game object. 
    /// Targetable objects will nedd an AiTargetableSystem with targetable points defined for this to work propoerly.
    /// </summary>
    public Dictionary<GameObject, List<Transform>> visibleTargetPoints { get; private set; } = new();
    Dictionary<GameObject, List<Transform>> inRangeTargets = new();
    float scanRate;
    float scanTimer = 0;

    Transform visionReference;
    GameObject parentObject;
    Transform turret;
    private LayerMask aimpointMask;

    public AIAimingSystem(Transform turret, GameObject parent, AiTargetingSysData targetingData, Transform eyePos, Transform visionReference = null)
    {
        this.turret = turret;
        parentObject = parent;
        forwardDirection = turret.forward;
        data = targetingData;
        sightOrigin = eyePos;
        scanRate = 0.2f;
        aimpointMask = targetingData.AimPointMask;
        this.visionReference = visionReference;
    }

    Vector3 GetForward()
    {
        return visionReference != null ? visionReference.forward : forwardDirection;
    }

    /// <summary>
    /// Detects all targets of the target faction within specified range, checks if the target is within the vision angle of the AI, and if the target is obstructed. 
    /// Updates visibleTargetPoints to reflect the targetable points which are visible. If any targets are visible onDetect event is invoked.
    /// </summary>
    void CheckDetectionZone()
    {
        inRangeTargets.Clear();
        visibleTargetPoints.Clear();

        Vector3 forward = GetForward();
        bool fullCircle = data.detctionZoneAngle >= 360f;
        // Detect all targets in range.
        int layerMask = LayerMask.GetMask(data.TargetFaction.ToString());
        Collider[] inRangeColliders = Physics.OverlapSphere(turret.position, data.detectionZoneRadius, layerMask);
        foreach (var hitCollider in inRangeColliders)
        {
            GameObject target = hitCollider.transform.root.gameObject;
            AiTargetableSystem aiTargetableSystem = target.GetComponentInChildren<AiTargetableSystem>();
            if (aiTargetableSystem != null)
            {
                inRangeTargets[target] = (aiTargetableSystem.targetablePoints);
            }
        }

        foreach(var target in inRangeTargets)
        {
            foreach (Transform point in target.Value)
            {
                Vector3 directionToPoint = (point.position - sightOrigin.position).normalized;

                if (!fullCircle &&
                    Vector3.Angle(forward, directionToPoint) > data.detctionZoneAngle / 2) continue;
                // Check line of sight:
                // Sort colliders hit by RaycastAll from AI to target point by distance, and check if the first collider hit which does not belong to the AI itself belongs to the target.
                RaycastHit[] hits;
                hits = Physics.RaycastAll(sightOrigin.position, directionToPoint, data.detectionZoneRadius, aimpointMask);
                Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));
                foreach (RaycastHit hit in hits)
                {
                    // If collider belongs to the AI game object, check the next one
                    if (hit.collider.gameObject == parentObject)
                    {
                        continue;
                    }
                    // If collider belongs to a descendent of the AI game object, check the next one
                    Transform[] childGameObjects = parentObject.GetComponentsInChildren<Transform>();
                    if (childGameObjects.Contains(hit.collider.transform))
                    {
                        continue;
                    }
                    // If the collider does not belong to the target, point is not visible.
                    if (hit.collider.transform.root.gameObject != target.Key.transform.root.gameObject)
                    {
                        break;
                    }
                    //If the point is visible add it into the visible points list for the target in the dictionary.
                    if (!visibleTargetPoints.ContainsKey(hit.collider.transform.root.gameObject))
                    {
                        visibleTargetPoints[hit.collider.transform.root.gameObject] = new List<Transform>();
                    }
                    visibleTargetPoints[hit.collider.transform.root.gameObject].Add(point);
                    break;
                }
                
            }
        }
        // Invoke onDetect if any targets were detected.
        if (visibleTargetPoints.Count > 0)
        {
            onDetect.Invoke();
        }
    }

    public override void Update()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer > scanRate)
        {
            CheckDetectionZone();
            scanTimer = 0;
        }
        //Yaw = Math.Clamp(Yaw, -maxYawAngle, maxYawAngle);
        //Debug.Log(Yaw);
        //Pitch = Math.Clamp(Pitch, minPitch, maxPitch);
    }
}
