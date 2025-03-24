using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System;
using System.Runtime.CompilerServices;

public class DialogueManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public List<string> dialogueLines = new List<string>
    {
        "I can't believe I've come this far...",
        "No one thought I would succeed.",
        "This is only the beginning!"
    };
    public string sceneName;

    private int currentLineIndex = 0;
    private bool cutsceneActive = false;

    public float typingSpeed = 0.02f;
    private bool isTyping = false;

    void Start()
    {
        StartCutscene();
    }

    void Update()
    {
        if (!cutsceneActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    public void StartCutscene()
    {
        cutsceneActive = true;
        currentLineIndex = 0;
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (currentLineIndex < dialogueLines.Count)
        {
            StopAllCoroutines();
            StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
        }
        else
        {
            EndCutscene();
        }
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void AdvanceDialogue()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
    }

    void EndCutscene()
    {
        cutsceneActive = false;
        dialogueText.gameObject.SetActive(false);


        SceneManagerController.Instance.CloseDialougeScene(sceneName);
        Debug.Log("Cutscene ended.");
    }
}
