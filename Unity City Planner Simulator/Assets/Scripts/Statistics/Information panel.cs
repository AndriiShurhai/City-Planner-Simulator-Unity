using UnityEngine;
using UnityEngine.UI;

public class InformationPanel : MonoBehaviour
{
    public GameObject[] pages;
    public int pageIndex = 0;
    public Button turnBackButton;
    public Button turnForwardButton;

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