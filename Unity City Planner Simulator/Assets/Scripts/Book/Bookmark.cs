using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Bookmark : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform bookmarkTransform;
    [SerializeField] public int bookmarkIndex;
    [SerializeField] public Button bookmarkButton;
    private BookmarkAnimator bookmarkAnimator;

    private void Awake()
    {
        bookmarkAnimator = new BookmarkAnimator();

        if (bookmarkTransform == null)
        {
            bookmarkTransform = GetComponent<RectTransform>();
            Debug.LogWarning("BookmarkTransform was not assigned - using this GameObject's RectTransform instead.");
        }
    }

    public void GoLeft()
    {
        bookmarkAnimator.GoLeft(bookmarkTransform);
    }

    public void GoRight()
    {
        bookmarkAnimator.GoRight(bookmarkTransform);
    }

    public bool IsPlayingNow()
    {
        return bookmarkAnimator.IsPlayingNow() || bookmarkAnimator.IsMoving();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsPlayingNow())
        {
            bookmarkAnimator.HoverAnimation(bookmarkTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsPlayingNow())
        {
            bookmarkAnimator.UnHoverAnimation(bookmarkTransform);
        }
    }
}