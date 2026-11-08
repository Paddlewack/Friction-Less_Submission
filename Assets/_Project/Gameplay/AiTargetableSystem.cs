using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Used to define points on a character/object which can be detected by an AI targeting system.
/// </summary>
public class AiTargetableSystem : MonoBehaviour
{
    /// <summary>
    /// The points on a character/object which can be detected by an AI targeting system (i.e. a bunch of empty game objects which are descendents of the object).
    /// </summary>
    public List<Transform> targetablePoints;
}
