using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Book : MonoBehaviour
{
    public List<RectTransform> pages;
    public int pageIndex = 0;

    private PageFlipper flipper;
    [SerializeField] private Button backButton;
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button bookButton;
    [SerializeField] private List<Bookmark> bookmarks;

    private Animator animator;

    public static Book Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        flipper = new PageFlipper();
        animator = GetComponent<Animator>();
        InitialStart();

        foreach (var bookmark  in bookmarks)
        {
            bookmark.bookmarkButton.onClick.AddListener(() => MoveBookmark(bookmark));
        }
    }

    private void MoveBookmark(Bookmark bookmark)
    {
        RotateToIndex(bookmark.bookmarkIndex);
    }

    private void InitialStart()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].rotation = Quaternion.identity;
        }

        if (pages.Count > 0)
        {
            pages[0].SetAsLastSibling();
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        backButton.interactable = (pageIndex > -1);

        forwardButton.interactable = (pageIndex < pages.Count);
    }

    public void HandleLeftButtonClick()
    {
        if (flipper.isRotating || pageIndex <= 0) return;

        foreach (var bookmark in bookmarks)
        {
            if (bookmark.IsPlayingNow()) return;
        }

        pageIndex--;
        pages[pageIndex].SetAsLastSibling();
        flipper.Rotate(pages[pageIndex], 0);
        UpdateButtonStates();

        foreach (var bookmark in bookmarks)
        {
            if (pageIndex == bookmark.bookmarkIndex)
            {
                if (!bookmark.isBookmarkRight)
                {
                    bookmark.GoRight();
                } 
            }
        }

    }

    public void HandleRightButtonClick()
    {
        if (flipper.isRotating || pageIndex >= pages.Count) return;

        foreach (var bookmark in bookmarks)
        {
            if (bookmark.IsPlayingNow()) return;
        }

        pages[pageIndex].SetAsLastSibling();
        flipper.Rotate(pages[pageIndex], 180);
        pageIndex++;
        UpdateButtonStates();

        foreach (var bookmark in bookmarks)
        {
            if (pageIndex == bookmark.bookmarkIndex+1)
            {
                bookmark.GoLeft();
            }
        }
    }

    public void RotateToIndex(int index)
    {
        if (index < -1 || index >= pages.Count) return;
        StartCoroutine(RotatePages(index));
    }

    public void OpenBook()
    {
        bookButton.gameObject.SetActive(false);
        animator.enabled = true;
        animator.SetBool("closeBook", false);
        animator.SetBool("openBook", true);
        StartCoroutine(OpenBookCoroutine());
    }

    public void CloseBook()
    {
        backButton.gameObject.SetActive(false);
        forwardButton.gameObject.SetActive(false);
        if (flipper.isRotating) return;
        StartCoroutine(CloseBookSequence());
    }

    private IEnumerator CloseBookSequence()
    {
        foreach (var bookmark in bookmarks)
        {
            if (!bookmark.isBookmarkRight)
            {
                bookmark.GoRight();
            }
        }

        if (pageIndex > 0)
        {
            float originalFlipDuration = flipper.flipDuration;
            flipper.flipDuration = 0.2f;

            yield return StartCoroutine(RotatePages(0));

            flipper.flipDuration = originalFlipDuration;
        }

        bookButton.gameObject.SetActive(true);
        animator.enabled = true;
        animator.SetBool("closeBook", true);
        animator.SetBool("openBook", false);
    }

    private IEnumerator RotatePages(int index)
    {
        flipper.flipDuration = 0.2f;

        while (pageIndex < index)
        {
            if (flipper.isRotating)
            {
                yield return null;
                continue;
            }

            Debug.Log("Right click");
            HandleRightButtonClick();
            yield return new WaitUntil(() => !flipper.isRotating);
        }

        while (pageIndex > index)
        {
            if (flipper.isRotating)
            {
                yield return null;
                continue;
            }

            Debug.Log("Left click");
            HandleLeftButtonClick();
            yield return new WaitUntil(() => !flipper.isRotating);
        }

        flipper.flipDuration = 0.8f;
    }

    private IEnumerator OpenBookCoroutine()
    {
        yield return WaitForAnimation("New Animation");
        pageIndex = 1;
        animator.enabled = false;
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        AnimatorStateInfo stateInfo;
        do
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (stateInfo.normalizedTime < 1.0f);
    }
}