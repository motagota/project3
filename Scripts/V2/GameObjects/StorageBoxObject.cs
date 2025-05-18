using UnityEngine;
using V2.Data;
using V2.UI;

namespace V2.GameObjects
{
    public class StorageBoxObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite storageBoxSprite;
        
        [Header("Settings")]
        [SerializeField] private int slotCount = 12;
        [SerializeField] private Color storageBoxColor = new Color(0.6f, 0.4f, 0.2f); // Brown color for storage
        
        private V2.Data.StorageBox _storageBox;
        
        public V2.Data.StorageBox StorageBoxData => _storageBox;
        
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }
            
            if (storageBoxSprite != null)
            {
                spriteRenderer.sprite = storageBoxSprite;
            }
            else
            {
                // Create a default sprite if none is assigned
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            
            spriteRenderer.color = storageBoxColor;
            
            // Add a box collider for interaction
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1, 1);
            }
        }
        
        public void Initialize(Vector2Int gridPosition)
        {
            // Create the storage box data
            _storageBox = new V2.Data.StorageBox(gridPosition, slotCount);
            
            // Position the game object on the grid
            transform.position = GridUtility.GridToWorld(gridPosition);
            
            // Set the name of the game object
            gameObject.name = $"StorageBox_{_storageBox.ID}";
        }
        
        private void OnMouseDown()
        {
            if (_storageBox != null)
            {
                // Open the storage box UI when clicked
                WindowManager.Instance.CreateStorageBoxWindow(_storageBox);
            }
        }
        
        private Sprite CreateDefaultSprite()
        {
            // Create a simple box sprite
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            
            // Fill the texture with the storage box color
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    // Create a border
                    if (x < 2 || x > 29 || y < 2 || y > 29)
                    {
                        colors[y * 32 + x] = Color.black;
                    }
                    else
                    {
                        colors[y * 32 + x] = storageBoxColor;
                    }
                    
                    // Add some details to make it look like a storage box
                    if ((x == 10 || x == 22) && y >= 8 && y <= 24)
                    {
                        colors[y * 32 + x] = Color.black;
                    }
                    if (y == 16 && x >= 4 && x <= 28)
                    {
                        colors[y * 32 + x] = Color.black;
                    }
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
        
        private void OnDestroy()
        {
            // Clean up any resources if needed
        }
    }
}