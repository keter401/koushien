//-------------------------------------------------------------
//  作成者：サム
//  用途：対象のファイルを ScriptableObject に流し込む際のデータ順番（形式）
//-------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "WordDataBase", menuName = "Scriptable Objects/WordDataBase")]
public class WordDataBase : DataBase
{
    public List<WordData> datas = new List<WordData>();

    override public void ClearAllDatas()
    {
        datas.Clear();
        datas.TrimExcess();
    }
    override public void AddData(DataBaseData data)
    {
        datas.Add(data as WordData);
    }

}


[System.Serializable]
public class WordData : DataBaseData
{
    [Tooltip("技名")] public string name;              //技名
    [Tooltip("文字数")] public int wordLength;         //技名の文字数
    [Tooltip("攻撃力")] public float attackPower;      //攻撃力   
    [Tooltip("防御力")] public float defensePower;     //防御力
    [Tooltip("機動力")] public float mobility;         //機動力
    [Tooltip("確率")] public float rate;               //確率

    public override bool Init(string[] data)
    {
        if(data.Count() < 6) {
            Debug.LogWarning("要素数が足りないよ");
            return  false; 
        }
        this.name = data[0];
        if (this.name == "") { return false; }

        int.TryParse(data[1], out wordLength);

        float.TryParse(data[2], out this.attackPower);
        float.TryParse(data[3], out this.defensePower);
        float.TryParse(data[4], out this.mobility);
        float.TryParse(data[5], out this.rate);

        return true;
    }
}
