using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Localization;
using UnityEngine;
using UnityEngine.UI;

public class DialogueContainerController : MonoBehaviour
{
    private int countSentences = 0;

    [SerializeField] private DialogueContainer storie;
    [SerializeField] private Button choicePrefab;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Text dialogueText;

    [Header("Typing Text")]
    public float typingSpeed;
    private int index;
    private bool endTyping;

    private List<Sentences> page = new List<Sentences>();
    private IEnumerable<NodeLinkData> choices;
    private Button[] buttons;

    private bool waitChoice;

    private void Start()
    {
        NodeLinkData narrativeData = storie.nodeLinks.First();
        ProceedToNarrative(narrativeData.targetNodeGuid);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !waitChoice)
        {
            if (dialogueText.text != page[countSentences].sentence)
            {
                StopAllCoroutines();
                dialogueText.text = page[countSentences].sentence;
                endTyping = true;
            }
            else if (countSentences < page.Count - 1)
            {
                countSentences++;
                dialogueText.text = "";
                StartCoroutine(Type(page[countSentences].sentence));
            }
        }

        if (countSentences == page.Count - 1 && endTyping)
            GenerateChoices();

    }

    private void ProceedToNarrative(string _narrativeDataGUID)
    {
        waitChoice = false;
        page = storie.dialogueNodeDatas.Find(x => x.guid == _narrativeDataGUID).sentences;
        choices = storie.nodeLinks.Where(x => x.baseNodeGuid == _narrativeDataGUID);

        buttons = choiceContainer.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }

        StartCoroutine(Type(page[countSentences].sentence));

    }

    private void GenerateChoices()
    {
        waitChoice = true;
        countSentences = 0;
        foreach (var choice in choices)
        {
            var button = Instantiate(choicePrefab, choiceContainer);
            button.GetComponentInChildren<Text>().text = new LanguageAuto(Int32.Parse(choice.portName));
            button.onClick.AddListener(() => ProceedToNarrative(choice.targetNodeGuid));
        }
    }

    private IEnumerator Type(string _sentence)
    {
        endTyping = false;
        foreach (char letter in _sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        endTyping = true;
        countSentences++;
    }

}
