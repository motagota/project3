using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public class MachineUIManager : MonoBehaviour
    {
        [Header("UI Panel References")]
        [SerializeField] private GameObject machineUIPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI inputItemsText;
        [SerializeField] private TextMeshProUGUI outputItemText;
        [SerializeField] private TextMeshProUGUI completedRecipesText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Toggle enabledToggle;
        [SerializeField] private Button closeButton;
        
        private MachineUI machineUI;
        
        private void Awake()
        {
            // Make sure the panel is hidden at start
            if (machineUIPanel != null)
            {
                machineUIPanel.SetActive(false);
            }
            
            // Add listener to the close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HidePanel);
            }
            
            // Create and initialize the MachineUI component
            machineUI = gameObject.AddComponent<MachineUI>();
            InitializeMachineUI();
        }
        
        private void InitializeMachineUI()
        {
            // Get the MachineUI component reference
            if (machineUI == null)
            {
                machineUI = GetComponent<MachineUI>();
                if (machineUI == null)
                {
                    Debug.LogError("MachineUI component not found!");
                    return;
                }
            }
            
            // Set the UI references in the MachineUI component using reflection
            var uiType = machineUI.GetType();
            
            SetPrivateField(uiType, "uiPanel", machineUIPanel);
            SetPrivateField(uiType, "titleText", titleText);
            SetPrivateField(uiType, "recipeNameText", recipeNameText);
            SetPrivateField(uiType, "inputItemsText", inputItemsText);
            SetPrivateField(uiType, "outputItemText", outputItemText);
            SetPrivateField(uiType, "completedRecipesText", completedRecipesText);
            SetPrivateField(uiType, "progressBar", progressBar);
            SetPrivateField(uiType, "enabledToggle", enabledToggle);
        }
        
        private void SetPrivateField(System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(machineUI, value);
            }
            else
            {
                Debug.LogWarning($"Field {fieldName} not found in MachineUI!");
            }
        }
        
        private void HidePanel()
        {
            if (machineUIPanel != null)
            {
                machineUIPanel.SetActive(false);
            }
        }
    }
}