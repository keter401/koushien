//-------------------------------------------------------------
//  作成者：サム
//  用途：対象のファイルを ScriptableObject に流し込む際のデータ順番（形式）
//-------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering.Universal;
using UnityEngine;

[CreateAssetMenu(fileName = "PartDataBase", menuName = "Scriptable Objects/PartDataBase")]
public class PartDataBase : DataBase
{
    public List<PartData> puchStyleData= new List<PartData>();
    public List<PartData> shotStyleData= new List<PartData>();

    override public void ClearAllDatas()
    {
        puchStyleData.Clear();
        puchStyleData.TrimExcess();
        shotStyleData.Clear();
        shotStyleData.TrimExcess();
    }
    override public void AddData(DataBaseData data)
    {
        var styleData = data as PartData;
        if(styleData.type.Contains("パンチ"))
        {
            puchStyleData.Add(styleData);
        }
        else if (styleData.type.Contains("ショット"))
        {
            shotStyleData.Add(styleData);
        }
        else 
        {
            Debug.LogWarning("種類が合ってないデータ検出");
        }
    }

}


[System.Serializable]
public class PartData : DataBaseData
{
    [Tooltip("技に付く名前")] public string name;      //技に付く名前
    [Tooltip("パーツ種類")] public string type;        //パーツタイプ
    [Tooltip("コスト")] public float cost;             //コスト   

    public override bool Init(string[] data)
    {
        //パラメーター変数分回して読み込み処理
        if (data.Count() < 3)
        {
            Debug.LogWarning("要素数が足りないよ");
            return false;
        }
        this.name = data[0];
        if (this.name == "") { Debug.LogWarning("名前が入ってない要素検出"); return false; }
        this.type = data[1];
        if(this.type == "") { Debug.LogWarning("種類が入ってない要素検出"); return false; }

        float.TryParse(data[2], out this.cost);

        return true;
    }
}

