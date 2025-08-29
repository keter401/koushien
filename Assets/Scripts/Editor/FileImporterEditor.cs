//-------------------------------------------------------------
//  �쐬�ҁF�T��
//  �p�r�F�Ώۂ̃t�@�C���� ScriptableObject �Ő����E���e�X�V�����s���镔��
//-------------------------------------------------------------
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FileImporter))]
public class FileImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //�ϐ����ŐV�̏�Ԃɂ���
        serializedObject.Update();
        base.OnInspectorGUI();
        var importer = target as FileImporter;
        if (GUILayout.Button("���[�h�f�[�^�x�[�X�X�V"))
        {
            UpdateDataBase(importer);
        }
    }

    void UpdateDataBase(FileImporter importer)
    {
        if (importer.file == null)
        {
            Debug.LogWarning(importer.name + "�ǂݍ��ރt�@�C����������܂���");
            return;
        }

        //�@CSV�`�����m�F
        string path = AssetDatabase.GetAssetPath(importer.file);
        string dataBaseName = "";
        if (path.IndexOf(".csv") != -1)
        {
            dataBaseName = path.Replace(".csv", ".asset");
        }
        else
        {
            Debug.LogWarning("�t�@�C���`����csv�ɕϊ����Ă�������");
            return;
        }

        //  �����̃f�[�^�[�x�[�X��ǂݍ��ށA�Ȃ��ꍇ�͐V���ɍ��
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

        //�t�@�C���̏���ǂݍ���Ńf�[�^�[�x�[�X�ɗ��Ƃ�
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
