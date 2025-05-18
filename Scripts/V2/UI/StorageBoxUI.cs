using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public class StorageBoxUI : MonoBehaviour
    {
        // These fields will be set by WindowManagerExtension when UI is created
        [HideInInspector] public GameObject uiPanel;
        [HideInInspector] public TextMeshProUGUI titleText;
        [HideInInspector] public Transform slotsContainer;
        [HideInInspector] public GameObject slotPrefab;
        [HideInInspector] public Toggle enabledToggle;
        
        private V2.Data.StorageBox _currentStorageBox;
        private List<SlotUI> _slotUIs = new List<SlotUI>();
        
        // Class to manage individual slot UI
        [System.Serializable]
        public class SlotUI
        {
            public GameObject slotObject;
            public TextMeshProUGUI itemTypeText;
            public TextMeshProUGUI countText;
            public Image itemIcon;
            public Button takeButton;
            
            public void UpdateUI(InventorySlot slot)
            {
                if (slot.IsEmpty)
                {
                    itemTypeText.text = "Empty";
                    countText.text = "0/" + slot.MaxStackSize;
                    itemIcon.color = new Color(1, 1, 1, 0.2f); // Transparent white
                    takeButton.interactable = false;
                }
                else
                {
                    itemTypeText.text = slot.ItemType;
                    countText.text = slot.Count + "/" + slot.MaxStackSize;
                    
                    // Try to get color from item definition
                    var item = slot.PeekItem();
                    if (item != null)
                    {
                        itemIcon.color = item.ItemColor;
                    }
                    else
                    {
                        itemIcon.color = Color.white;
                    }
                    
                    takeButton.interactable = true;
                }
            }
        }
        
        private void Start()
        {
            // Add listener to the toggle if it exists
            if (enabledToggle != null)
            {
                enabledToggle.onValueChanged.AddListener(OnEnabledToggleChanged);
            }
        }
        
        public void Initialize(V2.Data.StorageBox storageBox)
        {
            _currentStorageBox = storageBox;
            
            // Subscribe to events
            storageBox.OnItemAdded += OnItemChanged;
            storageBox.OnItemRemoved += OnItemChanged;
            storageBox.OnEnabledStateChanged += OnEnabledStateChanged;
            
            // Set toggle state
            if (enabledToggle != null)
            {
                enabledToggle.isOn = storageBox.IsEnabled;
            }
            
            // Create slot UIs
            RefreshSlotUIs();
            
            UpdateUI();
        }
        
        private void RefreshSlotUIs()
        {
            if (_currentStorageBox == null || slotsContainer == null || slotPrefab == null)
                return;
                
            // Clear existing slot UIs
            foreach (var slotUI in _slotUIs)
            {
                if (slotUI.slotObject != null)
                {
                    Destroy(slotUI.slotObject);
                }
            }
            _slotUIs.Clear();
            
            // Create slot UIs
            List<InventorySlot> slots = _currentStorageBox.GetSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                CreateSlotUI(i, slots[i]);
            }
        }
        
      private void CreateSlotUI(int index, InventorySlot slot)
{
    if (slotPrefab == null || slotsContainer == null)
        return;
        
    GameObject slotObject = Instantiate(slotPrefab, slotsContainer);
    slotObject.SetActive(true); // Activate the slot object
    
    SlotUI slotUI = new SlotUI
    {
        slotObject = slotObject,
        itemTypeText = slotObject.transform.Find("ItemTypeText")?.GetComponent<TextMeshProUGUI>(),
        countText = slotObject.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>(),
        itemIcon = slotObject.transform.Find("ItemIcon")?.GetComponent<Image>(),
        takeButton = slotObject.transform.Find("TakeButton")?.GetComponent<Button>()
    };
    
    // Set up take button
    if (slotUI.takeButton != null)
    {
        int slotIndex = index; // Capture for lambda
        slotUI.takeButton.onClick.AddListener(() => OnTakeButtonClicked(slotIndex));
    }
    
    _slotUIs.Add(slotUI);
    slotUI.UpdateUI(slot);
}
        
        private void OnTakeButtonClicked(int slotIndex)
        {
            if (_currentStorageBox == null || slotIndex < 0 || slotIndex >= _currentStorageBox.GetSlots().Count)
                return;
                
            // Here you would implement logic to take the item and give it to the player
            // For now, we'll just log it
            Debug.Log($"Taking item from slot {slotIndex}");
            
            // Example: If you have a player inventory system
            // PlayerInventory.Instance.AddItem(_currentStorageBox.TakeItemFromSlot(slotIndex));
            
            UpdateUI();
        }
        
        private void OnEnabledToggleChanged(bool isEnabled)
        {
            if (_currentStorageBox != null)
            {
                _currentStorageBox.IsEnabled = isEnabled;
            }
        }
        
        private void OnEnabledStateChanged(V2.Data.StorageBox storageBox)
        {
            if (enabledToggle != null)
            {
                enabledToggle.isOn = storageBox.IsEnabled;
            }
        }
        
        private void OnItemChanged(V2.Data.StorageBox storageBox, SimulationItem item)
        {
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            if (_currentStorageBox == null)
                return;
                
            List<InventorySlot> slots = _currentStorageBox.GetSlots();
            for (int i = 0; i < slots.Count && i < _slotUIs.Count; i++)
            {
                _slotUIs[i].UpdateUI(slots[i]);
            }
        }
        
        private SimulationManagerV2 _simulationManager;
        
        private void Awake()
        {
            _simulationManager = FindObjectOfType<SimulationManagerV2>();
        }
        
        private void Update()
        {
            UpdateUI();
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckForStorageBoxSelection();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _currentStorageBox != null)
            {
                HideUI();
            }
        }
        
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        private void CheckForStorageBoxSelection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            ChunkData chunk = _simulationManager.GetChunk(chunkCoord);
            
            if (chunk != null)
            {
                V2.Data.StorageBox storageBox = chunk.GetStorageBoxAt(gridPos);
                if (storageBox != null)
                {
                    // Use WindowManager to create/show the storage box UI
                    WindowManager.Instance.CreateStorageBoxWindow(storageBox);
                }
            }
        }
        
        private void HideUI()
        {
            if (_currentStorageBox != null)
            {
                string windowId = "StorageBox_" + _currentStorageBox.ID;
                WindowManager.Instance.CloseWindow(windowId);
                _currentStorageBox = null;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_currentStorageBox != null)
            {
                _currentStorageBox.OnItemAdded -= OnItemChanged;
                _currentStorageBox.OnItemRemoved -= OnItemChanged;
                _currentStorageBox.OnEnabledStateChanged -= OnEnabledStateChanged;
            }
        }
    }
}