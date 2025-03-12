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
}
