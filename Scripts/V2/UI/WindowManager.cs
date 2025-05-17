using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using V2.Data;

namespace V2.UI
{
    /// <summary>
    /// WindowManager handles the creation and management of UI windows in the game.
    /// It provides a centralized system for generating UI elements procedurally.
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject windowPrefab;
        
        [Header("Default Settings")]
        [SerializeField] private Vector2 defaultWindowSize = new Vector2(300, 400);
        [SerializeField] private Color defaultWindowColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [SerializeField] private int defaultFontSize = 14;
        
        private Dictionary<string, GameObject> _activeWindows = new Dictionary<string, GameObject>();
        private static WindowManager _instance;
        
        public static WindowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WindowManager>();
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("WindowManager");
                        _instance = managerObject.AddComponent<WindowManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create main canvas if it doesn't exist
            if (mainCanvas == null)
            {
                GameObject canvasObject = new GameObject("MainCanvas");
                mainCanvas = canvasObject.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            
            // Create window prefab if it doesn't exist
            if (windowPrefab == null)
            {
                windowPrefab = CreateDefaultWindowPrefab();
            }
        }
        
        /// <summary>
        /// Creates a new UI window with the specified ID and title.
        /// </summary>
        /// <param name="windowId">Unique identifier for the window</param>
        /// <param name="title">Title to display on the window</param>
        /// <param name="position">Screen position for the window</param>
        /// <returns>The RectTransform of the window content area</returns>
        public RectTransform CreateWindow(string windowId, string title, Vector2 position = default)
        {
            // Check if window already exists
            if (_activeWindows.ContainsKey(windowId))
            {
                // Bring existing window to front
                _activeWindows[windowId].transform.SetAsLastSibling();
                return _activeWindows[windowId].transform.Find("Content").GetComponent<RectTransform>();
            }
            
            // Instantiate window from prefab
            GameObject windowObject = Instantiate(windowPrefab, mainCanvas.transform);
            windowObject.name = $"Window_{windowId}";
            
            // Set window position to center of screen by default
            RectTransform rectTransform = windowObject.GetComponent<RectTransform>();
            if (position == default)
            {
                // Center the window on screen
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero; // This centers it based on the anchors
            }
            else
            {
                rectTransform.anchoredPosition = position;
            }
            
            // Set window title
            TextMeshProUGUI titleText = windowObject.transform.Find("TitleBar/TitleText").GetComponent<TextMeshProUGUI>();
            titleText.text = title;
            
            // Setup close button
            Button closeButton = windowObject.transform.Find("TitleBar/CloseButton").GetComponent<Button>();
            closeButton.onClick.AddListener(() => CloseWindow(windowId));
            
            // Store reference to active window
            _activeWindows[windowId] = windowObject;
            
            // Return content area for further customization
            return windowObject.transform.Find("Content").GetComponent<RectTransform>();
        }
        
        /// <summary>
        /// Creates a machine UI window for the specified machine.
        /// </summary>
        /// <param name="machine">The machine to create UI for</param>
        /// <returns>The MachineUI component attached to the window</returns>
        public MachineUI CreateMachineWindow(Machine machine)
        {
            // Use the helper method to get a consistent window ID
            string windowId = MachineSelectionManager.GetMachineWindowId(machine);
            string title = machine.GetType().Name;
            
            // Create base window
            RectTransform contentArea = CreateWindow(windowId, title);
            GameObject windowObject = contentArea.transform.parent.gameObject;
            
            // Add MachineUI component if it doesn't exist
            MachineUI machineUI = windowObject.GetComponent<MachineUI>();
            if (machineUI == null)
            {
                machineUI = windowObject.AddComponent<MachineUI>();
            }
            
            // Create UI elements for machine
            CreateMachineUIElements(contentArea, machineUI);
            
            // Initialize the UI with the machine
            machineUI.SelectMachine(machine);
            
            return machineUI;
        }
        
        /// <summary>
        /// Creates the UI elements for a machine window.
        /// </summary>
        /// <param name="contentArea">The content area to add elements to</param>
        /// <param name="machineUI">The MachineUI component to reference</param>
        private void CreateMachineUIElements(RectTransform contentArea, MachineUI machineUI)
        {
            // Clear existing content
            foreach (Transform child in contentArea)
            {
                Destroy(child.gameObject);
            }
            
            // Create vertical layout group
            VerticalLayoutGroup layoutGroup = contentArea.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.spacing = 10;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            // Add decorative header
            GameObject headerDecoration = new GameObject("HeaderDecoration");
            headerDecoration.transform.SetParent(contentArea, false);
            RectTransform headerRect = headerDecoration.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 4);
            Image headerImage = headerDecoration.AddComponent<Image>();
            headerImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            
            // Create recipe name text
            TextMeshProUGUI recipeNameText = CreateTextElement(contentArea, "RecipeNameText", "Recipe: ");
            machineUI.recipeNameText = recipeNameText;
            
            // Create input items text
            TextMeshProUGUI inputItemsText = CreateTextElement(contentArea, "InputItemsText", "Inputs: ");
            machineUI.inputItemsText = inputItemsText;
            
            // Create output item text
            TextMeshProUGUI outputItemText = CreateTextElement(contentArea, "OutputItemText", "Output: ");
            machineUI.outputItemText = outputItemText;
            
            // Create completed recipes text
            TextMeshProUGUI completedRecipesText = CreateTextElement(contentArea, "CompletedRecipesText", "Completed: 0");
            machineUI.completedRecipesText = completedRecipesText;
            
            // Create progress bar
            Slider progressBar = CreateProgressBar(contentArea, "ProgressBar");
            machineUI.progressBar = progressBar;
            
            // Create enabled toggle
            Toggle enabledToggle = CreateToggle(contentArea, "EnabledToggle", "Enabled");
            machineUI.enabledToggle = enabledToggle;
            
            // Add decorative footer
            GameObject footerDecoration = new GameObject("FooterDecoration");
            footerDecoration.transform.SetParent(contentArea, false);
            RectTransform footerRect = footerDecoration.AddComponent<RectTransform>();
            footerRect.sizeDelta = new Vector2(0, 4);
            Image footerImage = footerDecoration.AddComponent<Image>();
            footerImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            
            // Set the UI panel reference
            machineUI.uiPanel = contentArea.parent.gameObject;
        }
        
        /// <summary>
        /// Creates a text UI element.
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="name">Element name</param>
        /// <param name="defaultText">Default text</param>
        /// <returns>The created TextMeshProUGUI component</returns>
        private TextMeshProUGUI CreateTextElement(Transform parent, string name, string defaultText)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 30);
            
            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = defaultText;
            textComponent.fontSize = defaultFontSize;
            textComponent.alignment = TextAlignmentOptions.Left;
            textComponent.enableWordWrapping = true;
            textComponent.enableVertexGradient = true;
            textComponent.colorGradient = new VertexGradient(
                new Color(0.0f, 0.9f, 0.9f, 1f), // Cyan top-left
                new Color(0.0f, 0.8f, 0.9f, 1f), // Cyan top-right
                new Color(0.0f, 0.7f, 0.8f, 1f), // Darker cyan bottom-left
                new Color(0.0f, 0.6f, 0.7f, 1f)  // Darker cyan bottom-right
            );
            
            return textComponent;
        }
        
        /// <summary>
        /// Creates a progress bar UI element.
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="name">Element name</param>
        /// <returns>The created Slider component</returns>
        private Slider CreateProgressBar(Transform parent, string name)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = sliderObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 30);
            
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            RectTransform backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0);
            backgroundRect.anchorMax = new Vector2(1, 1);
            backgroundRect.sizeDelta = Vector2.zero;
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0);
            fillAreaRect.anchorMax = new Vector2(1, 1);
            fillAreaRect.offsetMin = new Vector2(5, 5);
            fillAreaRect.offsetMax = new Vector2(-5, -5);
            
            // Create fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.0f, 0.9f, 0.8f, 1f); // Bright cyan
            
            slider.fillRect = fillRect;
            
            return slider;
        }
        
        /// <summary>
        /// Creates a toggle UI element.
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="name">Element name</param>
        /// <param name="label">Toggle label</param>
        /// <returns>The created Toggle component</returns>
        private Toggle CreateToggle(Transform parent, string name, string label)
        {
            GameObject toggleObject = new GameObject(name);
            toggleObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = toggleObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 30);
            
            // Create horizontal layout
            HorizontalLayoutGroup layoutGroup = toggleObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.spacing = 10;
            
            // Create toggle control
            GameObject toggleControl = new GameObject("Toggle");
            toggleControl.transform.SetParent(toggleObject.transform, false);
            RectTransform toggleRect = toggleControl.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(30, 30);
            
            Toggle toggle = toggleControl.AddComponent<Toggle>();
            toggle.isOn = true;
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(toggleControl.transform, false);
            RectTransform backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0);
            backgroundRect.anchorMax = new Vector2(1, 1);
            backgroundRect.sizeDelta = Vector2.zero;
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            
            // Create checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform, false);
            RectTransform checkmarkRect = checkmark.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.1f, 0.1f);
            checkmarkRect.anchorMax = new Vector2(0.9f, 0.9f);
            checkmarkRect.sizeDelta = Vector2.zero;
            Image checkmarkImage = checkmark.AddComponent<Image>();
            checkmarkImage.color = new Color(0.0f, 0.9f, 0.8f, 1f); // Bright cyan
            
            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = backgroundImage;
            
            // Create label
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(toggleObject.transform, false);
            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(100, 30);
            
            TextMeshProUGUI labelText = labelObject.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = defaultFontSize;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.enableVertexGradient = true;
            labelText.colorGradient = new VertexGradient(
                new Color(0.0f, 0.9f, 0.9f, 1f), // Cyan top-left
                new Color(0.0f, 0.8f, 0.9f, 1f), // Cyan top-right
                new Color(0.0f, 0.7f, 0.8f, 1f), // Darker cyan bottom-left
                new Color(0.0f, 0.6f, 0.7f, 1f)  // Darker cyan bottom-right
            );
            
            return toggle;
        }
        
        /// <summary>
        /// Closes the window with the specified ID.
        /// </summary>
        /// <param name="windowId">The ID of the window to close</param>
        public void CloseWindow(string windowId)
        {
            if (_activeWindows.TryGetValue(windowId, out GameObject window))
            {
                Destroy(window);
                _activeWindows.Remove(windowId);
            }
        }
        
        /// <summary>
        /// Closes all active windows.
        /// </summary>
        public void CloseAllWindows()
        {
            foreach (var window in _activeWindows.Values)
            {
                Destroy(window);
            }
            
            _activeWindows.Clear();
        }
        
        /// <summary>
        /// Creates a default window prefab.
        /// </summary>
        /// <returns>The created window prefab</returns>
        private GameObject CreateDefaultWindowPrefab()
        {
            // Create window object
            GameObject windowObject = new GameObject("WindowPrefab");
            RectTransform windowRect = windowObject.AddComponent<RectTransform>();
            windowRect.sizeDelta = defaultWindowSize;
            
            // Set anchors and pivot for center positioning
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            
            // Add window background
            Image windowImage = windowObject.AddComponent<Image>();
            windowImage.color = defaultWindowColor;
            
            // Add glow effect around window
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(windowObject.transform, false);
            RectTransform glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(10, 10); // Slightly larger than the window
            glowRect.anchoredPosition = Vector2.zero;
            Image glowImage = glow.AddComponent<Image>();
            glowImage.color = new Color(0.0f, 0.9f, 0.8f, 0.3f); // Cyan glow
            glowImage.sprite = null; // You would need to assign a glow sprite here
            glowImage.type = Image.Type.Sliced;
            glow.transform.SetAsFirstSibling(); // Put glow behind everything
            
            // Create title bar
            GameObject titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(windowObject.transform, false);
            RectTransform titleRect = titleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            
            // Add title bar background
            Image titleImage = titleBar.AddComponent<Image>();
            titleImage.color = new Color(0.5f, 0.3f, 0.1f, 1f); // Brass/copper color
            
            // Add drag functionality
            DragHandler dragHandler = titleBar.AddComponent<DragHandler>();
            
            // Add gear decoration to title bar
            GameObject gearDecoration = new GameObject("GearDecoration");
            gearDecoration.transform.SetParent(titleBar.transform, false);
            RectTransform gearRect = gearDecoration.AddComponent<RectTransform>();
            gearRect.anchorMin = new Vector2(0, 0.5f);
            gearRect.anchorMax = new Vector2(0, 0.5f);
            gearRect.pivot = new Vector2(0, 0.5f);
            gearRect.sizeDelta = new Vector2(30, 30);
            gearRect.anchoredPosition = new Vector2(5, 0);
            Image gearImage = gearDecoration.AddComponent<Image>();
            gearImage.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brass/copper color
            gearImage.sprite = null; // You would need to assign a gear sprite here
            
            // Create title text
            GameObject titleText = new GameObject("TitleText");
            titleText.transform.SetParent(titleBar.transform, false);
            RectTransform titleTextRect = titleText.AddComponent<RectTransform>();
            titleTextRect.anchorMin = new Vector2(0, 0);
            titleTextRect.anchorMax = new Vector2(1, 1);
            titleTextRect.offsetMin = new Vector2(40, 0); // Adjusted for gear decoration
            titleTextRect.offsetMax = new Vector2(-40, 0);
            
            TextMeshProUGUI titleTextComponent = titleText.AddComponent<TextMeshProUGUI>();
            titleTextComponent.text = "Window";
            titleTextComponent.fontSize = defaultFontSize + 2;
            titleTextComponent.alignment = TextAlignmentOptions.Left;
            titleTextComponent.enableVertexGradient = true;
            titleTextComponent.colorGradient = new VertexGradient(
                new Color(0.0f, 0.9f, 0.9f, 1f), // Cyan top-left
                new Color(0.0f, 0.8f, 0.9f, 1f), // Cyan top-right
                new Color(0.0f, 0.7f, 0.8f, 1f), // Darker cyan bottom-left
                new Color(0.0f, 0.6f, 0.7f, 1f)  // Darker cyan bottom-right
            );
            
            // Add close button
            GameObject closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(titleBar.transform, false);
            RectTransform closeButtonRect = closeButton.AddComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(1, 0.5f);
            closeButtonRect.anchorMax = new Vector2(1, 0.5f);
            closeButtonRect.pivot = new Vector2(1, 0.5f);
            closeButtonRect.anchoredPosition = new Vector2(-10, 0);
            closeButtonRect.sizeDelta = new Vector2(30, 30);
            
            Image closeButtonImage = closeButton.AddComponent<Image>();
            closeButtonImage.color = new Color(0.9f, 0.2f, 0.3f, 1f); // Bright red with neon tint
            
            Button closeButtonComponent = closeButton.AddComponent<Button>();
            closeButtonComponent.targetGraphic = closeButtonImage;
            ColorBlock colors = closeButtonComponent.colors;
            colors.highlightedColor = new Color(1f, 0.4f, 0.5f, 1f); // Brighter highlight
            colors.pressedColor = new Color(0.7f, 0.1f, 0.2f, 1f); // Darker when pressed
            closeButtonComponent.colors = colors;
            
            // Create content area
            GameObject content = new GameObject("Content");
            content.transform.SetParent(windowObject.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -40);
            
            // Add content background
            Image contentImage = content.AddComponent<Image>();
            contentImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f); // Dark blue-black
            
            // Add decorative border
            GameObject border = new GameObject("Border");
            border.transform.SetParent(windowObject.transform, false);
            RectTransform borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(0.6f, 0.4f, 0.2f, 0.5f); // Brass/copper color
            borderImage.sprite = null; // You would need to assign a border sprite here
            borderImage.type = Image.Type.Sliced;
            borderImage.fillCenter = false;
            border.transform.SetAsFirstSibling(); // Put border behind everything
            
            return windowObject;
        }
    }
    
    /// <summary>
    /// Helper component to handle dragging UI elements.
    /// </summary>
    public class DragHandler : MonoBehaviour, IDragHandler
    {
        public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.parent.position += (Vector3)eventData.delta;
        }
    }
}