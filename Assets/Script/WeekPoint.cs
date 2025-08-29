using UnityEngine;

public class WeekPoint : MonoBehaviour
{
    public int weakPointDamage = 10;

    private Transform player;
    private ItemManager playerItemManager; // プレイヤーのアイテムマネージャーの参照

    public void Awake()
    {
        // プレイヤーのアイテムマネージャーを取得
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
                Debug.Log("弱点ヒット！");
            }
        }
    }
}
