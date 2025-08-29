// 制作者: 濵田 芳辰

using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private WordDataBase wordDataBase;     // ワードデータベース
    [SerializeField] private int maxWordCount = 3;         // 最大単語数
    [SerializeField] private int maxPartCount = 3;         // 最大パーツ数
    [SerializeField] private ItemBase[] words;        // 単語
    [SerializeField] private ItemBase[] parts;        // パーツ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        words = new ItemBase[maxWordCount];

        // 単語を初期化
        for (int i = 0; i < maxWordCount; i++) {
            words[i] = ItemBase.CreateNoneItem();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }


    /// <summary>
    /// 空いている単語があるかを確認する
    /// </summary>
    /// <returns>空いてたらTrue</returns>
    public bool HasEmptyWord()
    {
        // 空いている単語があるか
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type == ItemType.None) {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 空いている単語がないかを確認する
    /// </summary>
    /// <returns>全部埋まってなかったらTrue</returns>
    public bool IsFullyEmptyWord()
    {
        // 空いている単語がないか
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type != ItemType.None) {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// 単語を送信する
    /// </summary>
    /// <param name="itemManager">相手のワードマネージャー</param>
    /// <param name="wordIndex">送信するワードの要素番号</param>
    /// <returns>送信できたらTrue</returns>
    public bool SendWord(ItemManager itemManager, int wordIndex)
    {
        // 空いてる配列に単語を追加
        for (int i = 0; i < itemManager.words.Length; i++) {
            if (itemManager.words[i].Type == ItemType.None) {
                itemManager.words[i] = words[(int)wordIndex];
                Debug.Log($"{words[(int)wordIndex].Name} を送信  [{name}] -> [{itemManager.name}]");

                words[(int)wordIndex] = ItemBase.CreateNoneItem();

                return true;
            }
        }

        Debug.LogWarning($"[{itemManager.name}] 空いている単語がありません");
        return false;
    }


    /// <summary>
    /// 単語をランダムに送信する
    /// </summary>
    /// <param name="itemManager">ワードの管理クラス</param>
    /// <returns>送信できたらTrue</returns>
    public bool SendWord(ItemManager itemManager)
    {
        if (IsFullyEmptyWord()) return false;


        // ランダムに単語を選ぶ
        while (true) {
            int wordIndex = (int)Random.Range(0, maxWordCount);
            if (words[wordIndex].Type == ItemType.None) continue;

            if (SendWord(itemManager, wordIndex)) {
                return true;
            } else {
                Debug.LogWarning($"[{itemManager.name}] 空いている単語がありません");
                return false;
            }
        }
    }


    /// <summary>
    /// 単語をセットする
    /// </summary>
    /// <param name="wordDataIndex">単語のデータ要素番号</param>
    /// <returns>SetできたらTrue</returns>
    public bool SetWord(int wordDataIndex)
    {
        // 空いている単語があるか
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Type == ItemType.None) {
                words[i] = new WordItem((int)wordDataIndex);
                Debug.Log($"{wordDataBase.datas[(int)wordDataIndex].name} をセット [{name}]");
                return true;
            }
        }

        Debug.LogWarning($"[{name}] 空いている単語がありません");
        return false;
    }

    /// <summary>
    /// ランダムに単語を生成する
    /// </summary>
    public void RandomGenerateWords()
    {
        // ランダムに単語をセット
        for (int i = 0; i < maxWordCount; i++) {
            int wordDataIndex = (int)Random.Range(0, wordDataBase.datas.Count);
            SetWord(wordDataIndex);
        }
    }

    /// <summary>
    /// 指定した数の単語をドロップする
    /// </summary>
    /// <param name="dropCount">ドロップ数</param>
    /// <param name="dropWordIndices"> ドロップした単語のインデックスを格納する配列</param>
    public void DropWord(int dropCount)
    {
        for (int i = 0; i < dropCount; i++) {

            bool isBreak = false;


            // 空いている単語があるか
            while (true) {
                if (IsFullyEmptyWord()) {
                    int wordDataIndex = Random.Range(0, maxWordCount);
                    if (words[i].Type == ItemType.None) continue;

                    words[i] = ItemBase.CreateNoneItem();
                    break;

                } else {
                    Debug.LogWarning($"[{name}] 空いている単語がありません");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// 指定した数の単語をドロップする
    /// </summary>
    /// <param name="dropCount">ドロップ数</param>
    /// <param name="dropWordIndices"> ドロップした単語のインデックスを格納する配列</param>
    public void DropWord(int dropCount, ref int[] dropWordIndices)
    {
        for (int i = 0; i < dropCount; i++) {

            bool isBreak = false;

            while (true) {
                // 空いている単語があるか
                if (IsFullyEmptyWord()) {
                    int wordDataIndex = Random.Range(0, maxWordCount);
                    if (words[i].Type == ItemType.None) continue;

                    dropWordIndices.Append(words[i].Id);
                    words[i] = ItemBase.CreateNoneItem();
                    break;

                } else {
                    Debug.LogWarning($"[{name}] 空いている単語がありません");
                    isBreak = true;

                    break;
                }
            }

            if (isBreak) break;
        }
    }


    /// <summary>
    /// 最大の単語数を取得する
    /// </summary>
    /// <returns>最大の単語数</returns>
    public int GetMaxWordCount()
    {
        return (int)maxWordCount;
    }
}