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

        for (int i = 6; i < 16; i++)
        {
            Machine tmpMachine = createNewMachine(newChunk, new Vector2Int(i, 6));
            tmpMachine.SetRecipe(RecipeDatabase.Instance.GetRecipe("IronOre_to_IronPlate"));
        }

        for (int i = 6; i < 16; i++)
        {
            Connector tmpConnector = createNewConnector(newChunk, new Vector2Int(i, 5));
            tmpConnector.Rotate(newChunk);
        }
        
        // lay down 20 belts
        belt = null;
        for (int i = 6; i < 26; i++)
        {
            belt = createNewBelt(newChunk, new Vector2Int(i, 8));
            belt.Rotate(newChunk);
            
        }
        
        for (int i = 6; i < 16; i++)
        {
            Connector tmpConnector = createNewConnector(newChunk, new Vector2Int(i, 7));
            tmpConnector.Rotate(newChunk);
        }

       
        for (int i = 6; i < 16; i++)
        {
            Machine tmpMachine = createNewMachine(newChunk, new Vector2Int(i, 10));
            tmpMachine.SetRecipe(RecipeDatabase.Instance.GetRecipe("CopperOre_to_CopperPlate"));
        }
        
        for (int i = 6; i < 16; i++)
        {
            Connector tmpConnector = createNewConnector(newChunk, new Vector2Int(i, 9));
            tmpConnector.Rotate(newChunk);
            tmpConnector.Rotate(newChunk);
            tmpConnector.Rotate(newChunk);
        }
        
        for (int i = 4; i < 16; i++)
        {
            belt = createNewBelt(newChunk, new Vector2Int(i, 12));
            belt.Rotate(newChunk);
            
        }
        
        for (int i = 6; i < 16; i++)
        {
            Connector tmpConnector = createNewConnector(newChunk, new Vector2Int(i, 11));
            tmpConnector.Rotate(newChunk);
            tmpConnector.Rotate(newChunk);
            tmpConnector.Rotate(newChunk);
        }
        
        createNewMiner(newChunk, new Vector2Int(4, 14), "CopperOre");
        
        minerConnector1 = createNewConnector(newChunk, new Vector2Int(4, 13));
        minerConnector1.Rotate(newChunk);
        minerConnector1.Rotate(newChunk);
        minerConnector1.Rotate(newChunk);

        
        V2.Data.StorageBox s1 = new V2.Data.StorageBox(new Vector2Int(24, 10), 10);
        newChunk.AddStorageBox(s1);
        
        minerConnector1 = createNewConnector(newChunk, new Vector2Int(24, 09));
        minerConnector1.Rotate(newChunk);
        
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
  

    public Machine createNewMachine(ChunkData chunk, Vector2Int coords, Recipe recipe = null)
    {
     
        var machine = new Machine(coords);
        if (recipe != null) machine.CurrentRecipe = recipe;
        chunk.AddMachine(machine);
        machine.OnRecipeCompleted += OnMachineCompletedRecipe;
        return machine;
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
