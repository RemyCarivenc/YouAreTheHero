using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView targetGraphView;
    private DialogueContainer containerCache;

    private List<Edge> Edges => targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView _targetGraphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = _targetGraphView
        };
    }

    public void SaveGraph(DialogueContainer _dialogueContainer)
    {
        if (!SaveNode(_dialogueContainer)) return;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath($"Assets/Resources/{_dialogueContainer.name}.asset", typeof(DialogueContainer));

        if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
        {
            AssetDatabase.CreateAsset(_dialogueContainer, $"Assets/Resources/{_dialogueContainer.name}.asset");
        }
        else
        {
            DialogueContainer container = loadedAsset as DialogueContainer;
            container.nodeLinks = _dialogueContainer.nodeLinks;
            container.dialogueNodeDatas = _dialogueContainer.dialogueNodeDatas;
            EditorUtility.SetDirty(container);
        }

        AssetDatabase.SaveAssets();
    }

    private bool SaveNode(DialogueContainer _container)
    {
        _container.dialogueNodeDatas.Clear();

        if (!Edges.Any()) return false;

        _container.nodeLinks.Clear();

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {

            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            _container.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var dialogueNode in Nodes.Where(node => !node.entryPoint))
        {
            List<Sentences> tempSentense = new List<Sentences>();
            foreach (var sentence in dialogueNode.sentences)
            {
                if(sentence != null)
                    tempSentense.Add(sentence);
            }

            _container.dialogueNodeDatas.Add(new DialogueNodeData
            {
                guid = dialogueNode.GUID,
                dialogueText = dialogueNode.dialogueText,
                position = dialogueNode.GetPosition().position,
                sentences = new List<Sentences>(tempSentense)
            });
        }

        return true;
    }

    public void LoadGraph(DialogueContainer _dialogueContainer)
    {
        containerCache = _dialogueContainer;
        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found", "Target dialogue graph file does not exists!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        if (Nodes.Count == 1) return;

        for (int i = 0; i < Nodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.baseNodeGuid == Nodes[i].GUID).ToList();
            for (var j = 0; j < connections.Count; j++)
            {
                var targetNodeGUID = connections[j].targetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(
                    containerCache.dialogueNodeDatas.First(x => x.guid == targetNodeGUID).position,
                    targetGraphView.defaultNodeSize
                ));
            }
        }
    }

    private void LinkNodes(Port _output, Port _input)
    {
        var tempEdge = new Edge
        {
            output = _output,
            input = _input
        };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in containerCache.dialogueNodeDatas)
        {
            var tempNode = targetGraphView.CreateDialogueNode(nodeData.dialogueText, nodeData.position,nodeData.sentences);
            tempNode.GUID = nodeData.guid;
            targetGraphView.AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.guid).ToList();
            nodePorts.ForEach(x => targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        Nodes.Find(x => x.entryPoint).GUID = containerCache.nodeLinks[0].baseNodeGuid;

        foreach (var node in Nodes)
        {
            if (node.entryPoint) continue;

            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));

            targetGraphView.RemoveElement(node);
        }
    }
}