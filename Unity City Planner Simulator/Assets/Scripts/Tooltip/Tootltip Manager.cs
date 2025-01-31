using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    public static TooltipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector3 position = Input.mousePosition;
        position.x += 5;
        position.y += 5;
        transform.position = position;
    }

    public void SetAndShowTooltip(string message)
    {
        gameObject.SetActive(true);
        textComponent.text = message;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
        textComponent.text = string.Empty;
    }
}
