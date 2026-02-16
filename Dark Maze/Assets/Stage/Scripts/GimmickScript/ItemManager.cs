using UnityEngine;

/// <summary>
/// フィールド上のアイテムの挙動（回転・取得）を管理するクラス
/// </summary>
public class ItemManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] public int itemNum;        // アイテムの識別番号（どのアイテムか）
    [SerializeField] GameObject itemObject;    // 回転・消去させるアイテムのモデル
    [SerializeField] float rotateTime;         // 1回転（360度）にかける時間
    float rotateTimer;
    bool endFlag;                              // 二重取得を防止するためのフラグ

    void Update()
    {
        // --- アイテムをY軸を中心に回転させる演出 ---
        if (rotateTimer > rotateTime)
        {
            // タイマーをリセットして回転をループさせる
            rotateTimer = 0;
        }
        else if (rotateTimer < rotateTime)
        {
            rotateTimer += Time.deltaTime;

            // Mathf.Lerpを使って0度から360度まで滑らかに補間
            float y = Mathf.Lerp(0f, 360f, rotateTimer / rotateTime);

            // 計算した角度をモデルに適用
            itemObject.transform.eulerAngles = new Vector3(0f, y, 0f);
        }
    }

    /// <summary>
    /// プレイヤーがアイテム（のコライダー）に触れた時の処理
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 接触相手がプレイヤーかつ、まだ取得されていない場合
        if (other.gameObject.tag == "Player" && !endFlag)
        {
            // ヒエラルキーから"Player"を探して取得
            GameObject player = GameObject.Find("Player");

            // プレイヤーのコントローラーにアイテム番号を渡して取得処理を実行
            player.GetComponent<PlayerController>().GetItemControl(itemNum);

            // フィールドからアイテムのモデルを削除
            Destroy(itemObject);

            // 取得済みフラグを立てる（Destroyは即時ではないため、1フレーム内の多重検知を防ぐ）
            endFlag = true;
        }
    }
}
