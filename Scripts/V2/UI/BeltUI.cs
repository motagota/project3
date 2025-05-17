using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;
using System.Collections.Generic;

namespace V2.UI
{
    public class BeltUI : MonoBehaviour
    {
        // These fields will be set by WindowManager when UI is created
        [HideInInspector] public GameObject uiPanel;
        [HideInInspector] public TextMeshProUGUI titleText;
        [HideInInspector] public TextMeshProUGUI nextBeltText;
        [HideInInspector] public TextMeshProUGUI previousBeltText;
        [HideInInspector] public TextMeshProUGUI itemsCountText;
        [HideInInspector] public TextMeshProUGUI itemsListText;
        
        private BeltData _currentBelt;
        private SimulationManagerV2 _simulationManager;
        
        private void Awake()
        {
            _simulationManager = FindObjectOfType<SimulationManagerV2>();
        }
        
        private void Update()
        {
            if (_currentBelt != null)
            {
                UpdateUI(); // Update UI every frame to reflect real-time changes
            }
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckForBeltSelection();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _currentBelt != null)
            {
                HideUI();
            }
        }
        
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        private void CheckForBeltSelection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            ChunkData chunk = _simulationManager.GetChunk(chunkCoord);
            
            if (chunk != null)
            {
                BeltData belt = chunk.GetBeltAt(gridPos);
                if (belt != null)
                {
                    // Use WindowManager to create/show the belt UI
                    WindowManager.Instance.CreateBeltWindow(belt);
                }
            }
        }
        
        public void SelectBelt(BeltData belt)
        {
            _currentBelt = belt;
            
            if (titleText != null)
            {
                titleText.text = "Belt";
            }
            
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (_currentBelt == null) return;
            
            if (titleText != null)
            {
                titleText.text = "Belt";
            }
            
            if (nextBeltText != null)
            {
                BeltData nextBelt = _currentBelt.GetNextBelt();
                string nextBeltStr = "Next Belt: ";
                if (nextBelt != null)
                {
                    nextBeltStr += "ID: " + nextBelt.ID;
                }
                else
                {
                    nextBeltStr += "None";
                }
                nextBeltText.text = nextBeltStr;
            }
            
            if (previousBeltText != null)
            {
                BeltData previousBelt = _currentBelt.GetPreviousBelt();
                string previousBeltStr = "Previous Belt: ";
                if (previousBelt != null)
                {
                    previousBeltStr += "ID: " + previousBelt.ID;
                }
                else
                {
                    previousBeltStr += "None";
                }
                previousBeltText.text = previousBeltStr;
            }
            
            if (itemsCountText != null)
            {
                Dictionary<SimulationItem, float> items = _currentBelt.GetAllItemsWithProgress();
                itemsCountText.text = $"Items: {items.Count}";
            }
            
            if (itemsListText != null)
            {
                Dictionary<SimulationItem, float> items = _currentBelt.GetAllItemsWithProgress();
                string itemsStr = "";
                
                if (items.Count == 0)
                {
                    itemsStr = "No items on belt";
                }
                else
                {
                    foreach (var kvp in items)
                    {
                        SimulationItem item = kvp.Key;
                        float progress = kvp.Value;
                        itemsStr += $"{item.ItemType} - Progress: {progress:P0}\n";
                    }
                }
                
                itemsListText.text = itemsStr;
            }
        }
        
        private void HideUI()
        {
            if (_currentBelt != null)
            {
                string windowId = "Belt_" + _currentBelt.ID;
                WindowManager.Instance.CloseWindow(windowId);
                _currentBelt = null;
            }
        }
    }
}