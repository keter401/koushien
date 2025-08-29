//-------------------------------------------------------------
//  作成者：サム
//  用途：データベースのインタフェースとして使う
//-------------------------------------------------------------

using UnityEngine;

public class DataBase : ScriptableObject
{
    virtual public void ClearAllDatas() { }
    virtual public void AddData(DataBaseData data) { }

}

public class DataBaseData
{
    virtual public bool Init(string[] data) { return true; }

}