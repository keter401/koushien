//-------------------------------------------------------------
//  �쐬�ҁF�T��
//  �p�r�F�Ώۂ̃t�@�C���� ScriptableObject �Ő����E���e�X�V���邽�߂ɕK�v�ȃf�[�^
//  �ʂ̃f�[�^�[�x�[�X��o�^�������ꍇ��
//  �܂� WordDataBase �̂悤�ɂ����p�̃X�N���v�g���쐬
//  ���̌�́A���� DataBaseType �ɂ��̃f�[�^�[�x�[�X�̖��O�ǉ�
//  ���́A����ɉ��� DataBaseTypeDictionary �� DataTypeDictionary �ɓo�^
//  �Ō�͂��� FileImporter �A�Z�b�g�ŕϐ��̒l��ς��ă{�^��������
//�@���ӁF�蓮�Ń{�^���������Ȃ��ƁA�t�@�C���̓��e���X�V����Ă��AScritableObject �̂ق��ł͍X�V����Ȃ���[
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


    [Tooltip("�ǂݍ��܂Ȃ��w�b�_�[�̍s��")] public int skipLines;
    [Tooltip("CSV�t�@�C��")] public TextAsset file;
    [Tooltip("�f�[�^�x�[�X�̎��")] public DataBaseType dataBaseType;


    public Type GetDataBaseType()
    {
        Type result = null;
        if(!DataBaseTypeDictionary.TryGetValue(dataBaseType, out result))
        {
            Debug.LogError("�f�[�^�[�x�[�X�^�C�v���o�^����Ă��Ȃ�");
        }
        return result;
    }

    public Type GetDataType()
    {
        Type result = null;
        if (!DataBaseTypeDictionary.TryGetValue(dataBaseType, out result))
        {
            Debug.LogError("�f�[�^�[�x�[�X�^�C�v���o�^����Ă��Ȃ�");
        }
        else
        {
            if(!DataTypeDictionary.TryGetValue(result, out result))
            {
                result = null;
                Debug.LogError("�f�[�^�[�^�C�v���o�^����Ă��Ȃ�");
            }
        }
        return result;
    }

}
