using UnityEngine;

/// <summary>
/// プレイヤーが発射する矢の移動、衝突、消滅を管理するクラス
/// </summary>
public class ArrowManager : MonoBehaviour
{
    [Header("参照・コンポーネント")]
    [SerializeField] GameObject arrowObject;    // 矢の本体
    [SerializeField] GameObject playerObject;   // プレイヤー（方向計算用）
    [SerializeField] GameObject spawerObject;   // 発射位置（方向計算用）
    [SerializeField] Rigidbody rb;
    [SerializeField] BoxCollider boxCollider;

    [Header("移動設定")]
    [SerializeField] public float speed;        // 矢の速度
    [SerializeField] Vector3 position;          // 衝突時の座標保存用
    [SerializeField] Vector3 direction;         // 矢が進む方向
    [SerializeField] Vector3 rotate;            // 矢の回転値

    [Header("状態フラグ")]
    public bool stopFlag;                       // 何かに当たって停止しているか
    public bool hitFlag;                        // スイッチ等に命中したか

    [Header("消滅設定")]
    [SerializeField] public float lostTime;     // 消滅までの時間
    float lostTimer;

    void Start()
    {
        // シーン内からプレイヤーと発射ポイントを検索
        playerObject = GameObject.Find("Player");
        spawerObject = GameObject.Find("Spawer");

        // --- 進行方向の計算 ---
        // 発射地点からプレイヤーの位置を引くことで逆説的に前方向へのベクトルを算出
        direction = (spawerObject.transform.position - playerObject.transform.position).normalized;
        
        // プレイヤーの向きに合わせて矢の回転を設定
        rotate = new Vector3(playerObject.transform.eulerAngles.x, playerObject.transform.eulerAngles.y, playerObject.transform.eulerAngles.z);
        arrowObject.transform.rotation = Quaternion.Euler(rotate);
    }

    void Update()
    {
        arrowControl();
    }

    /// <summary>
    /// 矢の移動と寿命による消滅管理
    /// </summary>
    void arrowControl()
    {
        // 1. 移動中
        if (!stopFlag)
        {
            float z = speed * Time.deltaTime;
            Vector3 force = direction * z;

            // 速度ベクトルを加算して移動させる
            rb.linearVelocity += force;
            
            // 寿命チェック
            if (lostTimer > lostTime)
            {
                Destroy(arrowObject);
            }
            else if (lostTimer < lostTime)
            {
                lostTimer += Time.deltaTime;
            }
        }
        // 2. 衝突・停止後
        else
        {
            // 物理移動を停止させ、トリガー判定をオフにする（突き刺さった状態）
            rb.linearVelocity = Vector3.zero;
            boxCollider.isTrigger = false;

            // 停止後も寿命が来たら消滅
            if (lostTimer > lostTime)
            {
                Destroy(arrowObject);
            }
            else if (lostTimer < lostTime)
            {
                lostTimer += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 何かに当たった瞬間の処理
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // ギミックボタンに当たった場合
        if (collision.gameObject.tag == "Button")
        {
            stopFlag = true;
            hitFlag = true;
        }
        else stopFlag = true;

        // 衝突位置を記録
        position = arrowObject.transform.position;

        // --- 突き刺さる演出 ---
        // 当たった対象の子供にすることで、対象が動いても矢が離れないようにする
        transform.parent = collision.transform;
    }
}
