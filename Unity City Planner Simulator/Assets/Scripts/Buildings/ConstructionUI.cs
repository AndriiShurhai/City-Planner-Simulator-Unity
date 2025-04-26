using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstructionUI : MonoBehaviour
{
    private Building building;
    private Image progressBarFill;
    private TextMeshProUGUI progressText;
    private float constructionStartTime;
    private float constructionDuration;

    public void Initialize(Building building)
    {
        this.building = building;
        constructionStartTime = Time.time;
        constructionDuration = building.BuildingData.constructionDuration;

        progressBarFill = transform.Find("ProgressBarBackground/ProgressBarFill")?.GetComponent<Image>();
        progressText = transform.Find("ProgressBarBackground/ProgressText")?.GetComponent<TextMeshProUGUI>();

        if (progressBarFill == null)
        {
            Debug.LogError("ConstructionUI missing required components.");
            Destroy(gameObject);
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (building == null || building.State != BuildingState.Constructing)
        {
            OnBuildingDestroyed();
            return;
        }

        transform.position = new Vector3(building.OccupiedPositions[0].x, building.OccupiedPositions[0].y, 0) + new Vector3((float)building.Size.x / 2f, -2, 0) * 1f;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (constructionDuration <= 0)
        {
            progressBarFill.fillAmount = 1f ;
            progressText.text = "Construction: 100%";
            return;
        }

        float elapsedTime = Time.time - constructionStartTime;
        float progress = Mathf.Clamp01(elapsedTime / constructionDuration);
        progressBarFill.fillAmount = progress;
        //progressText.text = $"Construction: {Mathf.RoundToInt(progress * 100)}%";
    }

    public void OnBuildingDestroyed()
    {
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}