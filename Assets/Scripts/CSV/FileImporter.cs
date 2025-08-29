//-------------------------------------------------------------
//  作成者：サム
//  用途：対象のファイルを ScriptableObject で生成・内容更新するために必要なデータ
//  別のデーターベースを登録したい場合は
//  まず WordDataBase のようにそれ専用のスクリプトを作成
//  その後は、下の DataBaseType にそのデーターベースの名前追加
//  次は、さらに下の DataBaseTypeDictionary と DataTypeDictionary に登録
//  最後はこの FileImporter アセットで変数の値を変えてボタンを押す
//　注意：手動でボタンを押さないと、ファイルの内容が更新されても、ScritableObject のほうでは更新されないよー
//-------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FileImporter", menuName = "Scriptable Objects/FileImporter")]
public class FileImporter : ScriptableObject
{
    public enum DataBaseType
    {
        word,
    }
    public static Dictionary<DataBaseType, Type> DataBaseTypeDictionary = new Dictionary<DataBaseType, Type>()
    {
        { DataBaseType.word, typeof(WordDataBase)},
    };
    public static Dictionary<Type, Type> DataTypeDictionary = new Dictionary<Type, Type>()
    {
        {typeof(WordDataBase), typeof(WordData) },
    };


    [Tooltip("読み込まないヘッダーの行数")] public int skipLines;
    [Tooltip("CSVファイル")] public TextAsset file;
    [Tooltip("データベースの種類")] public DataBaseType dataBaseType;


    public Type GetDataBaseType()
    {
        Type result = null;
        if(!DataBaseTypeDictionary.TryGetValue(dataBaseType, out result))
        {
            Debug.LogError("データーベースタイプが登録されていない");
        }
        return result;
    }

    public Type GetDataType()
    {
        Type result = null;
        if (!DataBaseTypeDictionary.TryGetValue(dataBaseType, out result))
        {
            Debug.LogError("データーベースタイプが登録されていない");
        }
        else
        {
            if(!DataTypeDictionary.TryGetValue(result, out result))
            {
                result = null;
                Debug.LogError("データータイプが登録されていない");
            }
        }
        return result;
    }

}
