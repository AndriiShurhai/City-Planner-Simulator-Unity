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
    [SerializeField] private Button backButton;
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button bookButton;
    [SerializeField] private List<Bookmark> bookmarks;
    [SerializeField] private List<PageVideo> pageVideos;


    private PageFlipper flipper;
    private Animator animator;

    public static Book Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        flipper = new PageFlipper();
        animator = GetComponent<Animator>();
        InitialStart();
        RegisterBookmarksEvents();
        HandleVideoPlayback();
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

    private void RegisterBookmarksEvents()
    {
        foreach (var bookmark in bookmarks)
        {
            var currentBookmark = bookmark;
            currentBookmark.bookmarkButton.onClick.AddListener(() => MoveBookmark(currentBookmark));
        }
    }
    private void UpdateButtonStates()
    {
        backButton.interactable = (pageIndex > -1);

        forwardButton.interactable = (pageIndex < pages.Count-1);
    }
    private void HandleVideoPlayback()
    {
        foreach (var pageVideo in pageVideos)
        {
            bool isCurrentPage = pageVideo.pageIndex == pageIndex;
            if (pageVideo.videoDisplay != null)
            {
                if (isCurrentPage)
                {
                    pageVideo.videoDisplay.SetActive(isCurrentPage);
                }
                else
                {
                    StartCoroutine(DisactivateVideoPlayer(pageVideo));
                }
            }

            if (pageVideo.videoPlayer != null)
            {
                if (isCurrentPage) pageVideo.videoPlayer.Play();
                else pageVideo.videoPlayer.Stop();
            }
        }
    }

    private IEnumerator DisactivateVideoPlayer(PageVideo video)
    {
        yield return new WaitUntil(() => !flipper.isRotating);
        video.videoDisplay.SetActive(false);
    }

    public void HandleLeftButtonClick()
    {
        if (flipper.isRotating || pageIndex <= 0 || AnyBookmarkPlaying()) return;

        pageIndex--;
        pages[pageIndex].SetAsLastSibling();
        flipper.Rotate(pages[pageIndex], 0);
        UpdateButtonStates();
        HandleVideoPlayback();

        foreach (var bookmark in bookmarks)
        {
            if (pageIndex == bookmark.bookmarkIndex && !bookmark.isBookmarkRight)
            {
                bookmark.GoRight();
            }
        }

    }
    public void HandleRightButtonClick()
    {
        if (flipper.isRotating || pageIndex >= pages.Count || AnyBookmarkPlaying()) return;

        pages[pageIndex].SetAsLastSibling();
        flipper.Rotate(pages[pageIndex], 180);
        pageIndex++;
        UpdateButtonStates();
        StartCoroutine(DelayedVideoPlayback());

        foreach (var bookmark in bookmarks)
        {
            if (pageIndex == bookmark.bookmarkIndex+1 && bookmark.isBookmarkRight)
            {
                bookmark.GoLeft();
            }
        }
        HandleVideoPlayback();
    }

    private bool AnyBookmarkPlaying()
    {
        foreach (var bookmark in bookmarks)
        {
            if (bookmark.IsPlayingNow()) return true;
        }
        return false;
    }
    public void RotateToIndex(int index)
    {
        if (index < -1 || index >= pages.Count) return;

        StartCoroutine(RotatePages(index));
        foreach (var bookmark in bookmarks)
        {
            if (bookmark.bookmarkIndex > index && !bookmark.isBookmarkRight)
            {
                bookmark.GoRight();
            }
            else if (bookmark.bookmarkIndex < index && bookmark.isBookmarkRight)
            {
                bookmark.GoLeft();
            } 
        }

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

    private IEnumerator DelayedVideoPlayback()
    {
        yield return new WaitUntil(() => !flipper.isRotating);
        HandleVideoPlayback();
    }
    private IEnumerator WaitForAnimation(string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        do
        {
            yield return null;
        } while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    private void MoveBookmark(Bookmark bookmark)
    {
        if (animator.enabled) return;
        RotateToIndex(bookmark.bookmarkIndex);
    }

}