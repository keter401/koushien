using UnityEngine;

public class WeekPoint : MonoBehaviour
{
    public int weakPointDamage = 10;

    private Transform player;
    private ItemManager playerItemManager; // �v���C���[�̃A�C�e���}�l�[�W���[�̎Q��

    public void Awake()
    {
        // �v���C���[�̃A�C�e���}�l�[�W���[���擾
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerItemManager = player.GetComponent<ItemManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drill"))
        {
            Minerals ore = GetComponentInParent<Minerals>();
            if (ore != null)
            {
                ore.Mine(weakPointDamage, playerItemManager);
                Debug.Log("��_�q�b�g�I");
            }
        }
    }
}
