using System.Collections.Generic;
using Simulation;
using UnityEngine;
using V2.Data;

namespace V2.GameObjects
{
    public class ChunkRenderer : MonoBehaviour
    {
        public GameObject machinePrefab;
        public GameObject connectorPrefab;
        public GameObject beltPrefab;
        public GameObject storeageBoxPrefab;
        
        private Vector2Int _coords;
        private SimulationManagerV2 _sim;
        private ChunkData _data;
        private Dictionary<Vector2Int, GameObject> _machineGOs = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _connectorGOs = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _beltGOs = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _sbGOs = new Dictionary<Vector2Int, GameObject>();

        public void Initialize(Vector2Int coords, SimulationManagerV2 sim)
        {
            _coords = coords;
            _sim = sim;
            _data = _sim.GetChunk(coords);
            MachineInit();
            BeltInit();
            ConnectorInit();
            StorageBoxInit();

        }

        private void StorageBoxInit()
        {
            foreach (V2.Data.StorageBox sb in _data.GetStorageBoxes())
            {
                GameObject sbGO = Instantiate(storeageBoxPrefab);
                sbGO.name = $"SB_{sb.ID}";
                sbGO.transform.position = new Vector3(sb.LocalPosition.x, 0, sb.LocalPosition.y);
                sbGO.transform.rotation =  Quaternion.Euler(0, sb.Rotation, 0);
                sbGO.transform.SetParent(transform);
                StorageBoxObject sbRenderer = sbGO.GetComponent<StorageBoxObject>();
                if (sbRenderer != null)
                {
                    sbRenderer.Initialize(sb.LocalPosition);
                }
                _sbGOs.Add(sb.LocalPosition, sbGO);
            }
        }
        

        private void BeltInit()
        {
            foreach (BeltData belt in _data.GetBelts())
            {
                GameObject beltGO = Instantiate(beltPrefab);
                beltGO.name = $"Belt_{belt.ID}";
                beltGO.transform.position = new Vector3(belt.LocalPosition.x, 0, belt.LocalPosition.y);
                beltGO.transform.rotation =  Quaternion.Euler(0, belt.Rotation, 0);
                beltGO.transform.SetParent(transform);
                BeltRenderer beltRenderer = beltGO.GetComponent<BeltRenderer>();
                if (beltRenderer != null)
                {
                    beltRenderer.Initialize(belt);
                }
                _beltGOs.Add(belt.LocalPosition, beltGO);
            }
        }

        private void ConnectorInit()
        {
            foreach (Connector connector in _data.GetConnectors())
            {
                GameObject connectorGO = Instantiate(connectorPrefab);
                connectorGO.name = $"Connector_{connector.ID}";
                connectorGO.transform.position = new Vector3(connector.LocalPosition.x, 0, connector.LocalPosition.y);
                connectorGO.transform.rotation =  Quaternion.Euler(0, connector.Rotation, 0);
                connectorGO.transform.SetParent(transform);
                ConnectorRenderer connectorRenderer = connectorGO.GetComponent<ConnectorRenderer>();
                if (connectorRenderer != null)
                {
                    connectorRenderer.Initialize(connector);
                }
                _connectorGOs.Add(connector.LocalPosition, connectorGO);
            }
        }
        private void MachineInit()
        {
            foreach (Machine machine in _data.GetMachines())
            {
                GameObject machineGO = Instantiate(machinePrefab);
                machineGO.name = $"Machine_{machine.ID}";
                machineGO.transform.position = new Vector3(machine.LocalPosition.x, 0, machine.LocalPosition.y);
                machineGO.transform.rotation =  Quaternion.Euler(0, machine.Rotation, 0);
                machineGO.transform.SetParent(transform);
                _machineGOs.Add(machine.LocalPosition, machineGO);
            }
        }

        public void Refresh()
        {
            foreach (Machine machine in _data.GetMachines())
            {
                GameObject machineGO = _machineGOs[machine.LocalPosition];
            }
        }

    }
}