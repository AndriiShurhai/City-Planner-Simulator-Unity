using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public BuildingData[] shopItemsSO;
    public GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    public Button[] purchaseButtons;

    public void Start()
    {
        for (int i = 0; i < shopPanelsGO.Length; i++)
        {
            if (i < shopItemsSO.Length)
            {
                shopPanelsGO[i].SetActive(true);
            }
            else
            {
                shopPanelsGO[i].SetActive(false);
            }
        }
        LoadPanels();
        CheckPurchusable();
    }

    public void CheckPurchusable()
    {
        for (int i = 0; i < shopItemsSO.Length; i++)
        {
            if (EconomyManager.Instance.CurrentMoney >= shopItemsSO[i].cost)
            {
                purchaseButtons[i].interactable = true;
            }
            else
            {
                purchaseButtons[i].interactable = false;
            }
        }
    }
    public void LoadPanels()
    {
        for (int i = 0; i < shopItemsSO.Length; i++)
        {
            shopPanels[i].titleTxt.text = shopItemsSO[i].buildingName;
            shopPanels[i].descriptionTxt.text = shopItemsSO[i].buildingDescription;
            shopPanels[i].costTxt.text = shopItemsSO[i].cost.ToString();
        }
    }

    public void BuyShopItem(int buttonIndex)
    {
        Debug.Log(buttonIndex);
        if (EconomyManager.Instance.CurrentMoney >= shopItemsSO[buttonIndex].cost)
        {
            GridCity.Instance.SetActiveBuildingType(shopItemsSO[buttonIndex]);
            CheckPurchusable();
            SceneManagerController.Instance.LoadSampleScene();
        }
    }
}
