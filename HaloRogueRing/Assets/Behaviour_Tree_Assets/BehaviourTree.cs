using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "behaviourtree/createtree")] //Code Review highlighted that I should add a string to pass in to make a sub menu
public class BehaviourTree : ScriptableObject
{
    public Nodes rootNode;
    public Nodes.State treeState = Nodes.State.Running;
    public List<Nodes> nodes = new List<Nodes>();
    public Blackboard blackboard = new Blackboard();


    public Nodes.State Update()
    {
        if(rootNode.state == Nodes.State.Running)
        {
            treeState = rootNode.Update();
        }
        return treeState; 
    }
#if UNITY_EDITOR

    public Nodes CreateNode(System.Type type) 
    { 
      Nodes node = ScriptableObject.CreateInstance(type) as Nodes;
      node.name = type.Name;
      node.guid = GUID.Generate().ToString();

      Undo.RecordObject(this, "Behaviour Tree (AddChild)");  
      nodes.Add(node);

      if (!Application.isPlaying)
      {
          AssetDatabase.AddObjectToAsset(node, this);
      }
      
      Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
      AssetDatabase.SaveAssets();
      return node;
    
    }

    public void DeleteNode(Nodes node)
    {
        nodes.Remove(node);
        Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
       // AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);

        AssetDatabase.SaveAssets();
    }

    public void AddChild(Nodes parent, Nodes child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator)
        {
            Undo.RecordObject(decorator, "Behaviour Tree (AddChild");
            decorator.child = child;
            EditorUtility.SetDirty(decorator);
        }

        RootNode rootNode = parent as RootNode;
        if (rootNode)
        {
            Undo.RecordObject(rootNode, "Behaviour Tree (AddChild");
            rootNode.child = child;
            EditorUtility.SetDirty(rootNode);
        }

        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            Undo.RecordObject(composite, "Behaviour Tree (AddChild");
            composite.children.Add(child);
            EditorUtility.SetDirty(composite);
        }
    }
    public void RemoveChild(Nodes parent, Nodes child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator)
        {
            Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild");
            decorator.child = null;
            EditorUtility.SetDirty(decorator);
        }

        RootNode rootNode = parent as RootNode;
        if (rootNode)
        {
            Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild");
            rootNode.child = null;
            EditorUtility.SetDirty(rootNode);
        }

        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            Undo.RecordObject(composite, "Behaviour Tree (RemoveChild");
            composite.children.Remove(child);
            EditorUtility.SetDirty(composite);
        }
    }

    public List<Nodes> GetChildren(Nodes parent)
    {
        List<Nodes> children = new List<Nodes>();
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator && decorator.child != null)
        {
            children.Add(decorator.child);
        }

        RootNode rootNode = parent as RootNode;
        if (rootNode && rootNode.child != null)
        {
            children.Add(rootNode.child);
        }

        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            return composite.children;
        }
        return children;
    }
#endif

    public void Traverse(Nodes node, System.Action<Nodes> visiter)
    {
        if (node)
        {
            visiter.Invoke(node);
            var children = GetChildren(node);
            children.ForEach((n) => Traverse(n, visiter));
        }
    }

    public BehaviourTree Clone()
    {
        BehaviourTree tree = Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        tree.nodes = new List<Nodes>();
        Traverse(tree.rootNode, (n) =>
        {
            tree.nodes.Add(n);
        });
            
        return tree;
    }

    public void Bind()//AiAgent agent)
    {
        Traverse(rootNode, node =>
        {
            //node.agent = agent;
            node.blackboard = blackboard;
        });
    }
}

