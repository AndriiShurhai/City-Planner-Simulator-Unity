using UnityEngine;

public class TileVisualEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Vector3 originalPosition;
    public float timeOffset;

    public float colorPulseSpeed = 2f;
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.1f;
    public Color pulseColor = Color.cyan;

    [SerializeField] public Color baseColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        baseColor = spriteRenderer.color;
        pulseColor = spriteRenderer .color;
        timeOffset = Random.Range(0f, 2f); // To avoid uniform movement
    }

    void Update()
    {
        float yOffset = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmplitude;
        transform.position = originalPosition + new Vector3(0, yOffset, 0);

        float pulse = Mathf.Sin((Time.time + timeOffset) * colorPulseSpeed) * 0.5f + 0.5f;
        spriteRenderer.color = Color.Lerp(baseColor, pulseColor, pulse);
    }

    public void ResetEffect()
    {
        spriteRenderer.color = baseColor;
        transform.position = originalPosition;
    }
}
