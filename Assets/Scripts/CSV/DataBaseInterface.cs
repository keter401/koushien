//-------------------------------------------------------------
//  �쐬�ҁF�T��
//  �p�r�F�f�[�^�x�[�X�̃C���^�t�F�[�X�Ƃ��Ďg��
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