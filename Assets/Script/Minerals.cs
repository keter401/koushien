using UnityEngine;


public class Minerals : MonoBehaviour
{
    private const string LOG = "[�z��]";

    [Header("HP")]
    public int maxHP = 100;
    private int currentHP;

    // �N���X�^���i�z�΁j���̃A�C�e���Ǘ��i���M���j
    private ItemManager crystalItemManager;
    private Transform player;
    private ItemManager playerItemManager; // �v���C���[�̃A�C�e���}�l�[�W���[�̎Q��

    [Header("��V�ݒ�")]
    [Tooltip("���̍z�΂��痎�Ƃ��郏�[�h�i�K���i=�ő�z�z�񐔂̖ڈ��j")]
    public int wordCount = 5;

    // ���i�K���h���b�v�ς݂��iMine�Ői�߂�j
    private int stepsGiven = 0;

    private void Start()
    {
        currentHP = maxHP;

        crystalItemManager = GetComponent<ItemManager>();


        player = GameObject.FindGameObjectWithTag("Player").transform.parent;
        playerItemManager = player.GetComponent<ItemManager>();
        if (crystalItemManager)
        {
            crystalItemManager.RandomGenerateWords();
            int wc = crystalItemManager.GetMaxWordCount();
            if (wc > 0) wordCount = wc;

            crystalItemManager.RandomGenerateParts();
        }


        Debug.Log($"{LOG} ������: HP={currentHP}/{maxHP}, wordCount(�i�K��)={wordCount}");
    }

    /// <summary>
    /// �̌@�FHP�����{�i�K�h���b�v�i�󂯎���ɑ���j
    /// �߂�l: �u���񑗂ꂽ���[�h���v
    /// </summary>
    public int Mine(int damage, ItemManager receiver)
    {
        if (damage <= 0)
        {
            Debug.LogWarning($"{LOG} Mine��0�ȉ��̃_���[�W�ŌĂ΂�܂���: damage={damage}");
            return 0;
        }
        if (currentHP <= 0)
        {
            Debug.LogWarning($"{LOG} ���ɔj��ς݂̍z�΂ɑ΂���Mine���Ă΂�܂���");
            return 0;
        }

        int before = currentHP;
        currentHP = Mathf.Max(0, currentHP - damage);
        //Debug.Log($"{LOG} �_���[�W�K�p: {before} -> {currentHP} (damage={damage})");

        // --- �i�K�h���b�v�v�Z ---
        int wc = Mathf.Max(1, wordCount);          // 0�K�[�h
        int stepHP = Mathf.Max(1, maxHP / wc);     // 1�i�K�������HP��
        int stepsDone = (maxHP - currentHP) / stepHP; // ���݂܂łɒB�������i�K��
        int pending = Mathf.Max(0, stepsDone - stepsGiven); // ����V���Ƀh���b�v���ׂ��i�K��

        int dropped = 0;
        if (pending > 0)
        {
            if (crystalItemManager == null || receiver == null)
            {
                Debug.LogWarning($"{LOG} ���[�h���M�s��: crystalItemManager={(crystalItemManager != null)}, receiver={(receiver != null)}");
            }

            for (int i = 0; i < pending; i++)
            {
                if (crystalItemManager != null && receiver != null)
                {
                    bool ok = crystalItemManager.SendWord(receiver); // �����_��1���M
                    if (ok)
                    {
                        stepsGiven++;
                        dropped++;
                        Debug.Log($"{LOG} ���[�h��1���M���܂���: �i�K {stepsGiven}/{wc}");
                    }
                    else
                    {
                        Debug.LogWarning($"{LOG} ���M���s�i�󂯎��悪���t�H�j: �ȍ~�̑��M�͎��t���[���Ɏ����z��");
                        break; // ����ȏ�͑��炸�A���t���[���ɍĎ��s
                    }
                }
                else
                {
                    // ���M��/�悪�Ȃ��BstepsGiven�͐i�߂��A���t���[���ɍĎ��s�B
                    break;
                }
            }
        }

        // �j�󏈗�
        if (currentHP <= 0)
        {
            Debug.Log($"{LOG} �j��: �p�[�c�h���b�v���������s���܂�");
            DropParts(receiver);
            Destroy(gameObject);
        }

        return dropped;
    }


    private void OnTriggerEnter(Collider other)
    {
        // "�h����" �^�O�ƐڐG�������m�F
        if (other.CompareTag("Drill"))
        {
            Debug.Log("�h�����ƐڐG�I");
            Mine(10, playerItemManager); // �����Ń_���[�W�ʂ��w��i��F10�j
        }
    }

    private void DropParts(ItemManager receiver)
    {
        // �K�v�ɉ����ăp�[�c����������
        Debug.Log($"{LOG} DropParts ���s�i�����ɐ��������������j");
        if (crystalItemManager != null && receiver != null)
        {
            crystalItemManager.SendPart(receiver); // �����_��1���M
        }
    }




    public int GetCurrentHP()
    {
        return currentHP;
    }

}
