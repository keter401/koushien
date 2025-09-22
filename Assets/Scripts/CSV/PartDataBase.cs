//-------------------------------------------------------------
//  �쐬�ҁF�T��
//  �p�r�F�Ώۂ̃t�@�C���� ScriptableObject �ɗ������ލۂ̃f�[�^���ԁi�`���j
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
        if(styleData.type.Contains("�p���`"))
        {
            puchStyleData.Add(styleData);
        }
        else if (styleData.type.Contains("�V���b�g"))
        {
            shotStyleData.Add(styleData);
        }
        else 
        {
            Debug.LogWarning("��ނ������ĂȂ��f�[�^���o");
        }
    }

}


[System.Serializable]
public class PartData : DataBaseData
{
    [Tooltip("�Z�ɕt�����O")] public string name;      //�Z�ɕt�����O
    [Tooltip("�p�[�c���")] public string type;        //�p�[�c�^�C�v
    [Tooltip("�R�X�g")] public float cost;             //�R�X�g   

    public override bool Init(string[] data)
    {
        //�p�����[�^�[�ϐ����񂵂ēǂݍ��ݏ���
        if (data.Count() < 3)
        {
            Debug.LogWarning("�v�f��������Ȃ���");
            return false;
        }
        this.name = data[0];
        if (this.name == "") { Debug.LogWarning("���O�������ĂȂ��v�f���o"); return false; }
        this.type = data[1];
        if(this.type == "") { Debug.LogWarning("��ނ������ĂȂ��v�f���o"); return false; }

        float.TryParse(data[2], out this.cost);

        return true;
    }
}

