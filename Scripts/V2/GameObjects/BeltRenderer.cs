using System.Collections.Generic;
using UnityEngine;
using V2.Data;

namespace V2.GameObjects
{

    public class BeltRenderer : MonoBehaviour
    {
        public Material hasNextConnectionMaterial;
        public Material connectedMaterial;
        public GameObject itemPrefab; 
        
        public BeltData _beltData;
        private Dictionary<SimulationItem, GameObject> _itemObjects = new Dictionary<SimulationItem, GameObject>();
        
        [SerializeField] private float _itemHeightOffset = 0.1f; 
        [SerializeField] private Vector3 _startPosition; 
        [SerializeField] private Vector3 _endPosition; 
        [SerializeField] private int _maxItemsOnBelt = 5; 
        [SerializeField] private float _minItemSpacing = 0.2f; 

        public void Start()
        {
            if (hasNextConnectionMaterial == null)
            {
                hasNextConnectionMaterial = new Material(connectedMaterial);
                hasNextConnectionMaterial.EnableKeyword("_EMISSION");
                hasNextConnectionMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 1.0f) * 1.5f);
            }
            CalculateBeltPath();
            if (_beltData != null)
            {
                _beltData.OnItemAdded += OnItemAdded;
                _beltData.OnItemRemoved += OnItemRemoved;
                _beltData.OnConnectionChanged += OnConnectionChanged;
            }
        }

        public void Update()
        {
            if (_beltData == null)
                return;
            UpdateBeltMaterial();
            UpdateItemPositions();
        }
        
        private void UpdateBeltMaterial()
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer == null) return;
            
            if (_beltData.GetNextBelt() != null)
            {
                renderer.material = hasNextConnectionMaterial;
            }
            else
            {
                renderer.material = connectedMaterial;
            }
        }
        
        private void CalculateBeltPath()
        {
            Renderer beltRenderer = GetComponentInChildren<Renderer>();
            if (beltRenderer != null)
            {
                Bounds bounds = beltRenderer.bounds;
                float halfLength = bounds.size.z / 2f;
                
                _startPosition = transform.position - transform.forward * halfLength;
                _endPosition = transform.position + transform.forward * halfLength;
                
           
                float surfaceHeight = bounds.center.y + (bounds.size.y / 2f);
                _startPosition.y = surfaceHeight + _itemHeightOffset;
                _endPosition.y = surfaceHeight + _itemHeightOffset;
            }
            else
            {
                _startPosition = transform.position - transform.forward * 0.5f;
                _endPosition = transform.position + transform.forward * 0.5f;
                _startPosition.y += _itemHeightOffset;
                _endPosition.y += _itemHeightOffset;
            }
        }
        
        private void UpdateItemPositions()
        {
            Dictionary<SimulationItem, float> itemsWithProgress = _beltData.GetAllItemsWithProgress();
            
            foreach (var kvp in itemsWithProgress)
            {
                SimulationItem item = kvp.Key;
                float progress = kvp.Value;
                
                if (_itemObjects.TryGetValue(item, out GameObject itemObject) && itemObject != null)
                {
                   
                    Vector3 itemPosition = Vector3.Lerp(_startPosition, _endPosition, progress);
                    itemObject.transform.position = itemPosition;
                    itemObject.transform.rotation = transform.rotation;
                }
            }
        }
        
        private void OnItemAdded(BeltData belt, SimulationItem item)
        {
            if (belt == _beltData && !_itemObjects.ContainsKey(item) && itemPrefab != null)
            {
                GameObject itemObject = Instantiate(itemPrefab, _startPosition, transform.rotation);
                _itemObjects[item] = itemObject;
                Renderer itemRenderer = itemObject.GetComponent<Renderer>();
        if (itemRenderer != null)
        {
            // Create a new material instance to avoid affecting other items
            Material itemMaterial = new Material(itemRenderer.material);
            
            // Set the base color from the item
            itemMaterial.color = item.ItemColor;
            
            // Add emission for glow effect
            itemMaterial.EnableKeyword("_EMISSION");
            itemMaterial.SetColor("_EmissionColor", item.ItemColor * 0.5f);
            
            itemRenderer.material = itemMaterial;
        }
            }
        }
        
        private void OnItemRemoved(BeltData belt, SimulationItem item)
        {
            if (belt == _beltData && _itemObjects.TryGetValue(item, out GameObject itemObject))
            {
                // Always destroy the GameObject when an item is removed from this belt
                Destroy(itemObject);
                _itemObjects.Remove(item);
                
                // The next belt will create its own GameObject for the item when it receives it
                // through its OnItemAdded event
            }
        }
        
        private void OnConnectionChanged(BeltData belt, BeltData connectedBelt)
        {
            UpdateBeltMaterial();
        }
        
        private BeltRenderer FindNextBeltRenderer(BeltData nextBelt)
        {
            if (nextBelt == null) return null;
            BeltRenderer[] allRenderers = FindObjectsOfType<BeltRenderer>();
            foreach (BeltRenderer renderer in allRenderers)
            {
                if (renderer._beltData == nextBelt)
                {
                    return renderer;
                }
            }
            
            return null;
        }

        public void Initialize(BeltData beltData)
        {
            if (_beltData != null)
            {
                _beltData.OnItemAdded -= OnItemAdded;
                _beltData.OnItemRemoved -= OnItemRemoved;
                _beltData.OnConnectionChanged -= OnConnectionChanged;
            }
            foreach (var itemObject in _itemObjects.Values)
            {
                if (itemObject != null)
                {
                    Destroy(itemObject);
                }
            }
            _itemObjects.Clear();
            _beltData = beltData;
            CalculateBeltPath();
            
            // Subscribe to new belt's events
            if (_beltData != null)
            {
                _beltData.OnItemAdded += OnItemAdded;
                _beltData.OnItemRemoved += OnItemRemoved;
                _beltData.OnConnectionChanged += OnConnectionChanged;
                
                Dictionary<SimulationItem, float> existingItems = _beltData.GetAllItemsWithProgress();
                foreach (var item in existingItems.Keys)
                {
                    OnItemAdded(_beltData, item);
                }
                UpdateBeltMaterial();
            }
        }
        
        private void OnDestroy()
        {
            if (_beltData != null)
            {
                _beltData.OnItemAdded -= OnItemAdded;
                _beltData.OnItemRemoved -= OnItemRemoved;
                _beltData.OnConnectionChanged -= OnConnectionChanged;
            }
            foreach (var itemObject in _itemObjects.Values)
            {
                if (itemObject != null)
                {
                    Destroy(itemObject);
                }
            }
            _itemObjects.Clear();
        }
    }
}