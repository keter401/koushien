// �����: �M�c �F�C

using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private WordDataBase wordDataBase;     // ���[�h�f�[�^�x�[�X
    [SerializeField] private int maxWordCount = 3;         // �ő�P�ꐔ
    [SerializeField] private int maxPartCount = 3;         // �ő�p�[�c��
    [SerializeField] private ItemBase[] words;        // �P��
    [SerializeField] private ItemBase[] parts;        // �p�[�c

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        words = new ItemBase[maxWordCount];

        // �P���������
        for (int i = 0; i < maxWordCount; i++) {
            words[i] = ItemBase.CreateNoneItem();
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
    /// �ő�̒P�ꐔ���擾����
    /// </summary>
    /// <returns>�ő�̒P�ꐔ</returns>
    public int GetMaxWordCount()
    {
        return (int)maxWordCount;
    }
}