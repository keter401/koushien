// �����: �M�c �F�C

using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private WordDataBase wordDataBase;     // ���[�h�f�[�^�x�[�X
    [SerializeField] private PartDataBase partDataBase;     // ���[�h�f�[�^�x�[�X
    [SerializeField] private int maxWordCount = 3;         // �ő�P�ꐔ
    [SerializeField] private int maxPartCount = 3;         // �ő�p�[�c��
    [SerializeField] private ItemBase[] words;        // �P��
    [SerializeField] private ItemBase[] parts;        // �p�[�c

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        words = new ItemBase[maxWordCount];
        parts = new ItemBase[maxPartCount];

        // �P���������
        for (int i = 0; i < maxWordCount; i++) {
            words[i] = ItemBase.CreateNoneItem();
        }
        // �p�[�c��������
        for (int i = 0; i < maxPartCount; i++)
        {
            parts[i] = ItemBase.CreateNoneItem();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }


    /// <summary>
    /// �󂢂Ă���P�ꂪ���邩���m�F����
    /// </summary>
    /// <returns>�󂢂Ă���True</returns>
    public bool HasEmptyWord()
    {
        // �󂢂Ă���P�ꂪ���邩
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type == ItemType.None) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// �󂢂Ă���p�[�c�����邩���m�F����
    /// </summary>
    /// <returns>�󂢂Ă���True</returns>
    public bool HasEmptyPart()
    {
        // �󂢂Ă���P�ꂪ���邩
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Type == ItemType.None)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// �󂢂Ă���P�ꂪ�Ȃ������m�F����
    /// </summary>
    /// <returns>�S�����܂��ĂȂ�������True</returns>
    public bool IsFullyEmptyWord()
    {
        // �󂢂Ă���P�ꂪ�Ȃ���
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type != ItemType.None) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// �󂢂Ă���p�[�c���Ȃ������m�F����
    /// </summary>
    /// <returns>�S�����܂��ĂȂ�������True</returns>
    public bool IsFullyEmptyPart()
    {
        // �󂢂Ă���P�ꂪ�Ȃ���
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Type != ItemType.None)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// �P��𑗐M����
    /// </summary>
    /// <param name="itemManager">����̃��[�h�}�l�[�W���[</param>
    /// <param name="wordIndex">���M���郏�[�h�̗v�f�ԍ�</param>
    /// <returns>���M�ł�����True</returns>
    public bool SendWord(ItemManager itemManager, int wordIndex)
    {
        // �󂢂Ă�z��ɒP���ǉ�
        for (int i = 0; i < itemManager.words.Length; i++) {
            if (itemManager.words[i].Type == ItemType.None) {
                itemManager.words[i] = words[(int)wordIndex];
                Debug.Log($"{words[(int)wordIndex].Name} �𑗐M  [{name}] -> [{itemManager.name}]");

                words[(int)wordIndex] = ItemBase.CreateNoneItem();

                return true;
            }
        }

        Debug.LogWarning($"[{itemManager.name}] �󂢂Ă���P�ꂪ����܂���");
        return false;
    }


    /// <summary>
    /// �P��������_���ɑ��M����
    /// </summary>
    /// <param name="itemManager">���[�h�̊Ǘ��N���X</param>
    /// <returns>���M�ł�����True</returns>
    public bool SendWord(ItemManager itemManager)
    {
        if (IsFullyEmptyWord()) return false;


        // �����_���ɒP���I��
        while (true) {
            int wordIndex = (int)Random.Range(0, maxWordCount);
            if (words[wordIndex].Type == ItemType.None) continue;

            if (SendWord(itemManager, wordIndex)) {
                return true;
            } else {
                Debug.LogWarning($"[{itemManager.name}] �󂢂Ă���P�ꂪ����܂���");
                return false;
            }
        }
    }


    /// <summary>
    /// �p�[�c�𑗐M����
    /// </summary>
    /// <param name="itemManager">����̃p�[�c�}�l�[�W���[</param>
    /// <param name="partIndex">���M����p�[�c�̗v�f�ԍ�</param>
    /// <returns>���M�ł�����True</returns>
    public bool SendPart(ItemManager itemManager, int partIndex)
    {
        // �󂢂Ă�z��ɒP���ǉ�
        for (int i = 0; i < itemManager.parts.Length; i++)
        {
            if (itemManager.parts[i].Type == ItemType.None)
            {
                itemManager.parts[i] = parts[(int)partIndex];
                Debug.Log($"{parts[(int)partIndex].Name} �𑗐M  [{name}] -> [{itemManager.name}]");

                parts[(int)partIndex] = ItemBase.CreateNoneItem();

                return true;
            }
        }

        Debug.LogWarning($"[{itemManager.name}] �󂢂Ă���p�[�c������܂���");
        return false;
    }


    /// <summary>
    /// �p�[�c�𑗐M����
    /// </summary>
    /// <param name="itemManager">����̃p�[�c�}�l�[�W���[</param>
    /// <param name="partIndex">���M����p�[�c�̗v�f�ԍ�</param>
    public void SendPart(ItemManager itemManager)
    {
        // �����������Ă���p�[�c�������
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Type != ItemType.None)
            {
                SendPart(itemManager, i);
            }
        }
    }



    /// <summary>
    /// �P����Z�b�g����
    /// </summary>
    /// <param name="wordDataIndex">�P��̃f�[�^�v�f�ԍ�</param>
    /// <returns>Set�ł�����True</returns>
    public bool SetWord(int wordDataIndex)
    {
        // �󂢂Ă���P�ꂪ���邩
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type == ItemType.None) {
                words[i] = new WordItem((int)wordDataIndex);
                Debug.Log($"{wordDataBase.datas[(int)wordDataIndex].name} ���Z�b�g [{name}]");
                return true;
            }
        }

        Debug.LogWarning($"[{name}] �󂢂Ă���P�ꂪ����܂���");
        return false;
    }

    /// <summary>
    /// �p�[�c���Z�b�g����
    /// </summary>
    /// <param name="partDataIndex">�P��̃f�[�^�v�f�ԍ�</param>
    /// <returns>Set�ł�����True</returns>
    public bool SetPart(string key, int partDataIndex)
    {
        // �󂢂Ă���P�ꂪ���邩
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Type == ItemType.None)
            {
                parts[i] = new PartItem(key, (int)partDataIndex);
                Debug.Log($"{partDataBase.partStyleDictionary[key][(int)partDataIndex].name} ���Z�b�g [{name}]");
                return true;
            }
        }

        Debug.LogWarning($"[{name}] �󂢂Ă���p�[�c������܂���");
        return false;
    }

    /// <summary>
    /// �p�[�c���Z�b�g����
    /// </summary>
    /// <param name="partDataIndex">�P��̃f�[�^�v�f�ԍ�</param>
    /// <returns>Set�ł�����True</returns>
    public bool SetPart(int id, int partDataIndex)
    {
        // �󂢂Ă���P�ꂪ���邩
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Type == ItemType.None)
            {
                parts[i] = new PartItem(id, (int)partDataIndex);
                Debug.Log($"{partDataBase.partStyleDictionary.ElementAt(id).Value[(int)partDataIndex].name} ���Z�b�g [{name}]");
                return true;
            }
        }

        Debug.LogWarning($"[{name}] �󂢂Ă���p�[�c������܂���");
        return false;
    }


    /// <summary>
    /// �����_���ɒP��𐶐�����
    /// </summary>
    public void RandomGenerateWords()
    {
        // �����_���ɒP����Z�b�g
        for (int i = 0; i < maxWordCount; i++) {
            int wordDataIndex = (int)Random.Range(0, wordDataBase.datas.Count);
            SetWord(wordDataIndex);
        }
    }

    /// <summary>
    /// �����_���Ƀp�[�c�𐶐����� 
    /// </summary>
    public void RandomGenerateParts()
    {
        // �����_���ɒP����Z�b�g
        for (int i = 0; i < maxPartCount; i++)
        {
            //�����ɓo�^����Ă��郊�X�g�������_���Œ��o
            int partDataDictionaryId = (int)Random.Range(0, partDataBase.partStyleDictionary.Count);
            var list = partDataBase.partStyleDictionary.ElementAt(partDataDictionaryId).Value;

            //���o���ꂽ���X�g���烉���_���ŒP�ꒊ�o
            int partDataIndex = (int)Random.Range(0, list.Count);
            SetPart(partDataDictionaryId, partDataIndex);
        }
    }

    /// <summary>
    /// �w�肵�����̒P����h���b�v����
    /// </summary>
    /// <param name="dropCount">�h���b�v��</param>
    /// <param name="dropWordIndices"> �h���b�v�����P��̃C���f�b�N�X���i�[����z��</param>
    public void DropWord(int dropCount)
    {
        for (int i = 0; i < dropCount; i++) {

            bool isBreak = false;


            // �󂢂Ă���P�ꂪ���邩
            while (true) {
                if (IsFullyEmptyWord()) {
                    int wordDataIndex = Random.Range(0, maxWordCount);
                    if (words[i].Type == ItemType.None) continue;

                    words[i] = ItemBase.CreateNoneItem();
                    break;

                } else {
                    Debug.LogWarning($"[{name}] �󂢂Ă���P�ꂪ����܂���");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// �w�肵�����̒P����h���b�v����
    /// </summary>
    /// <param name="dropCount">�h���b�v��</param>
    /// <param name="dropWordIndices"> �h���b�v�����P��̃C���f�b�N�X���i�[����z��</param>
    public void DropWord(int dropCount, ref int[] dropWordIndices)
    {
        for (int i = 0; i < dropCount; i++) {

            bool isBreak = false;

            while (true) {
                // �󂢂Ă���P�ꂪ���邩
                if (IsFullyEmptyWord()) {
                    int wordDataIndex = Random.Range(0, maxWordCount);
                    if (words[i].Type == ItemType.None) continue;

                    dropWordIndices.Append(words[i].Id);
                    words[i] = ItemBase.CreateNoneItem();
                    break;

                } else {
                    Debug.LogWarning($"[{name}] �󂢂Ă���P�ꂪ����܂���");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// �w�肵�����̃p�[�c���h���b�v����
    /// </summary>
    /// <param name="dropCount">�h���b�v��</param>
    public void DropPart(int dropCount)
    {
        for (int i = 0; i < dropCount; i++)
        {

            bool isBreak = false;


            // �󂢂Ă���P�ꂪ���邩
            while (true)
            {
                if (IsFullyEmptyPart())
                {
                    int partDataIndex = Random.Range(0, maxPartCount);
                    if (parts[i].Type == ItemType.None) continue;

                    parts[i] = ItemBase.CreateNoneItem();
                    break;

                }
                else
                {
                    Debug.LogWarning($"[{name}] �󂢂Ă���P�ꂪ����܂���");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// �w�肵�����̃p�[�c���h���b�v����
    /// </summary>
    /// <param name="dropCount">�h���b�v��</param>
    /// <param name="dropPartIndices"> �h���b�v�����p�[�c�̃C���f�b�N�X���i�[����z��</param>
    public void DropPart(int dropCount, ref int[] dropPartIndices)
    {
        for (int i = 0; i < dropCount; i++)
        {

            bool isBreak = false;

            while (true)
            {
                // �󂢂Ă���p�[�c�����邩
                if (IsFullyEmptyPart())
                {
                    int partDataIndex = Random.Range(0, maxPartCount);
                    if (parts[i].Type == ItemType.None) continue;

                    dropPartIndices.Append(parts[i].Id);
                    parts[i] = ItemBase.CreateNoneItem();
                    break;

                }
                else
                {
                    Debug.LogWarning($"[{name}] �󂢂Ă���P�ꂪ����܂���");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// �ő�̒P�ꐔ���擾����
    /// </summary>
    /// <returns>�ő�̒P�ꐔ</returns>
    public int GetMaxWordCount()
    {
        return (int)maxWordCount;
    }


    /// <summary>
    /// �ő�̃p�[�c�����擾����
    /// </summary>
    /// <returns>�ő�̃p�[�c��</returns>
    public int GetMaxPartCount()
    {
        return (int)maxPartCount;
    }
}