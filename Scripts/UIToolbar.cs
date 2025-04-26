using UnityEngine;
using UnityEngine.UI;

public class UIToolbar : MonoBehaviour
{
    [Header("References")]
    public BuildingManager buildingManager;
    
    [Header("Buttons")]
    public Button minerButton;
    public Button storageBoxButton; // Added storage box button reference
    
    private void Start()
    {
        // Set up button listeners if not already done in BuildingManager
        if (minerButton != null && buildingManager != null)
        {
            minerButton.onClick.AddListener(() => buildingManager.SelectBuilding(BuildingManager.BuildingType.Miner));
        }
        
        // Set up storage box button listener
        if (storageBoxButton != null && buildingManager != null)
        {
            storageBoxButton.onClick.AddListener(() => buildingManager.SelectBuilding(BuildingManager.BuildingType.StorageBox));
        }
    }
}