//-------------------------------------------------------------
//  �쐬�ҁF�T��
//  �p�r�F�Ώۂ̃t�@�C���� ScriptableObject �ɗ������ލۂ̃f�[�^���ԁi�`���j
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
    [Tooltip("�Z��")] public string name;              //�Z��
    [Tooltip("������")] public int wordLength;         //�Z���̕�����
    [Tooltip("�U����")] public float attackPower;      //�U����   
    [Tooltip("�h���")] public float defensePower;     //�h���
    [Tooltip("�@����")] public float mobility;         //�@����
    [Tooltip("�m��")] public float rate;               //�m��

    public override bool Init(string[] data)
    {
        if(data.Count() < 6) {
            Debug.LogWarning("�v�f��������Ȃ���");
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
