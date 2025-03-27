using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeManager : MonoBehaviour
{
    public SpriteRenderer overlayRenderer;
    public Transform clock;
    public float timeSpeed = 1f;
    public float currentTime = 12f;

    private Color dayColor = new Color(1f, 1f, 1f, 0f);
    private Color nightColor = new Color(0f, 0f, 0.2f, 0.6f);

    private void Update()
    {
        currentTime += Time.deltaTime * timeSpeed;
        if (currentTime >= 24) currentTime -= 24f;

        float theta = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * currentTime / 12f);

        float clockAngle = (currentTime / 24f) * 360f;
        clock.rotation = Quaternion.Euler(0, 0, -clockAngle);

        overlayRenderer.color = Color.Lerp(dayColor, nightColor, theta);    
    }
}
