using UnityEngine;

public class WordItem : ItemBase
{
    private WordDataBase dataBase = null;

    public WordItem(int itemId) : base(ItemType.Word, itemId)
    {
        dataBase = GManager.instance.GetWordDataBase();

        Name = dataBase.datas[itemId].name;
        Description = $"Word: {Name}\n" +
                      $"Length: {dataBase.datas[itemId].wordLength}\n" +
                      $"Attack: {dataBase.datas[itemId].attackPower}\n" +
                      $"Defense: {dataBase.datas[itemId].defensePower}\n" +
                      $"Mobility: {dataBase.datas[itemId].mobility}\n" +
                      $"Rate: {dataBase.datas[itemId].rate}";
    }
}
