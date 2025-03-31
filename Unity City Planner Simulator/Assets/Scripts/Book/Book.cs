using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;


public class Book : MonoBehaviour
{
    public List<RectTransform> pages;

    public int pageIndex = 0;

    PageFlipper flipper;

    [SerializeField] private Button backButton;
    [SerializeField] private Button forwardButton;

    private Animator animator;


    public static Book Instance {  get; private set; }

    private void Awake()
    {
        if(Instance != null &&  Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        InitialStart();
        flipper = new PageFlipper();

        animator = GetComponent<Animator>();
        StartCoroutine(WaitForAnimation());
    }

    private void InitialStart()
    {
        for (int i =0; i < pages.Count; i++)
        {
            pages[i].transform.rotation = Quaternion.identity;
        }
        pages[pageIndex].SetAsLastSibling();
        //backButton.interactable = false;
    }

    public void ForwardButtonActions()
    {
        if (!backButton.interactable)
        {
            backButton.interactable = true;
        }

        if (pageIndex == pages.Count - 1)
        {
            forwardButton.interactable = false;
        }
    }

    public void BackButtonActions()
    {
        if (!forwardButton.interactable)
        {
            forwardButton.interactable = true;
        }

        if (pageIndex -1 == -1)
        {
            backButton.interactable = false;
        }
    }

    public void HandleLeftButtonClick()
    {
        if (flipper.isRotating) return;
        pages[pageIndex].SetAsLastSibling();
        BackButtonActions();
        flipper.Rotate(pages[pageIndex], 0);
        pageIndex--;
    }

    public void HandleRightButtonClick()
    {
        if (flipper.isRotating) return;
        pageIndex++;
        ForwardButtonActions();
        flipper.Rotate(pages[pageIndex], 180);
        pages[pageIndex].SetAsLastSibling();
    }

    public void RotateToIndex(int index)
    {
        StartCoroutine(RotatePages(index));
    }

    public void CloseBook()
    {
        if(flipper.isRotating) return;
        StartCoroutine(RotatePages(-1));
        StartCoroutine(WaitOnSec());
        animator.enabled = true;
    }

    private IEnumerator RotatePages(int index)
    {
        flipper.flipDuration = 0.2f;
        while (pageIndex < index)
        {
            if (flipper.isRotating) yield return null; 
            HandleRightButtonClick();
            yield return new WaitUntil(() => !flipper.isRotating); 
        }

        while (pageIndex > index)
        {
            if (flipper.isRotating) yield return null;
            HandleLeftButtonClick();
            yield return new WaitUntil(() => !flipper.isRotating);
        }
        flipper.flipDuration = 0.8f;
    }

    private IEnumerator WaitForAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f) // Wait until animation is 100% finished
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        animator.SetBool("closeBook", true);
        animator.enabled = false;
    }

    private IEnumerator WaitOnSec()
    {
        yield return new WaitForSeconds(1);
    }

}
