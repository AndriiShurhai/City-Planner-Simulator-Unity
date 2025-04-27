using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private BuildingData[] _shopItemsSO;
    [SerializeField] private GameObject[] _shopPanelsGO;
    [SerializeField] private ShopTemplate[] _shopPanels;
    [SerializeField] private Button[] _purchaseButtons;
    [SerializeField] private BuildingInfoTemplate[] _buildingInfoTemplates;

    private void Awake()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged += HandleMoneyChange;
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged -= HandleMoneyChange;
        }
    }
    private void Start()
    {
        for (int i = 0; i < _shopPanelsGO.Length; i++)
        {
            _shopPanelsGO[i].SetActive(i < _shopItemsSO.Length);
        }
        LoadPanels();
        LoadInfoPanels();
        CheckPurchasable();
    }

    private void HandleMoneyChange()
    {
        try
        {
            CheckPurchasable();
        }
        catch
        {
            return;
        }
    }
    private void CheckPurchasable()
    {
        for (int i = 0; i < _shopItemsSO.Length; i++)
        {
            _purchaseButtons[i].interactable = EconomyManager.Instance.CanAfford(_shopItemsSO[i].cost);
        }
    }
    private void LoadPanels()
    {
        for (int i = 0; i < _shopItemsSO.Length; i++)
        {
            _shopPanels[i].titleTxt.text = _shopItemsSO[i].buildingName;
            _shopPanels[i].descriptionTxt.text = _shopItemsSO[i].buildingDescription;
            _shopPanels[i].costTxt.text = _shopItemsSO[i].cost.ToString();
            _shopPanels[i].imageItem.sprite = _shopItemsSO[i].buildingSprite;
            int index = i;
            _shopPanels[i].infoButton.onClick.AddListener(() => HandleInfoButtonClick(index));
            AdjustSize(_shopPanels[i].imageItem);
        }
    }

    private void LoadInfoPanels()
    {
        for (int i = 0; i < _buildingInfoTemplates.Length; i++)
        {
            _buildingInfoTemplates[i].titleTxt.text = _shopItemsSO[i].buildingName;
            _buildingInfoTemplates[i].descriptionTxt.text = _shopItemsSO[i].buildingDescription;
            _buildingInfoTemplates[i].imageItem.sprite = _shopItemsSO[i].buildingSprite;
            AdjustSize(_buildingInfoTemplates[i].imageItem);
        }
    }
    public void BuyShopItem(int buttonIndex)
    {

        if (buttonIndex < 0 || buttonIndex >= _shopItemsSO.Length)
        {
            Debug.LogWarning($"The index of button is outside of range: {buttonIndex}");
            return;
        }

        BuildingData selectedBuilding = _shopItemsSO[buttonIndex];
        int buildingCost = selectedBuilding.cost;
        
        if (EconomyManager.Instance.CanAfford(buildingCost))
        {
            GridCity.Instance.SetActiveBuildingType(_shopItemsSO[buttonIndex]);
            SceneManagerController.Instance.CloseShopScene();
        }
    }

    void AdjustSize(Image imageItem)
    {
        if (imageItem.sprite == null)
            return;

        float width = imageItem.sprite.bounds.size.x;
        float height = imageItem.sprite.bounds.size.y;
        float maxDimension = Mathf.Max(width, height);

        float targetSize = 400f;

        float scale = targetSize / maxDimension;

        imageItem.rectTransform.sizeDelta = new Vector2(width * scale, height * scale);
    }

    public void HandleInfoButtonClick(int buttonIndex)
    {
        Debug.Log($"Button {buttonIndex} clicked");

        foreach (var item in _buildingInfoTemplates)
        {
            item.CloseBuildingInfo();
        }
        _buildingInfoTemplates[buttonIndex].OpenBuildingInfo();
    }
}
