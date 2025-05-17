using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public class ConnectorUI : MonoBehaviour
    {
        // These fields will be set by WindowManager when UI is created
        [HideInInspector] public GameObject uiPanel;
        [HideInInspector] public TextMeshProUGUI titleText;
        [HideInInspector] public TextMeshProUGUI inputConnectionText;
        [HideInInspector] public TextMeshProUGUI outputConnectionText;
        [HideInInspector] public TextMeshProUGUI heldItemText;
        [HideInInspector] public TextMeshProUGUI statusText;
        
        private Connector _currentConnector;
        private SimulationManagerV2 _simulationManager;
        
        private void Awake()
        {
            _simulationManager = FindObjectOfType<SimulationManagerV2>();
        }
        
        private void Update()
        {
            if (_currentConnector != null)
            {
                UpdateUI(); // Update UI every frame to reflect real-time changes
            }
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckForConnectorSelection();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _currentConnector != null)
            {
                HideUI();
            }
        }
        
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        private void CheckForConnectorSelection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            ChunkData chunk = _simulationManager.GetChunk(chunkCoord);
            
            if (chunk != null)
            {
                // Check if there's a connector at the clicked position
                // This would need to be implemented in ChunkData
                Connector connector = chunk.GetConnectorAt(gridPos);
                if (connector != null)
                {
                    // Use WindowManager to create/show the connector UI
                    WindowManager.Instance.CreateConnectorWindow(connector);
                }
            }
        }
        
        public void SelectConnector(Connector connector)
        {
            _currentConnector = connector;
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (_currentConnector == null) return;
            
            if (titleText != null)
            {
                titleText.text = "Connector";
            }
            
            if (inputConnectionText != null)
            {
                Entity inputEntity = _currentConnector.GetInputConnectedMachine();
                string inputText = "Input Connection: ";
                if (inputEntity != null)
                {
                    inputText += inputEntity.GetType().Name + " (ID: " + inputEntity.ID + ")";
                }
                else
                {
                    inputText += "None";
                }
                inputConnectionText.text = inputText;
            }
            
            if (outputConnectionText != null)
            {
                Entity outputEntity = _currentConnector.GetOutputConnectedMachine();
                string outputText = "Output Connection: ";
                if (outputEntity != null)
                {
                    outputText += outputEntity.GetType().Name + " (ID: " + outputEntity.ID + ")";
                }
                else
                {
                    outputText += "None";
                }
                outputConnectionText.text = outputText;
            }
            
            if (heldItemText != null)
            {
                SimulationItem heldItem = _currentConnector.GetHeldItem();
                string heldItemStr = "Held Item: ";
                if (heldItem != null)
                {
                    heldItemStr += heldItem.ItemType;
                }
                else
                {
                    heldItemStr += "None";
                }
                heldItemText.text = heldItemStr;
            }
            
            if (statusText != null)
            {
                string statusStr = "Status: ";
                if (_currentConnector.HasInputItem)
                {
                    if (_currentConnector.CanDropItem)
                    {
                        statusStr += "Transferring";
                    }
                    else
                    {
                        statusStr += "Blocked (Output Full)";
                    }
                }
                else
                {
                    statusStr += "Waiting for Input";
                }
                statusText.text = statusStr;
            }
        }
        
        private void HideUI()
        {
            if (_currentConnector != null)
            {
                string windowId = "Connector_" + _currentConnector.ID;
                WindowManager.Instance.CloseWindow(windowId);
                _currentConnector = null;
            }
        }
    }
}