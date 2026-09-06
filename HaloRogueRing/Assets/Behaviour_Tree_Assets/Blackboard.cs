using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Blackboard
{
    public Vector3 moveToPosition;
    public GameObject moveToObject;
}
public class BlackboardExample : MonoBehaviour
{
    public Blackboard blackboard; 
}