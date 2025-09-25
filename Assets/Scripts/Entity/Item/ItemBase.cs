using UnityEngine;

public enum ItemType
{
    None,
    Word,
    Part
}

[System.Serializable]
public class ItemBase
{
    [SerializeField] private ItemType type = ItemType.None;
    [SerializeField] private string dictionaryKey = string.Empty;
    [SerializeField] private int id = 0;
    [SerializeField] private string name = "Default Item";
    [SerializeField] private string description = "This is a default item description.";

    /// <summary>
    /// コンストラクタです
    /// </summary>
    /// <param name="itemType">アイテムの種類</param>
    /// <param name="itemId">アイテムID</param>
    /// <param name="itemName">アイテムの名前</param>
    /// <param name="itemDescription">アイテムの説明</param>
   public ItemBase(ItemType itemType, int itemId, string itemName = "Unknown Item", string itemDescription = "Unknown")
   {
        type = itemType;
        id = itemId;
        name = itemName;
        description = itemDescription;
   }
   public ItemBase(ItemType itemType, string key, int itemId, string itemName = "Unknown Item", string itemDescription = "Unknown")
   {
        type = itemType;
        dictionaryKey = key;
        id = itemId;
        name = itemName;
        description = itemDescription;
   }

    /// <summary>
    /// 存在しないアイテムを取得します
    /// </summary>
    /// <returns>存在しないアイテム</returns>
    public static ItemBase CreateNoneItem()
    {
        return new ItemBase(ItemType.None, 0, "No Item", "This item does not exist.");
    }



    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public ItemType Type
    {
        get { return type; }
        set { type = value; }
    }

    public string DictionaryKey
    {
        get { return dictionaryKey; }
        set { dictionaryKey = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public string Description
    {
        get { return description; }
        set { description = value; }
    }
}
