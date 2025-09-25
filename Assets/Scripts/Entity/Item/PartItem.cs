using System.Linq;
using UnityEngine;

public class PartItem : ItemBase
{
    private PartDataBase dataBase = null;

    public PartItem(int dictionaryId, int itemId) : base(ItemType.Part, itemId)
    {
        dataBase = GManager.instance.GetPartDataBase();
        var list = dataBase.partStyleDictionary.ElementAt(dictionaryId).Value;

        Name = list[itemId].name;
        Description = $"Part: {Name}\n" +
                      $"Type: {list[itemId].type}\n" +
                      $"Cost: {list[itemId].cost}\n";

        DictionaryKey = dataBase.partStyleDictionary.ElementAt(dictionaryId).Key;
    }

    public PartItem(string dictionaryKey, int itemId) : base(ItemType.Part, dictionaryKey, itemId)
    {
        dataBase = GManager.instance.GetPartDataBase();
        var list = dataBase.partStyleDictionary[dictionaryKey];

        Name = list[itemId].name;
        Description = $"Part: {Name}\n" +
                      $"Type: {list[itemId].type}\n" +
                      $"Cost: {list[itemId].cost}\n";
    }

}
