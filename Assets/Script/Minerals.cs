using UnityEngine;


public class Minerals : MonoBehaviour
{
    private const string LOG = "[鉱石]";

    [Header("HP")]
    public int maxHP = 100;
    private int currentHP;

    // クリスタル（鉱石）側のアイテム管理（送信元）
    private ItemManager crystalItemManager;
    private Transform player;
    private ItemManager playerItemManager; // プレイヤーのアイテムマネージャーの参照

    [Header("報酬設定")]
    [Tooltip("この鉱石から落とせるワード段階数（=最大配布回数の目安）")]
    public int wordCount = 5;

    // 何段階分ドロップ済みか（Mineで進める）
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


        Debug.Log($"{LOG} 初期化: HP={currentHP}/{maxHP}, wordCount(段階数)={wordCount}");
    }

    /// <summary>
    /// 採掘：HP減少＋段階ドロップ（受け取り先に送る）
    /// 戻り値: 「今回送れたワード数」
    /// </summary>
    public int Mine(int damage, ItemManager receiver)
    {
        if (damage <= 0)
        {
            Debug.LogWarning($"{LOG} Mineが0以下のダメージで呼ばれました: damage={damage}");
            return 0;
        }
        if (currentHP <= 0)
        {
            Debug.LogWarning($"{LOG} 既に破壊済みの鉱石に対してMineが呼ばれました");
            return 0;
        }

        int before = currentHP;
        currentHP = Mathf.Max(0, currentHP - damage);
        //Debug.Log($"{LOG} ダメージ適用: {before} -> {currentHP} (damage={damage})");

        // --- 段階ドロップ計算 ---
        int wc = Mathf.Max(1, wordCount);          // 0ガード
        int stepHP = Mathf.Max(1, maxHP / wc);     // 1段階あたりのHP幅
        int stepsDone = (maxHP - currentHP) / stepHP; // 現在までに達成した段階数
        int pending = Mathf.Max(0, stepsDone - stepsGiven); // 今回新たにドロップすべき段階数

        int dropped = 0;
        if (pending > 0)
        {
            if (crystalItemManager == null || receiver == null)
            {
                Debug.LogWarning($"{LOG} ワード送信不可: crystalItemManager={(crystalItemManager != null)}, receiver={(receiver != null)}");
            }

            for (int i = 0; i < pending; i++)
            {
                if (crystalItemManager != null && receiver != null)
                {
                    bool ok = crystalItemManager.SendWord(receiver); // ランダム1個送信
                    if (ok)
                    {
                        stepsGiven++;
                        dropped++;
                        Debug.Log($"{LOG} ワードを1つ送信しました: 段階 {stepsGiven}/{wc}");
                    }
                    else
                    {
                        Debug.LogWarning($"{LOG} 送信失敗（受け取り先が満杯？）: 以降の送信は次フレームに持ち越し");
                        break; // これ以上は送らず、次フレームに再試行
                    }
                }
                else
                {
                    // 送信元/先がない。stepsGivenは進めず、次フレームに再試行。
                    break;
                }
            }
        }

        // 破壊処理
        if (currentHP <= 0)
        {
            Debug.Log($"{LOG} 破壊: パーツドロップ処理を実行します");
            DropParts(receiver);
            Destroy(gameObject);
        }

        return dropped;
    }


    private void OnTriggerEnter(Collider other)
    {
        // "ドリル" タグと接触したか確認
        if (other.CompareTag("Drill"))
        {
            Debug.Log("ドリルと接触！");
            Mine(10, playerItemManager); // ここでダメージ量を指定（例：10）
        }
    }

    private void DropParts(ItemManager receiver)
    {
        // 必要に応じてパーツ生成を実装
        Debug.Log($"{LOG} DropParts 実行（ここに生成処理を実装）");
        if (crystalItemManager != null && receiver != null)
        {
            crystalItemManager.SendPart(receiver); // ランダム1個送信
        }
    }




    public int GetCurrentHP()
    {
        return currentHP;
    }

}
