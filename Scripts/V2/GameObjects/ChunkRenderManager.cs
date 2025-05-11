using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using V2.Data;
using V2.GameObjects;

public class ChunkRenderManager : MonoBehaviour
{
    [Tooltip("Chunk prefab to use for rendering")]
    public GameObject chunkPrefab;
    
    [Tooltip("Game object pool size")]
    public int gameObjectPoolSize = 50;
    
    private SimulationManagerV2 _sim;
    private Queue<GameObject> _goPool = new Queue<GameObject>();
    private Dictionary<Vector2Int, GameObject> _activeChunks = new Dictionary<Vector2Int, GameObject>();
    
    private void Awake()
    {
        _sim = FindObjectOfType<SimulationManagerV2>();
        
        // initalise game object pool

        for (int i = 0; i < gameObjectPoolSize; i++)
        {
            GameObject go = Instantiate(chunkPrefab, transform);
            go.SetActive(false);
            _goPool.Enqueue(go);
        }
        _sim.OnChunkDirty += HandleDirtyChunk;
    }

    private void OnDestroy()
    {
        _sim.OnChunkDirty -= HandleDirtyChunk;
    }
    void Update()
    {
        
    }

    private void HandleDirtyChunk(Vector2Int coords)
    {
        if (IsChunkVisible(coords))
        {
            if (_activeChunks.ContainsKey(coords))
            {
                UpdateChunkVisuals(coords);
            }
            else
            {
                SpawnChunk(coords);
            }
        }
    }

    private void UpdateChunkVisuals(Vector2Int coords)
    {
        GameObject go = _activeChunks[coords];
        go.GetComponent<ChunkRenderer>().Refresh();
        
    }

    public void SpawnChunk(Vector2Int coords)
    {
        if (_goPool.Count == 0)
        {
            Debug.LogWarning("No game objects available in pool");
            return; 
        }
        
        GameObject go = _goPool.Dequeue();
        go.SetActive(true);
        go.transform.position = new Vector3(coords.x * ChunkData.ChunkSize, 0, coords.y * ChunkData.ChunkSize);
        go.name = $"Chunk_{coords.x}_{coords.y}";
        
        ChunkRenderer chunkRenderer = go.GetComponent<ChunkRenderer>();
        chunkRenderer.Initialize(coords, _sim);
        
        _activeChunks[coords] = go;
    }
    private bool IsChunkVisible(Vector2Int coords)
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector2Int camChunk = WorldToChunkCoords(camPos);
        int visibleRange = 3;
        return (coords - camChunk).magnitude <= visibleRange;
    }

    private Vector2Int WorldToChunkCoords(Vector3 worldPos)
    {
        int cx = Mathf.FloorToInt(worldPos.x / ChunkData.ChunkSize);
        int cy = Mathf.FloorToInt(worldPos.z / ChunkData.ChunkSize);
        return new Vector2Int(cx, cy);
    }
}
