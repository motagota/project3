using System;
using UnityEngine;
using V2.Data;

namespace V2.GameObjects
{
    public class ConnectorRenderer : MonoBehaviour
    {
        public GameObject inputConnector; 
        
        
        [Header("Visual Feedback")]
        public Renderer inputConnectorRenderer; 
        public Renderer outConnectorRenderer; 
    
        public Material inputNotConnectedMaterial; // Material when not connected
        public Material outputNotConnectedMaterial; // Material when not connected
        
        public Material connectedMaterial;
        public Material waitingForItemMaterial;
        public Material hasItemMaterial;
        
        private Connector _connectorData;
        
        // Emission properties for glowing effect
        private float _emissionIntensity = 1.0f;
        private const float PulseSpeed = 2.0f;
        private const float MaxEmission = 2.0f;

        public void Initialize(Connector connectorData)
        {
            _connectorData = connectorData;
            connectorData.OnConnectionChanged += OnConnectionChanged;

        }

        private void OnConnectionChanged(Connector connectorData, Entity machine)
        {
            if (connectorData.ID == _connectorData.ID)
            {
                //inputConnectorRenderer.gameObject.transform.position= new Vector3( machine.LocalPostion.x, 0, machine.LocalPostion.y);        
            }
        }

        private void Start()
        {  if (inputConnectorRenderer != null && inputNotConnectedMaterial != null)
            {
                inputConnectorRenderer.material = inputNotConnectedMaterial;
            }
            if (outConnectorRenderer != null && outputNotConnectedMaterial != null)
            {
                outConnectorRenderer.material = outputNotConnectedMaterial;
            }
            
            if (waitingForItemMaterial == null)
            {
                waitingForItemMaterial = new Material(connectedMaterial);
                waitingForItemMaterial.EnableKeyword("_EMISSION");
                waitingForItemMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 1.0f) * 1.5f);
            }
            
            if (hasItemMaterial == null)
            {
                hasItemMaterial = new Material(connectedMaterial);
                hasItemMaterial.EnableKeyword("_EMISSION");
                hasItemMaterial.SetColor("_EmissionColor", new Color(1.0f, 0.5f, 0.0f) * 1.5f); 
            }
        }

        public void Update()
        {

            if (_connectorData != null)
            {
                UpdateVisuals();
            }
            
            _emissionIntensity = 1.0f + Mathf.PingPong(Time.time * PulseSpeed, MaxEmission - 1.0f);
            
           
                Color baseColor = new Color(0.5f, 0.5f, 1.0f); 
                waitingForItemMaterial.SetColor("_EmissionColor", baseColor * _emissionIntensity);
            
        }

        private void UpdateVisuals()
        {
            bool inputIsConnected = _connectorData.GetInputConnectedMachine() != null;
            bool outputIsConnected = _connectorData.GetOutputConnectedMachine() != null;

            if (inputConnectorRenderer != null)
            {
                if (_connectorData.HasInputItem)
                {
                    inputConnectorRenderer.material = hasItemMaterial;
                }
                else
                {
                    inputConnectorRenderer.material =
                        inputIsConnected ? waitingForItemMaterial : inputNotConnectedMaterial;
                }
            }

            if (outConnectorRenderer != null)
            {
                if (_connectorData.CanDropItem)
                {
                    outConnectorRenderer.material =
                        outputIsConnected ? waitingForItemMaterial : outputNotConnectedMaterial;
                }
                else
                {
                   outConnectorRenderer.material = hasItemMaterial;
                }
            }
            
            
        }
    }
}