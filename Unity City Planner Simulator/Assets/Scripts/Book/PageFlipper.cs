using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class PageFlipper 
{
    public float flipDuration = 0.8f;
    public bool isRotating = false;

    public void Rotate(RectTransform page, float targetAngle)
    {
        if (isRotating || Mathf.Approximately(page.localEulerAngles.y, targetAngle)) return;
        isRotating = true;

        // Ensure the rotation is always 0 or 180
        float startAngle = Mathf.Round(page.localEulerAngles.y);

        // Tween rotation
        page.DOLocalRotate(new Vector3(0, targetAngle, 0), flipDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => isRotating = false);
    }
}
