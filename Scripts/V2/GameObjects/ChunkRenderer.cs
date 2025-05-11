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
        
        private Vector2Int _coords;
        private SimulationManagerV2 _sim;
        private ChunkData _data;
        private Dictionary<Vector2Int, GameObject> _machineGOs = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _connectorGOs = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> _beltGOs = new Dictionary<Vector2Int, GameObject>();

        public void Initialize(Vector2Int coords, SimulationManagerV2 sim)
        {
            _coords = coords;
            _sim = sim;
            _data = _sim.GetChunk(coords);
            MachineInit();
            BeltInit();
            ConnectorInit();
            
        }

        private void BeltInit()
        {
            foreach (BeltData belt in _data.GetBelts())
            {
                GameObject beltGO = Instantiate(beltPrefab);
                beltGO.name = $"Belt_{belt.ID}";
                beltGO.transform.position = new Vector3(belt.LocalPostion.x, 0, belt.LocalPostion.y);
                beltGO.transform.rotation =  Quaternion.Euler(0, belt.Rotation, 0);
                beltGO.transform.SetParent(transform);
                _beltGOs.Add(belt.LocalPostion, beltGO);
            }
        }

        private void ConnectorInit()
        {
            foreach (Connector connector in _data.GetConnectors())
            {
                GameObject connectorGO = Instantiate(connectorPrefab);
                connectorGO.name = $"Connector_{connector.ID}";
                connectorGO.transform.position = new Vector3(connector.LocalPostion.x, 0, connector.LocalPostion.y);
                connectorGO.transform.rotation =  Quaternion.Euler(0, connector.Rotation, 0);
                connectorGO.transform.SetParent(transform);
                ConnectorRenderer connectorRenderer = connectorGO.GetComponent<ConnectorRenderer>();
                if (connectorRenderer != null)
                {
                    connectorRenderer.Initialize(connector);
                }
                _connectorGOs.Add(connector.LocalPostion, connectorGO);
            }
        }
        private void MachineInit()
        {
            foreach (Machine machine in _data.GetMachines())
            {
                GameObject machineGO = Instantiate(machinePrefab);
                machineGO.name = $"Machine_{machine.ID}";
                machineGO.transform.position = new Vector3(machine.LocalPostion.x, 0, machine.LocalPostion.y);
                machineGO.transform.rotation =  Quaternion.Euler(0, machine.Rotation, 0);
                machineGO.transform.SetParent(transform);
                _machineGOs.Add(machine.LocalPostion, machineGO);
            }
        }

        public void Refresh()
        {
            foreach (Machine machine in _data.GetMachines())
            {
                GameObject machineGO = _machineGOs[machine.LocalPostion];
            }
        }

    }
}