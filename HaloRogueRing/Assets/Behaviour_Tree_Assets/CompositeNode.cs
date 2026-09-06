using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : Nodes
{
    [HideInInspector] public List<Nodes> children = new List<Nodes>();

    public override Nodes Clone()
    {
        CompositeNode node = Instantiate(this);
        node.children = children.ConvertAll(c => c.Clone());
        return node;
    }
}
