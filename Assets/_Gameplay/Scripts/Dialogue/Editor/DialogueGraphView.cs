using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    private NodeSearchWindows searchWindows;

    public DialogueGraphView(EditorWindow _editorWindow)
    {

        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(_editorWindow);
    }

    private void AddSearchWindow(EditorWindow _editorWindow)
    {
        searchWindows = ScriptableObject.CreateInstance<NodeSearchWindows>();
        searchWindows.Init(_editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindows);

    }

    public override List<Port> GetCompatiblePorts(Port _startPort, NodeAdapter _nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
       {
           if (_startPort != port && _startPort.node != port.node)
               compatiblePorts.Add(port);
       });

        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode _node, Direction _portDirection, Port.Capacity _capacity = Port.Capacity.Single)
    {
        return _node.InstantiatePort(Orientation.Horizontal, _portDirection, _capacity, typeof(DialogueNode));
    }

    private DialogueNode GenerateEntryPointNode()
    {
        var node = new DialogueNode
        {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            entryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        generatedPort.portColor = Color.green;
        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public void CreateNode(string _nodeName, Vector2 _position)
    {
        AddElement(CreateDialogueNode(_nodeName, _position, new List<Sentences>()));
    }

    public DialogueNode CreateDialogueNode(string _nodeName, Vector2 _position, List<Sentences> _sentences)
    {
        var tempDialogueNode = new DialogueNode
        {
            title = _nodeName,
            dialogueText = _nodeName,
            GUID = Guid.NewGuid().ToString(),
            sentences = new List<Sentences>(_sentences)
        };

        var inputPort = GeneratePort(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "";
        inputPort.portColor = Color.green;
        tempDialogueNode.inputContainer.Add(inputPort);

        tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        Label label = new Label()
        {
            text = " List Sentences"
        };
        tempDialogueNode.inputContainer.Add(label);

        IntegerField integerField = new IntegerField()
        {
            value = tempDialogueNode.sentences.Count
        };
        integerField.RegisterValueChangedCallback(evt =>
        {
            GenerateSentences(tempDialogueNode, integerField);
        });
        tempDialogueNode.inputContainer.Add(integerField);
        if (tempDialogueNode.sentences.Count != 0)
            GenerateSentences(tempDialogueNode, integerField);

        var textField = new TextField("");
        textField.RegisterValueChangedCallback(evt =>
        {
            tempDialogueNode.dialogueText = evt.newValue;
            tempDialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(tempDialogueNode.title);
        tempDialogueNode.mainContainer.Add(textField);

        var button = new Button(() =>
        {
            AddChoicePort(tempDialogueNode);
        });
        button.text = "New Choice";
        tempDialogueNode.titleContainer.Add(button);

        tempDialogueNode.RefreshExpandedState();
        tempDialogueNode.RefreshPorts();
        tempDialogueNode.SetPosition(new Rect(_position, defaultNodeSize));

        return tempDialogueNode;
    }

    /* private FieldInfo GetFieldViaPath(Type type, string path)
     {
         var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
         var containingObjectType = type;
         FieldInfo fi = null;
         var paths = path.Split('.');


         for (int i = 0; i < paths.Length; i++)
         {
             fi = containingObjectType.GetField(paths[i], flags);
             if (fi != null)
             {
                 // there are only two container field type that can be serialized:
                 // Array and List<T>
                 if (fi.FieldType.IsArray)
                     containingObjectType = fi.FieldType.GetElementType();
                 else if (fi.FieldType.IsGenericType)
                     containingObjectType = fi.FieldType.GetGenericArguments()[0];
                 else
                     containingObjectType = fi.FieldType;
             }
             else
             {
                 break;
             }

         }
         return fi;
     }*/

    private void GenerateSentences(DialogueNode _dialogueNode, IntegerField _integer)
    {
        List<ObjectField> tempObjectField = new List<ObjectField>();

        for (int i = _dialogueNode.inputContainer.childCount; i > 0; i--)
        {
            if (_dialogueNode.inputContainer[i - 1].GetType() == typeof(ObjectField))
                _dialogueNode.inputContainer.RemoveAt(i - 1);
        }

        if (_integer.value == 0)
        {
            _dialogueNode.sentences = new List<Sentences>();
        }
        else
        {
            for (int i = 0; i < _integer.value; i++)
            {
                var scriptableObject = new ObjectField();
                scriptableObject.objectType = typeof(Sentences);
                if (_dialogueNode.sentences.Count > i)
                    scriptableObject.value = _dialogueNode.sentences[i];
                else
                    _dialogueNode.sentences.Add((Sentences)scriptableObject.value);

                scriptableObject.tabIndex = i;
                scriptableObject.RegisterValueChangedCallback(evt =>
                {
                    _dialogueNode.sentences[scriptableObject.tabIndex] = (Sentences)evt.newValue;
                });
                _dialogueNode.inputContainer.Add(scriptableObject);

                if (i == _integer.value - 1)
                    if (_dialogueNode.sentences.Count - 1 > i)
                    {
                        for (int y = _dialogueNode.sentences.Count - 1; y > i; y--)
                        {
                            _dialogueNode.sentences.RemoveAt(y);
                        }
                    }
            }
        }


        _dialogueNode.RefreshPorts();
        _dialogueNode.RefreshExpandedState();
    }

    public void AddChoicePort(DialogueNode _dialogueNode, string _overriddenPortName = "")
    {

        var generatedPort = GeneratePort(_dialogueNode, Direction.Output);

        var PortLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(PortLabel);

        var outputPortCount = _dialogueNode.outputContainer.Query("connector").ToList().Count;
        var outputPortName = string.IsNullOrEmpty(_overriddenPortName)
                ? 1
                : Int32.Parse(_overriddenPortName);

        Label label = new Label()
        {
            text = "" //LanguageCodeUtility.GetName(outputPortName)

        };
        generatedPort.contentContainer.Add(label);


        var addSentence = new Button(() => SetSentences(_dialogueNode, generatedPort, label))
        {
            text = "+"
        };
        generatedPort.contentContainer.Add(addSentence);

        var deleteButton = new Button(() => RemovePort(_dialogueNode, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);



        generatedPort.portName = outputPortName.ToString();
        generatedPort.portColor = Color.green;

        _dialogueNode.outputContainer.Add(generatedPort);
        _dialogueNode.RefreshPorts();
        _dialogueNode.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode _dialogueNode, Port _generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == _generatedPort.portName && x.output.node == _generatedPort.node);

        if (targetEdge.Count() != 0)
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);

            RemoveElement(targetEdge.First());
        }
        _dialogueNode.outputContainer.Remove(_generatedPort);
        _dialogueNode.RefreshPorts();
        _dialogueNode.RefreshExpandedState();
    }

    private void SetSentences(DialogueNode _dialogueNode, Port _port, Label _label)
    {
        CustomPopUp.Init(_dialogueNode, _port, GUIUtility.GUIToScreenPoint(Event.current.mousePosition), _label);
    }



}
