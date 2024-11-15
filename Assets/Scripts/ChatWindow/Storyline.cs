using System;
using UnityEngine;

public class Storyline : MonoBehaviour
{
    public enum ConditionType
    {
        None, TimeInHours, Item
    }

    [Serializable]
    public struct ContinuationCondition
    {
        public ConditionType condition;
        public string value;
    }

    public string storylineName;
    public ContinuationCondition[] continuationConditions;
}