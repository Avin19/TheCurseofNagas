using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class EnvironmentSO : ScriptableObject
{
    public LevelSO level;
    [Header("Environment")]
    public Transform[] trees;
    public Transform[] brushs;

    public Transform[] rocks;
}
