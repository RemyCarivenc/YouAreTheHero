using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueContainerController : MonoBehaviour
{
    private int countSentences = 0;

    [SerializeField] private DialogueContainer storie;
    [SerializeField] private Button choicePrefab;
    [SerializeField] private Transform choiceContainer;

    private List<Sentences> page = new();
    private IEnumerable<NodeLinkData> choices;

    private bool waitChoice;

    private bool nextSentence = false;

    void OnEnable()
    {
        Actions.EndType += EndType;
    }

    void OnDisable()
    {
        Actions.EndType -= EndType;
    }

    private void EndType()
    {
        nextSentence = true;

        if (countSentences == page.Count - 1)
            GenerateChoices();
    }

    private void Start()
    {
        NodeLinkData narrativeData = storie.nodeLinks.First();
        ProceedToNarrative(narrativeData.targetNodeGuid);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !waitChoice)
        {
            if (!nextSentence)
                Actions.SkipType?.Invoke();
            else
            {
                countSentences++;
                Actions.StartType?.Invoke(page[countSentences].sentence);
                nextSentence = false;
            }
        }
    }

    private void ProceedToNarrative(string _narrativeDataGUID)
    {
        waitChoice = false;
        page = storie.dialogueNodeDatas.Find(x => x.guid == _narrativeDataGUID).sentences;
        choices = storie.nodeLinks.Where(x => x.baseNodeGuid == _narrativeDataGUID);

        Button[] buttons = choiceContainer.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }

        Actions.StartType?.Invoke(page[countSentences].sentence);
    }

    private void GenerateChoices()
    {
        waitChoice = true;
        countSentences = 0;
        /*foreach (var choice in choices)
        {
            var button = Instantiate(choicePrefab, choiceContainer);
            button.GetComponentInChildren<Text>().text = new LanguageAuto(Int32.Parse(choice.portName));
            button.onClick.AddListener(() => ProceedToNarrative(choice.targetNodeGuid));
        }*/
    }
}
