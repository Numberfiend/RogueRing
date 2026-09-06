using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomSelectionNode : CompositeNode
{
    int current;

    protected override void OnStart()
    {
        current = Random.Range(0, children.Count);
    }

    protected override void OnStop()
    {

    }

    protected override State OnUpdate()
    {
        var child = children[current];
        return child.Update();
    }
}
