using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public static class WindowManagerExtension
    {
       
        /// <summary>
        /// Creates a storage box UI window for the specified storage box.
        /// </summary>
        /// <param name="windowManager">The window manager instance</param>
        /// <param name="storageBox">The storage box to create UI for</param>
        /// <returns>The StorageBoxUI component attached to the window</returns>
        public static StorageBoxUI CreateStorageBoxWindow(this WindowManager windowManager, V2.Data.StorageBox storageBox)
        {
            // Create a consistent window ID based on the storage box's ID
            string windowId = "StorageBox_" + storageBox.ID;
            string title = "Storage Box";
            
            // Create base window
            RectTransform contentArea = windowManager.CreateWindow(windowId, title);
            GameObject windowObject = contentArea.transform.parent.gameObject;
            
            // Add StorageBoxUI component if it doesn't exist
            StorageBoxUI storageBoxUI = windowObject.GetComponent<StorageBoxUI>();
            if (storageBoxUI == null)
            {
                storageBoxUI = windowObject.AddComponent<StorageBoxUI>();
            }
            
            // Create UI elements for storage box
            CreateStorageBoxUIElements(contentArea, storageBoxUI);
            
            // Initialize the UI with the storage box
            storageBoxUI.Initialize(storageBox);
            
            return storageBoxUI;
        }
        
     
        private static void CreateStorageBoxUIElements(RectTransform contentArea, StorageBoxUI storageBoxUI)
        {
            // Clear existing content
            foreach (Transform child in contentArea)
            {
                Object.Destroy(child.gameObject);
            }
            
            // Create a vertical layout group for organizing elements
            VerticalLayoutGroup layoutGroup = contentArea.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.spacing = 10;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            
            // Add decorative header
            GameObject headerDecoration = new GameObject("HeaderDecoration");
            headerDecoration.transform.SetParent(contentArea, false);
            RectTransform headerRect = headerDecoration.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 4);
            Image headerImage = headerDecoration.AddComponent<Image>();
            headerImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            
            // Create title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(contentArea, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Storage Box";
            titleText.fontSize = 18;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.9f, 0.7f, 0.3f); // Brass/copper color
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 30);
            storageBoxUI.titleText = titleText;
            
            // Create slots container
            GameObject slotsContainerObj = new GameObject("SlotsContainer", typeof(RectTransform));
            slotsContainerObj.transform.SetParent(contentArea, false);
            RectTransform slotsContainerRect = slotsContainerObj.GetComponent<RectTransform>();
            slotsContainerRect.sizeDelta = new Vector2(0, 300);
            
            // Add grid layout for slots
            GridLayoutGroup gridLayout = slotsContainerObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(80, 80);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.padding = new RectOffset(5, 5, 5, 5);
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            
            // Create slot prefab
            GameObject slotPrefab = CreateSlotPrefab();
            storageBoxUI.slotPrefab = slotPrefab;
            storageBoxUI.slotsContainer = slotsContainerRect;
            
            // Create enabled toggle
            GameObject toggleObj = new GameObject("EnabledToggle", typeof(RectTransform));
            toggleObj.transform.SetParent(contentArea, false);
            RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(0, 30);
            
            // Add horizontal layout for toggle
            HorizontalLayoutGroup toggleLayout = toggleObj.AddComponent<HorizontalLayoutGroup>();
            toggleLayout.childAlignment = TextAnchor.MiddleCenter;
            toggleLayout.spacing = 10;
            toggleLayout.childControlWidth = false;
            toggleLayout.childForceExpandWidth = false;
            
            // Create toggle label
            GameObject toggleLabelObj = new GameObject("ToggleLabel", typeof(RectTransform));
            toggleLabelObj.transform.SetParent(toggleObj.transform, false);
            TextMeshProUGUI toggleLabel = toggleLabelObj.AddComponent<TextMeshProUGUI>();
            toggleLabel.text = "Enabled:";
            toggleLabel.fontSize = 14;
            toggleLabel.alignment = TextAlignmentOptions.Right;
            RectTransform toggleLabelRect = toggleLabelObj.GetComponent<RectTransform>();
            toggleLabelRect.sizeDelta = new Vector2(80, 30);
            
            // Create toggle
            GameObject toggleButtonObj = new GameObject("Toggle", typeof(RectTransform));
            toggleButtonObj.transform.SetParent(toggleObj.transform, false);
            Toggle toggle = toggleButtonObj.AddComponent<Toggle>();
            RectTransform toggleButtonRect = toggleButtonObj.GetComponent<RectTransform>();
            toggleButtonRect.sizeDelta = new Vector2(30, 30);
            
            // Create toggle background
            GameObject toggleBgObj = new GameObject("Background", typeof(RectTransform));
            toggleBgObj.transform.SetParent(toggleButtonObj.transform, false);
            Image toggleBg = toggleBgObj.AddComponent<Image>();
            toggleBg.color = new Color(0.2f, 0.2f, 0.2f);
            RectTransform toggleBgRect = toggleBgObj.GetComponent<RectTransform>();
            toggleBgRect.anchorMin = Vector2.zero;
            toggleBgRect.anchorMax = Vector2.one;
            toggleBgRect.sizeDelta = Vector2.zero;
            
            // Create toggle checkmark
            GameObject toggleCheckObj = new GameObject("Checkmark", typeof(RectTransform));
            toggleCheckObj.transform.SetParent(toggleBgObj.transform, false);
            Image toggleCheck = toggleCheckObj.AddComponent<Image>();
            toggleCheck.color = new Color(0.9f, 0.7f, 0.3f); // Brass/copper color
            RectTransform toggleCheckRect = toggleCheckObj.GetComponent<RectTransform>();
            toggleCheckRect.anchorMin = new Vector2(0.1f, 0.1f);
            toggleCheckRect.anchorMax = new Vector2(0.9f, 0.9f);
            toggleCheckRect.sizeDelta = Vector2.zero;
            
            // Set up toggle references
            toggle.graphic = toggleCheck;
            toggle.targetGraphic = toggleBg;
            storageBoxUI.enabledToggle = toggle;
            
            // Add decorative footer
            GameObject footerDecoration = new GameObject("FooterDecoration");
            footerDecoration.transform.SetParent(contentArea, false);
            RectTransform footerRect = footerDecoration.AddComponent<RectTransform>();
            footerRect.sizeDelta = new Vector2(0, 4);
            Image footerImage = footerDecoration.AddComponent<Image>();
            footerImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            
            // Set the UI panel reference
            storageBoxUI.uiPanel = contentArea.parent.gameObject;
        }
        
        
        private static GameObject CreateSlotPrefab()
        {
            GameObject slotObj = new GameObject("SlotPrefab", typeof(RectTransform));
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(80, 80);
            
            // Add background image
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add item icon
            GameObject iconObj = new GameObject("ItemIcon", typeof(RectTransform));
            iconObj.transform.SetParent(slotObj.transform, false);
            Image itemIcon = iconObj.AddComponent<Image>();
            itemIcon.color = new Color(1, 1, 1, 0.2f); // Transparent white by default
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            
            // Add item type text
            GameObject typeObj = new GameObject("ItemTypeText", typeof(RectTransform));
            typeObj.transform.SetParent(slotObj.transform, false);
            TextMeshProUGUI itemTypeText = typeObj.AddComponent<TextMeshProUGUI>();
            itemTypeText.text = "Empty";
            itemTypeText.fontSize = 10;
            itemTypeText.alignment = TextAlignmentOptions.Center;
            RectTransform typeRect = typeObj.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.15f);
            typeRect.anchorMax = new Vector2(1, 0.3f);
            typeRect.sizeDelta = Vector2.zero;
            
            // Add count text
            GameObject countObj = new GameObject("CountText", typeof(RectTransform));
            countObj.transform.SetParent(slotObj.transform, false);
            TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
            countText.text = "0/99";
            countText.fontSize = 10;
            countText.alignment = TextAlignmentOptions.Center;
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0, 0);
            countRect.anchorMax = new Vector2(1, 0.15f);
            countRect.sizeDelta = Vector2.zero;
            
            // Add take button
            GameObject buttonObj = new GameObject("TakeButton", typeof(RectTransform));
            buttonObj.transform.SetParent(slotObj.transform, false);
            Button takeButton = buttonObj.AddComponent<Button>();
            takeButton.interactable = false; // Disabled by default for empty slots
            
            // Add button image
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.9f, 0.7f, 0.3f, 0.5f); // Brass/copper color with transparency
            
            // Set button position and size
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.7f, 0.7f);
            buttonRect.anchorMax = new Vector2(0.95f, 0.95f);
            buttonRect.sizeDelta = Vector2.zero;
            
            // Add button text
            GameObject buttonTextObj = new GameObject("Text", typeof(RectTransform));
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Take";
            buttonText.fontSize = 8;
            buttonText.alignment = TextAlignmentOptions.Center;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            
            // Set up button colors
            ColorBlock colors = takeButton.colors;
            colors.normalColor = new Color(0.9f, 0.7f, 0.3f, 0.5f);
            colors.highlightedColor = new Color(0.9f, 0.7f, 0.3f, 0.8f);
            colors.pressedColor = new Color(0.7f, 0.5f, 0.2f, 0.8f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            takeButton.colors = colors;
            
            // Don't destroy the prefab when a new scene is loaded
            Object.DontDestroyOnLoad(slotObj);
            
            // Hide the prefab initially
            slotObj.SetActive(false);
            
            return slotObj;
        }
    }
}