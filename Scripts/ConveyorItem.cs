using UnityEngine;

public class ConveyorItem : MonoBehaviour
{
    [Header("Item Properties")]
    public int itemType; // 1: coal, 2: iron, 3: copper, 4: stone, etc.
    public int quantity = 1;
    
    [Header("Visual")]
    public GameObject itemModel;
    
    // Optional: Add rotation while on conveyor
    public bool rotateOnBelt = false;
    public float rotationSpeed = 30f;
    
    private void Update()
    {
        if (rotateOnBelt)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    public static ConveyorItem CreateItem(int type, int amount, Vector3 position)
    {
        // This could be expanded to use an object pool for better performance
        GameObject itemObj = new GameObject($"Item_{type}");
        itemObj.transform.position = position;
        
        ConveyorItem item = itemObj.AddComponent<ConveyorItem>();
        item.itemType = type;
        item.quantity = amount;
        
        // Create a simple visual representation based on type
        item.CreateVisualRepresentation();
        
        return item;
    }
    
    private void CreateVisualRepresentation()
    {
        // Create a simple visual based on item type
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // Set color based on item type
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (itemType)
            {
                case 1: // Coal
                    renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
                    break;
                case 2: // Iron
                    renderer.material.color = new Color(0.8f, 0.4f, 0.2f);
                    break;
                case 3: // Copper
                    renderer.material.color = new Color(0.8f, 0.5f, 0.2f);
                    break;
                case 4: // Stone
                    renderer.material.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
                default:
                    renderer.material.color = Color.white;
                    break;
            }
        }
        
        itemModel = visual;
    }
}