using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
public class BookmarkAnimator
{
    private float yPosAnimationDuration = 0.2f;
    private float xPosAnimationDuration = 0.2f;
    private float xMoveDistance = 610f;
    private float yMoveDistance = 50f;
    private Vector2 originalPosition; 
    private bool isHovering = false;
    private Sequence currentSequence;
    private bool isMoving = false; 

    public void GoLeft(RectTransform buttonRect, float originalPositionY)
    {
        if (IsPlayingNow() || buttonRect == null) return;

        isMoving = true;

        originalPosition = buttonRect.anchoredPosition;

        currentSequence = DOTween.Sequence()
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x, originalPositionY - yMoveDistance),
                yPosAnimationDuration))
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x - xMoveDistance, originalPosition.y - yMoveDistance),
                xPosAnimationDuration))
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x - xMoveDistance, originalPositionY),
                yPosAnimationDuration))
            .OnComplete(() => {
                currentSequence = null;

                originalPosition = new Vector2(originalPosition.x - xMoveDistance, originalPositionY);
                isMoving = false;
            });
    }

    public void GoRight(RectTransform buttonRect, float originalPositionY)
    {
        if (IsPlayingNow() || buttonRect == null) return;

        isMoving = true;

        originalPosition = buttonRect.anchoredPosition;

        currentSequence = DOTween.Sequence()
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x, originalPositionY - yMoveDistance),
                yPosAnimationDuration))
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x + xMoveDistance, originalPosition.y - yMoveDistance),
                xPosAnimationDuration))
            .Append(buttonRect.DOAnchorPos(
                new Vector2(originalPosition.x + xMoveDistance, originalPositionY),
                yPosAnimationDuration))
            .OnComplete(() => {
                currentSequence = null;

                originalPosition = new Vector2(originalPosition.x + xMoveDistance, originalPositionY);
                isMoving = false;
            });
    }

    public void HoverAnimation(RectTransform buttonRect)
    {
        if (buttonRect == null || IsPlayingNow() || isMoving) return;

        if (!isHovering)
        {
            originalPosition = buttonRect.anchoredPosition;
            isHovering = true;
        }

        buttonRect.DOKill(false); 

        buttonRect.DOAnchorPos(
            new Vector2(originalPosition.x, originalPosition.y + 15),
            yPosAnimationDuration
        ).SetEase(Ease.OutQuad);
    }

    public void UnHoverAnimation(RectTransform buttonRect)
    {
        if (buttonRect == null || IsPlayingNow() || isMoving) return;

        buttonRect.DOKill(false); 

        buttonRect.DOAnchorPos(
            originalPosition,
            yPosAnimationDuration
        ).SetEase(Ease.OutQuad)
        .OnComplete(() => isHovering = false);
    }

    public bool IsPlayingNow() => currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying();

    public bool IsMoving() => isMoving;
}