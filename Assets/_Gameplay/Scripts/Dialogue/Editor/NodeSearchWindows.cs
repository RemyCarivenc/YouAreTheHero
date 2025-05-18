using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindows : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView graphView;
    private EditorWindow window;
    private Texture2D indentationIcon;

    public void Init(EditorWindow _window, DialogueGraphView _graphview)
    {
        graphView = _graphview;
        window = _window;

        //Indentation hack for search window as a transparent icon
        indentationIcon = new Texture2D(1, 1);
        indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
        indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0));
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1));
        /* foreach (var dialog in graphView.allDialogs)
         {
             tree.Add(new SearchTreeEntry(new GUIContent(dialog.name, indentationIcon)){userData = new Dialog(), level =2});
         }
         tree.Add(new SearchTreeGroupEntry(new GUIContent("Fight"),1));
         foreach (var fight in graphView.allFights)
         {
             tree.Add(new SearchTreeEntry(new GUIContent(fight.name, indentationIcon)){userData = new Fight(), level =2});
         }*/
        tree.Add(new SearchTreeEntry(new GUIContent("Dialogue Node", indentationIcon))
        {
            level = 2,
            userData = new DialogueNode()
        });
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousPosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
        var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousPosition);
        switch (SearchTreeEntry.userData)
        {
            case DialogueNode dialogueNode:
                graphView.CreateNode("Dialogue Node", localMousePosition);
                return true;
           /* case Dialog dialog:
                graphView.CreateNode(SearchTreeEntry.name, localMousePosition, DialogueNodeData.TypeInteractions.Dialog);
                return true;
            case Fight fight:
                graphView.CreateNode(SearchTreeEntry.name, localMousePosition, DialogueNodeData.TypeInteractions.Fight);
                return true;*/
            default:
                return false;
        }
    }
}
