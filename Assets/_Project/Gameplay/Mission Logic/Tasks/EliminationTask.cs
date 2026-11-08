using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EliminationTask : BaseTask
{
    public override TaskType taskType => TaskType.Elimination;

    [Header("Elimination Task Config")]
    [Tooltip("Leave empty to count any tank death")]
    public List<string> targetNames; // match by target GameObject names
    public int killsRequired = 1;

    private int killCount;

    public override void Activate()
    {
        base.Activate();
        killCount = 0;
        IDamageable.onUnitDeath.AddListener(OnUnitDeath);
    }

    public override void Deactivate()
    {
        IDamageable.onUnitDeath.RemoveListener(OnUnitDeath);
        base.Deactivate();
    }
    public override GameObject[] GetTargets()
    {
        List<GameObject> gameObjectList = new List<GameObject>();

        List<IDamageable> damageables =
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDamageable>()
            .ToList();

        foreach (IDamageable damageable in damageables)
        {
            if (damageable.IsAlive)
            {
                MonoBehaviour mb = damageable as MonoBehaviour;
                if (mb != null && targetNames.Contains(mb.gameObject.name))
                    gameObjectList.Add(mb.gameObject);
            }
        }

        return gameObjectList.ToArray();
    }

    private void OnUnitDeath(IDamageable unit)
    {
        if (!IsActiveOrInProgress()) return;

        // Ignore player deaths
        if (unit is PlayerTankController) return;

        // Cast to MonoBehaviour to access gameObject name
        var mb = unit as MonoBehaviour;
        if (mb == null) return;

        bool isTarget = targetNames == null || targetNames.Count == 0
            || targetNames.Contains(mb.gameObject.name);

        if (!isTarget) return;

        killCount++;
        Debug.Log($"[Task] {taskName}: {killCount}/{killsRequired} kills");
        UpdateProgress((float)(1f / killsRequired));
    }

}