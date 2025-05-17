using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using V2.Data;

public class SimulationManagerV2 : MonoBehaviour
{
    [Tooltip("Simulation ticks per second")]
    public float ticksPerSecond = 20f;
    private Dictionary<Vector2Int, ChunkData> _chunks = new Dictionary<Vector2Int, ChunkData>();

    private float _tickTimer = 0f;
    private float TickInterval => 1f / ticksPerSecond;
    private bool _isRunning = true;

    private HashSet<Vector2Int> _dirtyChunks = new HashSet<Vector2Int>();

    public event Action<Vector2Int> OnChunkDirty;
    private UPSMonitor upsMonitor;
    
    private bool _showUI;
    private Rect _windowRect = new Rect(20, 20, 300, 280); 
    private int _windowID = 0;

    // Machine statistics tracking
    private int _totalMachines = 0;
    private int _totalCompletedRecipes = 0;
    
    private int _totalConnectors = 0;
    
    private int _totalBelts = 0;
    
    public SimulationManagerV2()
    {
        upsMonitor = new UPSMonitor();
        
        GetChunk(new Vector2Int(0, 0));
 
    }
    private void InitaliseChunk(ChunkData newChunk)
    {
        
        // Subscribe to machine events
        newChunk.OnMachineAdded += OnMachineAdded;
        newChunk.OnMachineRemoved += OnMachineRemoved;
        
        // Subscribe to connector events
        newChunk.OnConnectorAdded += OnConnectorAdded;
        newChunk.OnConnectorRemoved += OnConnectorRemoved;

        newChunk.OnBeltAdded += OnBeltAdded;
        newChunk.OnBeltRemoved += OnBeltRemoved;
        
        // Create a miner at the start of the first belt line (5,6)
        createNewMiner(newChunk, new Vector2Int(4, 6), "IronOre");

        // lay down 20 belts
        BeltData belt = null;
        for (int i = 4; i < 16; i++)
        {
            belt = createNewBelt(newChunk, new Vector2Int(i, 4));
            belt.Rotate(newChunk);
            
        }

        // Create a connector to connect the miner to the belt
         Connector minerConnector1 = createNewConnector(newChunk, new Vector2Int(4, 5));
        minerConnector1.Rotate(newChunk);
        minerConnector1.Rotate(newChunk);
        minerConnector1.Rotate(newChunk);
        
        
        createNewMachine(newChunk,new Vector2Int(15, 6));
        
        Connector tmpConnector = createNewConnector(newChunk,new Vector2Int(15, 5));
        tmpConnector.Rotate(newChunk);
       
      /* 
        createNewBelt(newChunk,new Vector2Int(5, 6));
        createNewBelt(newChunk,new Vector2Int(6, 6));
        createNewBelt(newChunk,new Vector2Int(7, 6));
        createNewBelt(newChunk,new Vector2Int(8, 6));
        createNewBelt(newChunk,new Vector2Int(9, 6));

        
        createNewMachine(newChunk,new Vector2Int(6, 8));
        createNewMachine(newChunk,new Vector2Int(7, 8));
        createNewMachine(newChunk,new Vector2Int(8, 8));
        createNewMachine(newChunk,new Vector2Int(9, 8));
        
        
        BeltData newBelt = createNewBelt(newChunk,new Vector2Int(5, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(6, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(7, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(8, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(9, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(10, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(11, 10));
        newBelt.Rotate(newChunk);
        newBelt =createNewBelt(newChunk,new Vector2Int(12, 10));
        newBelt.Rotate(newChunk);
        
        createNewMachine(newChunk,new Vector2Int(5, 12));
        createNewMachine(newChunk,new Vector2Int(6, 12));
        createNewMachine(newChunk,new Vector2Int(7, 12));
        createNewMachine(newChunk,new Vector2Int(8, 12));
        createNewMachine(newChunk,new Vector2Int(9, 12));
        
        // Create a miner at the start of the second belt line (5,14)
        createNewMiner(newChunk, new Vector2Int(4, 14), "CopperOre");
        // Create a connector to connect the miner to the belt
        Connector minerConnector2 = createNewConnector(newChunk, new Vector2Int(4, 13));
        minerConnector2.Rotate(newChunk);
        
        createNewBelt(newChunk,new Vector2Int(5, 14));
        createNewBelt(newChunk,new Vector2Int(6, 14));
        createNewBelt(newChunk,new Vector2Int(7, 14));
        createNewBelt(newChunk,new Vector2Int(8, 14));
        createNewBelt(newChunk,new Vector2Int(9, 14));
        
        createNewConnector(newChunk,new Vector2Int(5, 7)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(6, 7)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(7, 7)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(8, 7)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(9, 7)).Rotate(newChunk);
        
        createNewConnector(newChunk,new Vector2Int(5, 9)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(6, 9)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(7, 9)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(8, 9)).Rotate(newChunk);
        createNewConnector(newChunk,new Vector2Int(9, 9)).Rotate(newChunk);
        
        Connector tmpConnector = createNewConnector(newChunk,new Vector2Int(5, 11));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);

        tmpConnector = createNewConnector(newChunk,new Vector2Int(6, 11));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(7, 11));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(8, 11));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(9, 11));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(5, 13));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);

        tmpConnector = createNewConnector(newChunk,new Vector2Int(6, 13));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(7, 13));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(8, 13));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(9, 13));
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        tmpConnector.Rotate(newChunk);
        
        
        
       
        List<string> inputTypes = new List<string> { "Iron" };
        Recipe consumptionRecipe = new Recipe(
            duration: 1.0f,
            outputItemType: "ProcessedIron",
            inputItemTypes: inputTypes,
            inputItemCount: 1
        );

        createNewMachine(newChunk,new Vector2Int(11, 12), consumptionRecipe);
        
       
        
        tmpConnector = createNewConnector(newChunk,new Vector2Int(11, 11));
        tmpConnector.Rotate(newChunk);
        
        */
        foreach (var b in newChunk.GetBelts())
        {
            b.CheckConnections(newChunk);
        }
        MarkChunkDirty(newChunk.Coords);
    }

    public BeltData createNewBelt(ChunkData chunk, Vector2Int coords)
    {
        BeltData belt = new BeltData(coords);
        chunk.AddBelt(belt);
        belt.CheckConnections(chunk);
        return belt;
    }
  

    public void createNewMachine(ChunkData chunk, Vector2Int coords, Recipe recipe = null)
    {
     
        var machine = new Machine(coords);
        if (recipe != null) machine.CurrentRecipe = recipe;
        chunk.AddMachine(machine);
        machine.OnRecipeCompleted += OnMachineCompletedRecipe;
    }
    
    public Miner createNewMiner(ChunkData chunk, Vector2Int coords, string oreType = "IronOre")
    {
        var miner = MinerFactory.CreateMiner(coords, oreType);
        chunk.AddMachine(miner);
        miner.OnRecipeCompleted += OnMachineCompletedRecipe;
        return miner;
    }

    
    public void createNewMachine(ChunkData chunk, Vector2 worldPosition)
    {
        Vector2Int gridPosition = GridUtility.SnapToGrid(worldPosition);
        createNewMachine(chunk, gridPosition);
    }

    public Connector createNewConnector(ChunkData chunk, Vector2Int coords)
    {
        
        var Connector = new Connector(coords);
        
        chunk.AddConnector(Connector);
        return Connector;
    }

    
    public Connector createNewConnector(ChunkData chunk, Vector2 worldPosition)
    {
        Vector2Int gridPosition = GridUtility.SnapToGrid(worldPosition);
        return createNewConnector(chunk, gridPosition);
    }
    
    public bool CanPlaceEntityAt(ChunkData chunk, Vector2Int gridPosition)
    {
        return !GridUtility.IsOccupied(chunk, gridPosition);
    }
    
    public bool CanPlaceEntityAt(ChunkData chunk, Vector2 worldPosition)
    {
        Vector2Int gridPosition = GridUtility.SnapToGrid(worldPosition);
        return CanPlaceEntityAt(chunk, gridPosition);
    }

    private void OnBeltAdded(BeltData belt)
    {
        _totalBelts++;
    }

    private void OnBeltRemoved(BeltData belt)
    {
        _totalBelts--;
    }
    private void OnMachineCompletedRecipe(Machine machine)
    {
        _totalCompletedRecipes++;
    }
    
    // Event handlers for machine count tracking
    private void OnMachineAdded(Machine machine)
    {
        _totalMachines++;
    }

    private void OnMachineRemoved(Machine machine)
    {
        _totalMachines--;
    }

    private void OnConnectorAdded(Connector connector)
    {
        _totalConnectors++;
    }

    private void OnConnectorRemoved(Connector connector)
    {
        _totalConnectors--;
    }
    
    private void Update()
    {
        if( Input.GetKeyDown(KeyCode.I))
        {
            _showUI = !_showUI;
        }

        if (_isRunning)
        {
            _tickTimer += Time.deltaTime;
            while (_tickTimer >= TickInterval)
            {
                upsMonitor.update();
                _tickTimer -= TickInterval;
                RunSimulationTick();
            }

        }
    }


    private void RunSimulationTick()
    {
        if (_dirtyChunks.Count == 0) return;

        var chunksToProcess = new List<Vector2Int>(_dirtyChunks);
        _dirtyChunks.Clear();

        foreach (var coord in chunksToProcess)
        {
            
            if (!_chunks.TryGetValue(coord, out var chunk))
                continue;
            
            chunk.ProcessTick(TickInterval);
            foreach (var neighbor in chunk.GetNeighborsThatChanged())
            {
                MarkChunkDirty(neighbor);
            }
            
            MarkChunkDirty(coord);
        }
    }
    
    public void MarkChunkDirty(Vector2Int coord)
    {
        if (_dirtyChunks.Add(coord))
        {
            OnChunkDirty?.Invoke(coord);
        }
    }

    public ChunkData GetChunk(Vector2Int coord)
    {
        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            chunk = new ChunkData(coord);
            _chunks.Add(coord, chunk);
            _dirtyChunks.Add(coord);
            InitaliseChunk(chunk);
        }
        
        return chunk;
    }
    
    

    private void OnGUI()
    {
        if (_showUI)
        {
            _windowRect = GUI.Window(_windowID, _windowRect, DrawWindow, "Simulation Control");
        }
    }


    private void DrawWindow(int id)
    {
        string statusText = _isRunning ? "Status: <color=green>Running</color>" : "Status: <color=red>Not Running</color>";
        GUILayout.Label(statusText, new GUIStyle(GUI.skin.label) { richText = true });
       
        GUILayout.Label($"UPS: {upsMonitor.getUPS()}");
        GUILayout.Label($"Total Chunks: {_chunks.Count}");
        GUILayout.Label($"Dirty Chunks: {_dirtyChunks.Count}");
        
        GUILayout.Label($"Total Machines: {_totalMachines}");
        GUILayout.Label($"Total Completed Recipes: {_totalCompletedRecipes}");
        
        GUILayout.Label($"Total Connectors: {_totalConnectors}");
        GUILayout.Label($"Total Belts: {_totalBelts}");
        
        GUI.enabled = true;
        
        string toggleButtonText = _isRunning ? "Stop Simulation" : "Start Simulation";
        if (GUILayout.Button(toggleButtonText))
        {
            if (_isRunning)
                StopSimulaton();
            else
                StartSimulation();
        }
        
        // Close button
        if (GUILayout.Button("Close"))
        {
            _showUI = false;
        }
        GUI.DragWindow();
    }

    private void StartSimulation()
    {
        _isRunning = true;
    }

    private void StopSimulaton()
    {
        _isRunning = false;
    }
}
