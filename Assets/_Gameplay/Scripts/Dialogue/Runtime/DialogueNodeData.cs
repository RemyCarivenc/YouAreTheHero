using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string guid;
    public string dialogueText;
    public Vector2 position;
    public List<Sentences> sentences = new List<Sentences>();
}
