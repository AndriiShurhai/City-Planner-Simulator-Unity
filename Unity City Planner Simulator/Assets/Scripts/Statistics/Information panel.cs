using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InformationPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public GameObject[] pages;
    public int pageIndex = 0;
    public Button turnBackButton;
    public Button turnForwardButton;
    public RectTransform dragPanel;
    public RectTransform panelRect;

    private Vector2 offset;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panelRect == null || panelRect.parent == null)
            return;

        Vector2 localPoint;
        RectTransform parentRect = panelRect.parent as RectTransform;

        // Convert the screen point to a local point in the parent's space.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            // Move the panel by setting its anchoredPosition.
            panelRect.anchoredPosition = localPoint - offset;
        }
    }
    public void TurnPageDown()
    {
        pageIndex -= 1;
        if (pageIndex < 0)
        {
            pageIndex = pages.Length - 1;
        }
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        pages[pageIndex].SetActive(true);
    }

    public void TurnPageUp()
    {
        pageIndex += 1;
        if (pageIndex > pages.Length - 1)
        {
            pageIndex = 0;
        }
        
        for (int i = 0; i < pages.Length;i++)
        {
            pages[i].SetActive(false);
        }
        pages[pageIndex].SetActive(true);
    }

    public void ClosePanel()
    { 
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        pages[0].SetActive(true);
        pageIndex = 0;

        gameObject.SetActive(true);
    }
}