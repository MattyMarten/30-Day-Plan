using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{

    [Header("Item Info")]
    public string itemName;
    public ItemType type;

    [Header("Held Object")]
    public GameObject heldPrefab;

    [Header("Inventory Object")]
    public Sprite image;

    [Header("Gameplay")]
    public ActionType actionType;

}
public enum ItemType
{
    Loot,
    Utility,
    KeyItem,
    Armor,
}

public enum ActionType
{
    Flashlight,
    Hit,
    Open,
    Use,
    Pickup,
}
