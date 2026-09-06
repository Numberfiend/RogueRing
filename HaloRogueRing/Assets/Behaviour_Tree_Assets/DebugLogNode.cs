using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DebugLogNode : ActionNode
{
    public string message;
    public bool output;
    public Transform player;

    protected override void OnStart()
    {
        output = true;
        //Debug.Log($"OnStart{message}");
    }

    protected override void OnStop()
    {
       // Debug.Log($"OnStop{message}");
    }

    protected override State OnUpdate()
    {
        if(output == true)
        {
            
            Debug.Log($"OnUpdate{message}");
            return State.Success;
        }
        else
        {
            return State.Failure;
        }
        

        
       // Debug.Log($"Blackboard:{blackboard.moveToPosition}");
       // blackboard.moveToPosition.x += 1;
        
    }
}
