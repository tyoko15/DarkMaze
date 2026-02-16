using UnityEngine;

/// <summary>
/// 壊せるオブジェクト（タルなど）を管理するクラス
/// 攻撃を受けると中身のアイテムを生成し、自身は消滅する
/// </summary>
public class BarrelManager : MonoBehaviour
{
    [Header("出現するアイテムの設定")]
    [Header("0:Heart(回復), 1:Arrow(矢), 2:その他")]
    [SerializeField] int itemNum;
    [SerializeField] GameObject[] itemObjects; // 出現候補のプレハブ配列
    GameObject itemObject;                     // 生成されたアイテムのインスタンス

    [Header("アニメーション・消滅設定")]
    [SerializeField] Animator animator;
    public bool destroyFlag;                   // 破壊されたかどうかのフラグ
    [SerializeField] float destroyTime;        // アニメーション開始から消滅までの猶予
    float destroyTimer;

    private Camera mainCamera;
    private GameObject canvas;                 // 頭上に表示するUI（Canvas）
    bool canvasFlag;                           // Canvasを表示して良い状態か

    void Start()
    {
        mainCamera = Camera.main;

        // --- アイテムの事前生成 ---
        itemObject = Instantiate(itemObjects[itemNum], new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
        itemObject.transform.parent = transform.parent; // 親階層を自身と同じにする
        itemObject.SetActive(false);

        // --- UI（Canvas）の初期設定 ---
        // 子オブジェクトの最後にあると想定されるCanvasを取得
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
    }

    void Update()
    {
        // 破壊フラグが立っている場合
        if (destroyFlag)
        {
            animator.SetBool("Destroy", destroyFlag); // 破壊アニメーション開始
            if (destroyTimer > destroyTime)
            {
                // 指定時間経過でオブジェクトを完全に削除
                Destroy(gameObject);
                // 削除直前に物理判定をトリガー化してすり抜けるようにする（念のため）
                gameObject.GetComponent<Collider>().isTrigger = true;
            }
            else if (destroyTimer < destroyTime)
            {
                destroyTimer += Time.deltaTime;
            }
        }

        // 破壊後などはCanvasを表示しないように強制制御
        if (!canvasFlag) canvas.SetActive(false);
    }

    /// <summary>
    /// UI（Canvas）を常にカメラの方向へ向かせる（ビルボード処理）
    /// </summary>
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // カメラの方を向くように回転を計算
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z); // Y軸（横回転）をメインに調整
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    /// <summary>
    /// 攻撃判定（Attackタグ）を持つオブジェクトが触れたら破壊
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Attack")
        {
            itemObject.SetActive(true); // 隠していたアイテムを出現させる
            destroyFlag = true;         // 破壊開始
            canvasFlag = false;         // UIを永久に消す
        }
    }

    /// <summary>
    /// プレイヤーが接触している間だけUI（Canvas）を表示
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(true);
    }

    /// <summary>
    /// プレイヤーが離れたらUIを非表示
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(false);
    }
}
