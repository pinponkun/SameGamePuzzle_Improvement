using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SameGamePuzzleSceneDirector : MonoBehaviour
{
    // アイテムのプレハブ
    [SerializeField] List<GameObject> prefabBubbles;
    // ゲーム時間
    [SerializeField] float gameTimer;
    // フィールドのアイテム総数
    [SerializeField] int fieldItemCountMax;
    // 削除できるアイテム数
    [SerializeField] int deleteCount;

    // UI
    [SerializeField] TextMeshProUGUI textGameScore;
    [SerializeField] TextMeshProUGUI textGameTimer;
    [SerializeField] GameObject panelGameResult;
    // Audio
    [SerializeField] AudioClip seBubble;
    [SerializeField] AudioClip seSpecial;

    // フィールド上のアイテム
    List<GameObject> bubbles;
    // スコア
    int gameScore;
    // 再生装置
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // 全アイテム
        bubbles = new List<GameObject>();

        // リザルト画面非表示
        panelGameResult.SetActive(false);

        // アイテム生成
        SpawnItem(fieldItemCountMax);
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームタイマー更新
        gameTimer -= Time.deltaTime;
        textGameTimer.text = "" + (int)gameTimer;

        // ゲーム終了
        if (0 > gameTimer)
        {
            // リザルト画面表示
            panelGameResult.SetActive(true);
            // Updateに入らないようにする
            enabled = false;
            // この時点でUpdateから抜ける
            return;
        }

        // タッチ処理
        if (Input.GetMouseButtonUp(0))
        {
            // スクリーン座標からワールド座標に変換
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2d = Physics2D.Raycast(worldPoint, Vector2.zero);

            // 削除されたアイテムをクリア
            bubbles.RemoveAll(item => item == null);

            // 何か当たり判定があれば
            if(hit2d)
            {
                // 当たり判定があったオブジェクト
                GameObject obj = hit2d.collider.gameObject;
                CheckItems(obj);
            }
        }
    }

    // アイテム生成
    void SpawnItem(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 色ランダム
            int rnd = Random.Range(0, prefabBubbles.Count);
            // 場所ランダム
            float x = Random.Range(-2.0f, 2.0f);
            float y = Random.Range(-2.0f, 2.0f);

            // アイテム生成
            GameObject bubble = Instantiate(prefabBubbles[rnd], new Vector3(x, 7 + y, 0), Quaternion.identity);

            // 内部データ追加
            bubbles.Add(bubble);
        }
    }

    // 引数と同じ色のアイテムを削除する
    void DeleteItems(List<GameObject> checkItems)
    {
        // 削除可能数に達していなかったらなにもしない
        if (checkItems.Count < deleteCount) return;

        // ボーナスとしてオーバーしたカウント*5個削除
        int overCount = checkItems.Count - deleteCount;
        overCount *= 5;

        // ランダムなアイテムを削除リストに追加（被りの可能性あり）
        while (overCount > 0)
        {
            int rnd = Random.Range(0, bubbles.Count);
            checkItems.Add(bubbles[rnd]);
            overCount--;
        }

        // 削除してスコア加算
        List<GameObject> destroyItems = new List<GameObject>();
        foreach (var item in checkItems)
        {
            // 被り無しの削除したアイテムをカウント
            if (!destroyItems.Contains(item))
            {
                destroyItems.Add(item);
            }

            // 削除
            Destroy(item);
        }

        // 実際に削除した分生成してスコア加算
        SpawnItem(destroyItems.Count);
        gameScore += destroyItems.Count * 100;

        // スコア表示更新
        textGameScore.text = "" + gameScore;
    }

    // 同じ色のアイテムを返す
    List<GameObject> GetSameItems(GameObject target)
    {
        List<GameObject> ret = new List<GameObject>();

        foreach (var item in bubbles)
        {
            // アイテムがない、同じアイテム、違う色、距離が遠い場合はスキップ
            if (!item || target == item) continue;

            if (item.GetComponent<SpriteRenderer>().sprite != target.GetComponent<SpriteRenderer>().sprite)
            {
                continue;
            }

            float distance = Vector2.Distance(target.transform.position, item.transform.position);

            if (distance > 1.1f) continue;

            // ここまできたらアイテム追加
            ret.Add(item);
        }

        return ret;
    }

    // 引数と同じ色のアイテムを探す
    void CheckItems(GameObject target)
    {
        // このアイテムと同じ色を追加する
        List<GameObject> checkItems = new List <GameObject>();
        // 自分を追加
        checkItems.Add(target);

        // チェック済のインデックス
        int checkIndex = 0;

        // checkItemsの最大値までループ
        while (checkIndex < checkItems.Count)
        {
            // 隣接する同じ色を取得
            List<GameObject> sameItems = GetSameItems(checkItems[checkIndex]);
            // チェック済のインデックスを進める
            checkIndex++;

            // まだ追加されていないアイテムを追加する
            foreach (var item in sameItems)
            {
                if (checkItems.Contains(item)) continue;
                checkItems.Add(item);
            }

            // 削除
            DeleteItems(checkItems);
        }
    }

    // リトライボタン
    public void OnClickRetry()
    {
        SceneManager.LoadScene("SameGamePuzzleScene");
    }
}
