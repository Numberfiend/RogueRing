using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorNode : Nodes
{
    [HideInInspector] public Nodes child;

    public override Nodes Clone()
    {
        DecoratorNode node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}
