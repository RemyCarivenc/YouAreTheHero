using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView graphView;
    private DialogueContainer dialogueContainer;
    private string fileName = "Name to create";

    private const string guidStart = "94772229-5e60-451a-8ee6-c8419720b876";
    private const string portNameStart = "Next";

    private ObjectField dialogueContainerObjectField;

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
    }



    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        toolbar.Add(new Button(() => CreateDialogueContainer()) { text = "Create Dialog" });
        var fileText = new TextField();
        fileText.SetValueWithoutNotify(fileName);
        fileText.MarkDirtyRepaint();
        fileText.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        toolbar.Add(fileText);



        dialogueContainerObjectField = new ObjectField();
        dialogueContainerObjectField.SetValueWithoutNotify(dialogueContainer);
        dialogueContainerObjectField.objectType = typeof(DialogueContainer);
        dialogueContainerObjectField.MarkDirtyRepaint();
        dialogueContainerObjectField.RegisterValueChangedCallback(evt => dialogueContainer = (DialogueContainer)evt.newValue);
        toolbar.Add(dialogueContainerObjectField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

        rootVisualElement.Add(toolbar);
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap { anchored = true };
        //This will give 10 px offset from left side
        var cords = graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        graphView.Add(miniMap);
    }

    private void RequestDataOperation(bool save)
    {
        if (dialogueContainer == null)
        {
            EditorUtility.DisplayDialog("Invalid dialog!", "Please enter a valid dialog.", "OK");
            return;
        }
        var saveUtility = GraphSaveUtility.GetInstance(graphView);
        if (save)
            saveUtility.SaveGraph(dialogueContainer);
        else
            saveUtility.LoadGraph(dialogueContainer);
    }

    private void CreateDialogueContainer()
    {
        if (string.IsNullOrEmpty(fileName) || fileName == "Name to create")
            EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
        else if (Resources.Load<DialogueContainer>(fileName) != null)
            EditorUtility.DisplayDialog("Invalid File name", "File already exist", "OK");
        else
        {
            var saveUtility = GraphSaveUtility.GetInstance(graphView);

            dialogueContainer = null;

            var dialogueContainerTemp = ScriptableObject.CreateInstance<DialogueContainer>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(dialogueContainerTemp, $"Assets/Resources/{fileName}.asset");

            dialogueContainer = Resources.Load<DialogueContainer>(fileName);

            if (dialogueContainerObjectField.value == null)
            {
                saveUtility.SaveGraph(dialogueContainer);
                if (dialogueContainer.nodeLinks.Count == 0)
                {
                    dialogueContainer.nodeLinks.Add(new NodeLinkData());
                    dialogueContainer.nodeLinks[0].baseNodeGuid = guidStart;
                    dialogueContainer.nodeLinks[0].portName = portNameStart;
                }
            }
            else
            {
                dialogueContainer.nodeLinks.Add(new NodeLinkData());
                dialogueContainer.nodeLinks[0].baseNodeGuid = guidStart;
                dialogueContainer.nodeLinks[0].portName = portNameStart;
                saveUtility.LoadGraph(dialogueContainer);
            }
            DialogueContainer container = dialogueContainer;
            container.nodeLinks = dialogueContainer.nodeLinks;
            container.dialogueNodeDatas = dialogueContainer.dialogueNodeDatas;
            EditorUtility.SetDirty(container);
            
            AssetDatabase.SaveAssets();
            dialogueContainerObjectField.SetValueWithoutNotify(dialogueContainer);
        }

    }


    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
}
