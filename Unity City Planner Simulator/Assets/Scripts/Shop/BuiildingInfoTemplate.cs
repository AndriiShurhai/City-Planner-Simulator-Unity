using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BuildingInfoTemplate : MonoBehaviour
{
    public TMP_Text titleTxt;
    public TMP_Text descriptionTxt;
    public TMP_Text costTxt;
    public Image imageItem;
    public Vector3 startPosition;
    public Button closeButton;

    public void Start()
    {
        startPosition = GetComponent<RectTransform>().anchoredPosition;

    }

    public void OpenBuildingInfo()
    {
        GetComponent<RectTransform>().DOAnchorPos(new Vector2(startPosition.x - 1500f, startPosition.y), 1f);
        closeButton.onClick.AddListener(() => CloseBuildingInfo());
    }

    public void CloseBuildingInfo()
    {
        GetComponent<RectTransform>().DOAnchorPos(startPosition, 1f);
    }
}
