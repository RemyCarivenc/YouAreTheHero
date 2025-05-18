using System.Collections;
using System.Collections.Generic;
using Localization;
using Localization.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomPopUp : EditorWindow
{
    [SerializeField]
    private LanguageAuto Sentence;

    private static DialogueNode dialogueNode;
    private static Label label;
    private static Port port;


    public static void Init(DialogueNode _dialogueNode, Port _port, Vector2 _mousePosition, Label _label)
    {
        dialogueNode = _dialogueNode;
        port = _port;
        label = _label;

        CustomPopUp window = ScriptableObject.CreateInstance<CustomPopUp>();
        window.position = new Rect(_mousePosition.x, _mousePosition.y, 300, 150);
        window.ShowPopup();
    }

    void OnGUI()
    {
        SerializedObject so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("Sentence"));
        so.ApplyModifiedProperties();

        if (GUILayout.Button("Validate!"))
        {
            label.text = LanguageCodeUtility.GetName(Sentence.Code);
            port.portName = Sentence.Code.ToString();
            
            this.Close();
        }
        if (GUILayout.Button("Cancel!"))
        {
            this.Close();
        }
    }
}
