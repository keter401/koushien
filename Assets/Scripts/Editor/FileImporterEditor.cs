//-------------------------------------------------------------
//  作成者：サム
//  用途：対象のファイルを ScriptableObject で生成・内容更新を実行する部分
//-------------------------------------------------------------
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FileImporter))]
public class FileImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //変数を最新の状態にする
        serializedObject.Update();
        base.OnInspectorGUI();
        var importer = target as FileImporter;
        if (GUILayout.Button("ワードデータベース更新"))
        {
            UpdateDataBase(importer);
        }
    }

    void UpdateDataBase(FileImporter importer)
    {
        if (importer.file == null)
        {
            Debug.LogWarning(importer.name + "読み込むファイルが見つかりません");
            return;
        }

        //　CSV形式か確認
        string path = AssetDatabase.GetAssetPath(importer.file);
        string dataBaseName = "";
        if (path.IndexOf(".csv") != -1)
        {
            dataBaseName = path.Replace(".csv", ".asset");
        }
        else
        {
            Debug.LogWarning("ファイル形式をcsvに変換してください");
            return;
        }

        //  同名のデーターベースを読み込む、ない場合は新たに作る
        var dataBase = AssetDatabase.LoadAssetAtPath(dataBaseName, importer.GetDataBaseType()) as DataBase;
        if (dataBase == null)
        {
            dataBase = ScriptableObject.CreateInstance(importer.GetDataBaseType()) as DataBase;
            AssetDatabase.CreateAsset((ScriptableObject)dataBase, dataBaseName);
        }
        else
        {
            dataBase.ClearAllDatas();
        }

        //ファイルの情報を読み込んでデーターベースに落とす
        string text = importer.file.text;
        string[] lines = text.Split('\n');
        for (int i = importer.skipLines; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(",");
            var dataInstance = Activator.CreateInstance(importer.GetDataType()) as DataBaseData;
            if (dataInstance.Init(data))
            {
                dataBase.AddData(dataInstance);
            }
        }


        EditorUtility.SetDirty(dataBase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


}
