using UnityEngine;

/// <summary>
/// 宝箱の挙動を管理するクラス
/// 攻撃を受けると蓋が開き、アイテムフラグをプレイヤーに与える
/// </summary>
public class ChestManger : MonoBehaviour
{
    [Header("オブジェクト参照")]
    [SerializeField] GameObject chestObject;      // 宝箱全体
    GameObject chestTopObject;                   // 宝箱の「蓋」の部分
    Vector3 originRotation;                      // 蓋の初期角度
    [SerializeField] GameObject player;           // プレイヤーへの参照

    [Header("アイテム設定")]
    // 0.Bow&Arrow 1.Rope
    [SerializeField] GameObject[] itemPrefabs;
    GameObject itemAnimeObject;
    [SerializeField] int itemNum;                // どのアイテムを解放するか
    bool openFlag;                               // 開いている最中か
    [SerializeField] float openedTime;           // 開き始めてから消滅するまでの時間
    float openedTimer;
    bool animeFlag;
    float originY;
    [SerializeField] float animeTime;
    float animeTimer;
    [SerializeField] bool hideFlag;              // 最初は隠しておくか

    private Camera mainCamera;
    private GameObject canvas;                   // 近づいた時のUI
    bool canvasFlag;
    void Start()
    {
        // 初期状態で隠す設定なら非アクティブにする
        if (hideFlag) chestObject.SetActive(false);

        // 子オブジェクトの0番目を「蓋」として取得（階層構造に依存
        if (chestObject.transform.childCount > 1) chestTopObject = chestObject.transform.GetChild(0).gameObject;
        originRotation = chestTopObject.transform.eulerAngles;

        // UI（キャンバス）の設定
        mainCamera = Camera.main;
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
    }

    void Update()
    {
        if (openFlag)        
        {
            // 消滅タイマーが規定時間を超えたら削除
            if (openedTimer > openedTime)
            {
                openedTimer = 0;
                Destroy(chestObject);
            }
            else
            {
                openedTimer += Time.deltaTime;
                // --- 蓋が開くアニメーション処理 ---
                if (chestTopObject != null)
                {
                    // 開き始めてから前半（50%）の時間で回転させる
                    if (openedTimer > openedTime * 0.5f) chestTopObject.transform.eulerAngles = new Vector3(290f, originRotation.y, originRotation.z);
                    else if (openedTimer < openedTime * 0.5f)
                    {
                        // 360度から290度へ滑らかに変化（Mathf.Lerp）
                        float x = Mathf.Lerp(360f, 290f, openedTimer / (openedTime * 0.5f));
                        chestTopObject.transform.eulerAngles = new Vector3(x, originRotation.y, originRotation.z);
                    }
                }
            }
            // 演出
            ItemAnime();
        }
    }

    /// <summary>
    /// 開いた時アイテムを見せる演出
    /// </summary>
    void ItemAnime()
    {
        if (animeFlag)
        {
            if (animeTimer == 0)
            {
                itemAnimeObject = Instantiate(itemPrefabs[itemNum], new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), Quaternion.identity);
                itemAnimeObject.transform.parent = transform;
                itemAnimeObject.transform.eulerAngles = new Vector3(30f, 180f, 0f);
                originY = transform.position.y;
            }
            if (animeTimer > animeTime)
            {
                Destroy(itemAnimeObject);
                animeFlag = false;
                animeTimer = 0;
            }
            else
            {
                animeTimer += Time.deltaTime;
                Vector3 itemPos = transform.position;
                itemPos.y = Mathf.Lerp(originY, originY + 3.5f, animeTimer / animeTime);
                itemAnimeObject.transform.position = itemPos;
            }
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null && !canvasFlag) return;

        // カメラの方向を向く
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z);
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    // --- 接触判定 ---

    private void OnCollisionEnter(Collision collision)
    {
        // 攻撃タグのオブジェクトが当たったら開く
        if (collision.gameObject.tag == "Attack" && collision.gameObject.name == "Sword")
        {
            if (!openFlag) openFlag = true;
            animeFlag = true;
            player.GetComponent<PlayerController>().canItemFlag[itemNum] = true;
            collision.gameObject.SetActive(false);
        }
        // プレイヤーが近づいたらUI表示
        if (collision.gameObject == player && canvasFlag) canvas.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // トリガー判定の攻撃（剣だけ）開く
        if (other.gameObject.tag == "Attack" && other.gameObject.name == "Sword")
        {
            if (!openFlag) openFlag = true;
            animeFlag = true;
            player.GetComponent<PlayerController>().canItemFlag[itemNum] = true;
            other.gameObject.SetActive(false);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // プレイヤーが離れたらUI非表示
        if (collision.gameObject == player && canvasFlag) canvas.SetActive(false);
    }
}
