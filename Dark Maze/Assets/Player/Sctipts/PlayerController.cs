using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの移動、アクション（攻撃・ライト・アイテム使用）、HP管理を統括するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ===== GameManger制御用 =====
    [Header("GameManger制御用")]
    [SerializeField] public int status;                 // 0.start 1.play 2.stop 3.over 4.clear

    // ===== AudioManager =====
    [Header("AudioManager")]
    AudioManager audioManager;                          // 音響管理

    // ===== プレイヤーの基本情報 =====
    [Header("プレイヤーの基本情報")]
    [SerializeField] GameObject playerObject;           // プレイヤー本体
    [SerializeField] Animator animator;                 // アニメーション制御
    [SerializeField] Rigidbody rb;                      // 物理演算
    [SerializeField] Vector3 gravity;                   // カスタム重力
    [SerializeField] float playerHorizontal;            // 入力：水平
    [SerializeField] float playerVertical;              // 入力：垂直
    [SerializeField] float playerSpeed;                 // 移動速度
    [SerializeField] public int clearStageNum;          // 現在のクリアステージ数

    // ===== HP情報 =====
    [Header("HP情報")]
    [SerializeField] float maxPlayerHp;                      // 最大HP
    [SerializeField] public float playerHP;                  // 現在のHP
    public float beforeHP;                                   // 変化前のHP（演出用）
    public float afterHP;                                    // 変化後のHP（演出用）
    public float damageAmount;                               // ダメージ量
    public bool damageFlag;                                  // ダメージ発生フラグ
    bool farstDamageFlag;                                    // ダメージ開始フラグ
    [SerializeField] float damageTime;                       // ダメージ演出時間
    float damageTimer;                                       // ダメージ演出用タイマー
    public float recoveryAmount;                             // 回復量
    public bool recoveryFlag;                                // 回復発生フラグ
    bool farstRecoveryFlag;                                  // 回復開始フラグ
    [SerializeField] float recoveryTime;                     // 回復演出時間
    float recoveryTimer;                                     // 回復演出用タイマー
    [SerializeField] Image playerHpGauge;                    // メインHPゲージUI
    [SerializeField] Image playerDamageGauge;                // ダメージ表示UI
    [SerializeField] Image playerRecoveryGauge;              // 回復表示UI
    [SerializeField] TextMeshProUGUI playerHpText;           // HP数値テキスト

    // ===== Camera情報 =====
    [Header("Camera情報")]
    [SerializeField] GameObject mainCamera;                   // 追従カメラ

    // ===== プレイヤーのライト情報 =====
    [Header("プレイヤーのライト情報")]
    [SerializeField] Light playerLight;                      // スポットライト
    [SerializeField] GameObject lightRangeObject;            // ライト当たり判定オブジェクト
    [SerializeField] Image lightGauge;                       // ライトゲージUI
    [SerializeField, Range(30f, 180f)] float playerLightRange; // ライト照射角
    [SerializeField, Range(2f, 18f)] float lightRangeObjectRange; // 判定サイズ
    [Header("最拡大するまでにかかる秒数"), SerializeField] float lightSpreadTime;
    float lightSpreadTimer;                                  // 拡大用タイマー
    [Header("最縮小するまでにかかる秒数"), SerializeField] float lightShrinkTime;
    float lightRangeMinRange;                                // 縮小開始時の角度
    float lightShrinkTimer;                                  // 縮小用タイマー
    [SerializeField] Image lightMaxIntervalTimerGauge;       // インターバルUI
    [SerializeField] float lightMaxIntervalTime;              // 最大拡大後の待機時間
    float lightMaxIntervalTimer;                             // インターバルタイマー
    int onLight;                                             // ライト状態（0:縮小 1:拡大 2:最大維持）

    // ===== プレイヤーの攻撃情報 =====
    [Header("プレイヤーの攻撃情報")]
    [SerializeField] bool attackFlag;                        // 攻撃中フラグ
    [SerializeField] GameObject sword;                       // 武器オブジェクト

    // ===== プレイヤーのアイテム情報 =====
    [Header("プレイヤーのアイテム情報")]
    [SerializeField] int maxArrowCount;                      // 最大所持矢数
    public int arrowCount;                                   // 現在の矢数

    // ===== アイテム選択情報 =====
    [Header("アイテム選択情報")]
    bool getItemFlag;                                        // アイテム取得フラグ
    [SerializeField] public bool[] canItemFlag;              // 使用可能フラグ（0.弓、1.縄）
    [SerializeField] GameObject[] itemSlots;                 // スロットUI
    [SerializeField] Sprite[] itemImageSprites;              // アイテムアイコン
    [SerializeField] GameObject itemSelect;                   // 選択中アイコンUI
    [SerializeField] GameObject selectObject;                // 選択カーソルUI
    [SerializeField] GameObject itemCountText;                // 残数テキストUI
    [SerializeField] GameObject itemIntervalObject;          // アイテム使用不可UI
    [SerializeField] int itemSelectNum;                      // 現在選択中のインデックス
    bool itemSelectFlag;                                     // セレクト画面表示中か
    bool startSelectFlag;                                    // セレクト開始演出
    bool endSelectFlag = true;                               // セレクト終了演出
    [SerializeField] float itemTime;                         // 出現演出時間
    float[] itemTimer = new float[3];                        // スロット別タイマー
    bool itemIntervalFlag;                                   // 使用間隔フラグ
    [SerializeField] float itemIntervalTime;                 // 使用間隔（クールタイム）
    float itemIntervalTimer;                                 // クールタイム用タイマー

    // ===== アイテム使用情報 =====
    [Header("アイテム使用情報")]
    bool itemUseFlag;                                        // 使用開始フラグ
    bool endUseFlag;                                         // 使用終了フラグ
    Vector3 itemUseDirection;                                // 使用時の向き

    // ===== 弓矢詳細 =====
    [Header("弓矢詳細")]
    [SerializeField] GameObject bow;                         // 弓本体
    [SerializeField] GameObject arrowSpawer;                 // 矢の生成位置
    [SerializeField] GameObject arrowPrefab;                 // 矢のプレハブ
    [SerializeField] GameObject arrowObject;                 // 生成された矢
    [SerializeField] bool arrowAnimeFlag;                    // 矢の演出中か

    // ===== 縄詳細 =====
    [Header("縄詳細")]
    [SerializeField] LayerMask woodLayer;                    // ターゲット（木）のレイヤー
    bool betweenObjectFlag;                                  // 障害物の有無
    GameObject betweenObject;                                // 遮蔽物
    [SerializeField] GameObject ropePrefab;                  // 縄プレハブ
    [SerializeField] GameObject ropeObject;                  // 縄実体
    [SerializeField] GameObject[] ropeTargetObjects;         // ターゲット候補
    [SerializeField] GameObject rangeRopeTargetObject;       // 射程内のターゲット
    [SerializeField] GameObject handObject;                  // 縄を持つ手
    [SerializeField] GameObject ropeAnimeObject;             // 縄の演出用
    float handRadius = 0.1f;                                 // 手の回転半径
    float ropeRadius = 1f;                                   // 縄の回転半径
    float handSpeed = 370f;                                  // 手の回転速度
    float ropeSpeed = 360f;                                  // 縄の回転速度
    float handAngle;                                         // 手の現在角度
    float ropeAngle;                                         // 縄の現在角度
    [SerializeField] LineRenderer line;                      // 縄の描画
    Vector3[] points;                                        // LineRendererの座標群
    [SerializeField] int segmentCount = 2;                   // 縄の分割数
    Vector3 originPlayerObjectPosition;                      // 移動開始時の座標
    Vector3 rangeRopeTargetPosition;                         // 縄の接地点
    [SerializeField] bool ropeMoveFlag;                      // 縄移動中か
    [SerializeField] float ropeMoveTime;                     // 縄移動にかかる時間
    float ropeMoveTimer;                                     // 縄移動用タイマー

    // ===== 特殊効果・環境 =====
    [Header("特殊効果")]
    [SerializeField] GameObject[] respawnPositions;          // 砂に飲まれた時の復帰地点
    int playerPosiNum;                                       // 現在のエリア番号
    [SerializeField] LayerMask sandLayer;                    // 砂地レイヤー
    bool onSandFlag;                                         // 砂搭乗中フラグ
    [SerializeField] float sandTime;                         // 沈むまでの時間
    float sandTimer;                                         // 沈下タイマー
    Vector3 originPosition;                                  // 沈み始めの座標

    // ===== エフェクト =====
    [Header("Effect")]
    [SerializeField] GameObject maxEffectOrigin;             // ライト最大時プレハブ
    [SerializeField] GameObject clearEffectOrigin;           // クリア時プレハブ
    [SerializeField] GameObject overEffectOrigin;            // ゲームオーバー時プレハブ
    GameObject maxEffect;                                    // 最大エフェクト実体
    GameObject clearEffect;                                  // クリアエフェクト実体
    GameObject overEffect;                                   // オーバーエフェクト実体
    [SerializeField] float maxEffectTime;                    // エフェクト持続（最大）
    float maxEffectTimer;                                    // タイマー
    bool onMaxEffectFlag;                                    // エフェクト表示中
    [SerializeField] float overEffectTime;                   // エフェクト持続（オーバー）
    float overEffectTimer;
    bool onOverEffectFlag;

    // ===== Input =====
    public InputActionAsset inputAsset;
    InputAction[] inputActions;

    void Awake()
    {
        inputActions = new InputAction[7];
        inputActions[0] = inputAsset.FindAction("PlayerControl");
        inputActions[1] = inputAsset.FindAction("LightButton");
        inputActions[2] = inputAsset.FindAction("AttackButton");
        inputActions[3] = inputAsset.FindAction("ItemSelectButton");
        inputActions[4] = inputAsset.FindAction("ItemSelectControl");
        inputActions[5] = inputAsset.FindAction("UseItemButton");
        inputActions[6] = inputAsset.FindAction("UseItemControl");
    }

    void OnEnable()
    {
        for (int i = 0; i < inputActions.Length; i++) inputActions[i].Enable();

        inputActions[0].started += InputPlayerControl;
        inputActions[1].started += InputPlayerLightButton;
        inputActions[2].started += InputPlayerAttackButton;
        inputActions[3].started += InputPlayerSelectItemButton;
        inputActions[4].started += InputPlayerSelectItemControl;
        inputActions[5].started += InputPlayerUseItemButton;
        inputActions[6].started += InputPlayerUseItemControl;
        inputActions[0].performed += InputPlayerControl;
        inputActions[1].performed += InputPlayerLightButton;
        inputActions[2].performed += InputPlayerAttackButton;
        inputActions[3].performed += InputPlayerSelectItemButton;
        inputActions[4].performed += InputPlayerSelectItemControl;
        inputActions[5].performed += InputPlayerUseItemButton;
        inputActions[6].performed += InputPlayerUseItemControl;
        inputActions[0].canceled += InputPlayerControl;
        inputActions[1].canceled += InputPlayerLightButton;
        inputActions[2].canceled += InputPlayerAttackButton;
        inputActions[3].canceled += InputPlayerSelectItemButton;
        inputActions[4].canceled += InputPlayerSelectItemControl;
        inputActions[5].canceled += InputPlayerUseItemButton;
        inputActions[6].canceled += InputPlayerUseItemControl;

    }

    private void OnDisable()
    {
        inputActions[0].started -= InputPlayerControl;
        inputActions[1].started -= InputPlayerLightButton;
        inputActions[2].started -= InputPlayerAttackButton;
        inputActions[3].started -= InputPlayerSelectItemButton;
        inputActions[4].started -= InputPlayerSelectItemControl;
        inputActions[5].started -= InputPlayerUseItemButton;
        inputActions[6].started -= InputPlayerUseItemControl;
        inputActions[0].performed -= InputPlayerControl;
        inputActions[1].performed -= InputPlayerLightButton;
        inputActions[2].performed -= InputPlayerAttackButton;
        inputActions[3].performed -= InputPlayerSelectItemButton;
        inputActions[4].performed -= InputPlayerSelectItemControl;
        inputActions[5].performed -= InputPlayerUseItemButton;
        inputActions[6].performed -= InputPlayerUseItemControl;
        inputActions[0].canceled -= InputPlayerControl;
        inputActions[1].canceled -= InputPlayerLightButton;
        inputActions[2].canceled -= InputPlayerAttackButton;
        inputActions[3].canceled -= InputPlayerSelectItemButton;
        inputActions[4].canceled -= InputPlayerSelectItemControl;
        inputActions[5].canceled -= InputPlayerUseItemButton;
        inputActions[6].canceled -= InputPlayerUseItemControl;

        for (int i = 0; i < inputActions.Length; i++) inputActions[i].Disable();
    }

    void Start()
    {
        // 初期化：重力・HP・アイテム・縄の描画準備
        rb.useGravity = false;
        playerHP = maxPlayerHp;
        itemCountText.SetActive(false);
        arrowCount = maxArrowCount;

        line.positionCount = segmentCount;
        ropeAnimeObject.transform.SetParent(null);
        line.gameObject.transform.SetParent(null);
        handObject.transform.SetParent(null);

        points = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++) points[i] = playerObject.transform.position;
    }

    void Update()
    {
        // マネージャーの取得と状態（Status）による分岐
        if (audioManager == null) audioManager = AudioManager.Instance;

        switch (status) 
        {
            case 0: // start
            break;
            case 1: // play
                if (!arrowAnimeFlag)
                {
                    PlayerControl();         // 移動・砂地判定
                    HPControl();             // HPゲージ更新
                    PlayerLightControl();    // ライトの拡大縮小
                    MaxEffectControl();      // ライト最大時エフェクト
                    PlayerItemUseControl();  // アイテム（弓・縄）使用
                    CameraControl();         // カメラ追従              
                }
                else  ArrowAnime();

                // ライト使用中は移動速度が落ちるためアニメ速度を調整
                if (onLight != 1) animator.speed = 1f;
                else if (onLight == 1) animator.speed = 0.5f;
                break;
            case 2: // stop
                audioManager.PausePlayerSE();
                animator.speed = 0f;
                break;
            case 3: // menu
                audioManager.PausePlayerSE();
                animator.speed = 0f;
                break;
            case 4: // over
                audioManager.PausePlayerSE();
                OverControl();
                break;
            case 5: // clear               
                if (clearEffect == null)
                {
                    animator.SetTrigger("Clear");
                    clearEffect = Instantiate(clearEffectOrigin, transform.position, Quaternion.identity);
                    audioManager.PausePlayerSE();
                }
                break;
        }
        PlayerItemSelectControl();      // アイテム選択UIはStatusに関わらず更新       
    }

    /// <summary>
    /// プレイヤーの移動、砂地への沈下、地形に応じた重力制御
    /// </summary>
    void PlayerControl()
    {
        // 落下防止：一定高度以下に落ちた場合に復帰させる
        if (playerObject.transform.position.y < -1f) playerObject.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 3f, playerObject.transform.position.z);
        
        // 地面の法線検知：斜面に応じて重力を調整
        Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 0.5f, playerObject.transform.position.z), playerObject.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 1);
            if (playerObject.transform.position.y < 1.9f)
            {
                // 急斜面でなければ重力を弱めて滑り落ちを防止
                if (hit.normal.y > 0.7f) gravity.y = -5f;
                else gravity.y = -100f;
            }
            else gravity.y = -100f;
        }
        else gravity.y = -100f;

        // 現在のエリア番号（リスポーン地点の決定用）を判定
        for (int i = 0; i < playerObject.transform.parent.childCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (playerObject.transform.parent.GetChild(i).name == $"EnterArea ({j})")
                {
                    playerPosiNum = j;
                }
            }
        }

        // 砂地（Layer 7）への接触判定
        Ray underRay = new Ray(playerObject.transform.position, Vector3.down);
        if (Physics.Raycast(underRay, out hit, 0.001f) && !ropeMoveFlag && hit.collider.gameObject.layer == 7) onSandFlag = true;
        
        // 砂地での沈下演出とダメージ処理
        if (onSandFlag)
        {
            playerObject.GetComponent<Collider>().enabled = false; // 沈むために当たり判定を消す
            if (sandTimer == 0)
            {
                originPosition = playerObject.transform.position;
                damageFlag = true; // 沈下時に定数ダメージ
                damageAmount = 5;
            }
            if (sandTimer > sandTime)
            {
                // 沈下完了：リスポーン地点へ移動
                playerObject.GetComponent<Collider>().enabled = true;
                sandTimer = 0;
                onSandFlag = false;
                playerObject.transform.position = new Vector3(respawnPositions[playerPosiNum].transform.position.x, respawnPositions[playerPosiNum].transform.position.y + 3f, respawnPositions[playerPosiNum].transform.position.z);
            }
            else
            {
                // 沈下中：座標を徐々に下げる
                sandTimer += Time.deltaTime;
                float y = Mathf.Lerp(originPosition.y, originPosition.y -2.5f, sandTimer / sandTime);
                playerObject.transform.position = new Vector3(playerObject.transform.position.x, y, playerObject.transform.position.z);                
            }
        }

        // 通常移動処理（アイテム使用中や砂地でない場合）
        if (!itemSelectFlag && !itemUseFlag && !endUseFlag && !onSandFlag)
        {
            // 矢が存在する場合は距離を確認（移動制限など）
            if (arrowObject)
            {
                float arrowDistance = Vector2.Distance(new Vector2(arrowObject.transform.position.x, arrowObject.transform.position.z), new Vector2(playerObject.transform.position.x, playerObject.transform.position.z));
                if (arrowDistance > 3f || arrowObject.GetComponent<ArrowManager>().stopFlag)
                {
                    Vector3 playerPosition = playerObject.transform.position;
                    if (onLight == 1) playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime) * 0.1f;
                    else playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime);
                    playerObject.transform.position = playerPosition;
                }
            }
            else
            {
                Vector3 playerPosition = playerObject.transform.position;
                if (onLight == 1) playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime) * 0.1f;                
                else playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime);
                playerObject.transform.position = playerPosition;
                // Animation
                if (playerHorizontal != 0f || playerVertical != 0f)
                {
                    animator.SetBool("Idle", false);
                    animator.SetBool("Move", true);
                    if (!audioManager.playerSEs[0].isPlaying) audioManager.PlaySE(AudioManager.SEName.playerSes, 0);
                    float magunitude = Vector3.Magnitude(new Vector3(playerHorizontal, 0, playerVertical));
                    if (onLight == 1 || magunitude < 0.5f) audioManager.playerSEs[0].pitch = 1f;
                    else audioManager.playerSEs[0].pitch = 2f;
                }
                else if (playerHorizontal == 0f && playerVertical == 0f)
                {
                    animator.SetBool("Move", false);
                    animator.SetBool("Idle", true);
                    if (audioManager.playerSEs[0].isPlaying) audioManager.StopSE(AudioManager.SEName.playerSes, 0);
                }
            }
            //進む方向に滑らかに向く。
            transform.forward = Vector3.Slerp(transform.forward, new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime), Time.deltaTime * 10f);
        }
        rb.AddForce(gravity * Time.deltaTime, ForceMode.Impulse);
    }
    
    
    void CameraControl()
    {
        if(canItemFlag[itemSelectNum] && itemSelectNum == 1)
        {
            if (rangeRopeTargetObject != null)
            {
                float x = (playerObject.transform.position.x + rangeRopeTargetObject.transform.position.x) / 2;
                float z = (playerObject.transform.position.z + rangeRopeTargetObject.transform.position.z) / 2;
                mainCamera.transform.position = new Vector3(x, playerObject.transform.position.y + 15f, z);
            }
            else mainCamera.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 10f, playerObject.transform.position.z - 2f);
        }
        else mainCamera.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 10f, playerObject.transform.position.z - 2f);
    }

    /// <summary>
    /// HPの変化（ダメージ・回復）をゲージUIに滑らかに反映する
    /// </summary>
    void HPControl()
    {
        // ダメージと回復が同時に発生した場合の優先解決
        if (damageFlag && recoveryFlag)
        {
            if (farstDamageFlag && !farstRecoveryFlag) // ダメージ優先
            {
                float v = Mathf.Lerp(beforeHP, afterHP, 1);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerDamageGauge.fillAmount = v;
                damageTimer = 0;
                float volue = Mathf.InverseLerp(0f, maxPlayerHp, afterHP);
                playerHpGauge.fillAmount = volue;
                
                farstDamageFlag = false;
                damageFlag = false;
            }
            // 回復のほうが早い
            else if (!farstDamageFlag && farstRecoveryFlag) // 回復優先
            {
                playerRecoveryGauge.enabled = false;
                float v = Mathf.Lerp(beforeHP, afterHP, recoveryTimer / recoveryTime);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerHpGauge.fillAmount = v;           
                recoveryTimer = 0;

                farstRecoveryFlag = false;
                recoveryFlag = false;
            }
            playerHP = afterHP;
            playerHpText.text = $"HP : {afterHP} / {maxPlayerHp}";
            beforeHP = 0f;
            afterHP = 0f;
        }

        // ダメージ演出
        if (damageFlag)
        {
            damageTimer += Time.deltaTime;
            if (!farstDamageFlag)
            {
                audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 5);
                playerDamageGauge.enabled = true;
                beforeHP = playerHP;
                afterHP = playerHP - damageAmount;
                farstDamageFlag = true;
                float v = Mathf.InverseLerp(0f, maxPlayerHp, afterHP);
                playerHpGauge.fillAmount = v;
                playerHpText.text = $"HP : {afterHP} / {maxPlayerHp}";
                animator.SetTrigger("Damage");
            }
            if (damageTimer > damageTime) // 演出終了
            {
                playerHP = afterHP;
                beforeHP = 0f;
                afterHP = 0f;
                damageTimer = 0;
                damageFlag = false;
                farstDamageFlag = false;
            }
            else // 赤いゲージが追いかける演出
            {
                float v = Mathf.Lerp(beforeHP, afterHP, damageTimer / damageTime);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerDamageGauge.fillAmount = v;
            }
        }
        // 回復演出
        else if (recoveryFlag)
        {
            recoveryTimer += Time.deltaTime;
            if (!farstRecoveryFlag)
            {                
                playerRecoveryGauge.enabled = true;
                farstRecoveryFlag = true;
                beforeHP = playerHP;
                afterHP = playerHP + recoveryAmount;
                if (beforeHP == maxPlayerHp) // 元のHPが最大の場合
                {
                    farstRecoveryFlag = false;
                    recoveryFlag = false;
                }
                else audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 4);
                if (afterHP > 100f) afterHP = 100f;
                float v = Mathf.InverseLerp(0f, maxPlayerHp, afterHP);
                playerRecoveryGauge.fillAmount = v;
                playerHpText.text = $"HP : {afterHP} / {maxPlayerHp}";
            }
            if (recoveryTimer > recoveryTime)
            {
                playerHP = afterHP;
                beforeHP = 0f;
                afterHP = 0f;
                recoveryTimer = 0;
                recoveryFlag = false;
                farstRecoveryFlag = false;
            }
            else if (recoveryTimer < recoveryTime)
            {
                float v = Mathf.Lerp(beforeHP, afterHP, recoveryTimer / recoveryTime);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerHpGauge.fillAmount = v;
            }
        }
        // 演出外
        else if (!damageFlag && !recoveryFlag)
        {
            playerDamageGauge.enabled = false;
            playerRecoveryGauge.enabled = false;
            float v = Mathf.InverseLerp(0f, maxPlayerHp, playerHP);
            playerHpGauge.fillAmount = v;
            playerRecoveryGauge.fillAmount = v;
            playerHpText.text = $"HP : {playerHP} / {maxPlayerHp}";
        }
    }

    /// <summary>
    /// プレイヤーの持つライトの照射角と、それに伴うスタン判定範囲の制御
    /// </summary>
    void PlayerLightControl()
    {
        float propotrion = Mathf.InverseLerp(30f, 180f, playerLightRange);
        
        // ステート：最大拡大後のインターバル終了
        if (onLight == 2 && lightMaxIntervalTimer > lightMaxIntervalTime)
        {
            onLight = 0;
            lightRangeMinRange = playerLightRange;
            lightMaxIntervalTimer = 0;
            lightMaxIntervalTimerGauge.fillAmount = 0f;
            lightRangeObject.tag = "LightRange"; // 判定を通常に戻す
        }
        // ステート：最大拡大インターバル中
        else if (onLight == 2 && lightMaxIntervalTimer < lightMaxIntervalTime)
        {
            if (audioManager.playerSEs[2].isPlaying) audioManager.StopSE(AudioManager.SEName.playerSes, 2); 
            lightMaxIntervalTimer += Time.deltaTime;
            float normal = Mathf.InverseLerp(lightMaxIntervalTime, 0f, lightMaxIntervalTimer);
            lightMaxIntervalTimerGauge.fillAmount = normal;
        }

        // 最大拡大に達した瞬間の処理
        if (onLight == 1 && playerLightRange == 180f)
        {
            playerLightRange = 180f;
            lightSpreadTimer = 0;
            onLight = 2;
            lightRangeObject.tag = "StanRange"; // 敵をスタンさせる判定に変更
            Vector3 posi = new Vector3(transform.position.x, 0f, transform.position.z);
            GameObject effect = Instantiate(maxEffectOrigin, posi, Quaternion.identity);
            maxEffect = effect;
            maxEffect.transform.parent = transform;
            onMaxEffectFlag = true;
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 3);
        }

        // 角度の計算（拡大・縮小・UI更新）
        if (onLight == 0 && playerLightRange > 30f) // 縮小中
        {
            lightShrinkTimer += Time.deltaTime;
            float ratio = Mathf.InverseLerp(30f, 180f, lightRangeMinRange);
            lightShrinkTime = Mathf.Lerp(0f, lightSpreadTime, ratio);
            playerLightRange = Mathf.Lerp(lightRangeMinRange, 30f, lightShrinkTimer / lightShrinkTime);
            lightMaxIntervalTimerGauge.enabled = false;
        }
        else if (onLight == 1 && playerLightRange < 180f) // 拡大中
        {
            // 照射角が広がるほどチャージ速度が遅くなる演出
            float volume = Mathf.Lerp(0.05f, 0.25f, lightSpreadTimer / lightSpreadTime);
            audioManager.playerSEs[2].volume = volume;
            // ライト拡大時の演出部分
            if (propotrion < 0.2f) lightSpreadTimer += Time.deltaTime * 10f;
            else if (propotrion < 0.4f) lightSpreadTimer += Time.deltaTime;
            else lightSpreadTimer += Time.deltaTime * 0.5f;
            playerLightRange = Mathf.Lerp(30f, 180f, lightSpreadTimer / lightSpreadTime);
            lightMaxIntervalTimerGauge.enabled = true;
            float nor = Mathf.InverseLerp(30f, 180f, playerLightRange);
            lightMaxIntervalTimerGauge.fillAmount = nor;
        }
        // 最縮小
        else if (onLight == 0 && playerLightRange == 30f)
        {
            playerLightRange = 30f;
            lightShrinkTimer = 0;
            lightSpreadTimer = 0;
            playerLight.spotAngle = 0;
        }

        // 反映：スポットライトの角度と判定オブジェクトのスケール
        playerLight.spotAngle = playerLightRange;
        playerLight.innerSpotAngle = playerLightRange;        
        float normalized = Mathf.InverseLerp(30f, 180f, playerLightRange);        
        lightGauge.fillAmount = normalized;
        //lightRangeObject.transform.localScale = new Vector3(Mathf.Lerp(2.75f, 17.25f, normalized), 0.1f, Mathf.Lerp(2.75f, 17.25f, normalized));
        lightRangeObject.transform.localScale = new Vector3(Mathf.Lerp(2f, 18f, normalized), 0.1f, Mathf.Lerp(2f, 18f, normalized));
    }
    
    /// <summary>
    /// ゲームオーバー時の演出制御
    /// </summary>
    void OverControl()
    {
        // 死亡アニメーションを一度だけ実行
        if (!onOverEffectFlag)
        {
            animator.SetTrigger("Dead");
            onOverEffectFlag = true;
        }

        // 演出用エフェクトの生存期間をタイマーで管理
        if (overEffectTimer > overEffectTime)
        {
            Destroy(overEffect);
        }
        else
        {
            overEffectTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// ライト最大チャージ時の衝撃波エフェクトの拡大制御
    /// </summary>
    void MaxEffectControl()
    {
        if (onMaxEffectFlag)
        {
            if (maxEffectTimer > maxEffectTime)
            {
                maxEffectTimer = 0;
                Destroy(maxEffect);
                onMaxEffectFlag = false;
            }
            else
            {
                maxEffectTimer += Time.deltaTime;
                // 時間経過に合わせてエフェクトを等比的に拡大（1.0 -> 17.25）
                float scale = Mathf.Lerp(1, 17.25f, maxEffectTimer / maxEffectTime);
                maxEffect.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    /// <summary>
    /// アイテム選択メニュー（UI）の出現・消失アニメーション
    /// </summary>
    void PlayerItemSelectControl()
    {
        // アイテム選択画面を開いている時
        if (itemSelectFlag)
        {
            if (!startSelectFlag)
            {
                // 各スロットを画面外から指定位置へスライドインさせる
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * -175f - 350f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        startSelectFlag = true;
                    }
                    else
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(-125f, i * -175f -350f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }
            // 選択中のアイコンを一回り大きくして強調
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (i == itemSelectNum) itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(165f, 165f);
                else itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
            }
        }
        else // 選択画面を閉じる時のスライドアウト
        {
            if (!endSelectFlag)
            {
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-125f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        endSelectFlag = true;
                        itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
                    }
                    else
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(i * -175f - 350f, -125f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// アイテム（弓・縄）の具体的な使用ロジックとインターバル管理
    /// </summary>
    void PlayerItemUseControl()
    {
        // 連続使用防止のクールダウン処理
        if (itemIntervalFlag)
        {
            if (itemIntervalTimer > itemIntervalTime)
            {
                itemIntervalTimer = 0f;
                itemIntervalFlag = false;
            }
            else
            {
                itemIntervalTimer += Time.deltaTime;
                float v = Mathf.Lerp(1f, 0f, itemIntervalTimer / itemIntervalTime);
                itemIntervalObject.GetComponent<Image>().fillAmount = v;
            }
        }

        // 非使用時は常にプレイヤーの正面を使用方向にセット
        if (!itemUseFlag) itemUseDirection = playerObject.transform.forward;

        // ステージ進行度に応じたアイテム解禁判定
        if (getItemFlag)
        {
            if (clearStageNum == 1) canItemFlag[0] = true; // 弓解禁
            else if (clearStageNum == 5) canItemFlag[1] = true; // 縄解禁
        }

        // 弓の使用可
        if (clearStageNum > 5)
        {
            for (int i = 0; i < 2; i++) canItemFlag[i] = true;
        }
        else if (clearStageNum > 1)
        {
            canItemFlag[0] = true;
        }
        // 投げ縄の使用可
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[i];
            }
            else if (!canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[3];
            }
            if (itemSelectNum == i) selectObject.GetComponent<RectTransform>().anchoredPosition = itemSlots[i].GetComponent<RectTransform>().anchoredPosition;
        }
        // 使用
        // --- 弓の使用処理 (Index 0) ---
        if (canItemFlag[itemSelectNum] && itemSelectNum == 0)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[0];
            itemCountText.SetActive(true);
            TextMeshProUGUI countText = itemCountText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            countText.text = arrowCount.ToString("00");
            if (arrowCount == maxArrowCount) countText.color = new Color(0f, 0.75f, 0f);
            else if (arrowCount == 0) countText.color = Color.red;
            else countText.color = Color.black;

            if (itemUseFlag && !arrowAnimeFlag && arrowCount > 0)
            {
                // 構え動作：移動を止めて正面を向く
                animator.SetBool("Move", false);
                animator.SetBool("Idle", true);

                // 射線上の障害物検知（SphereCastで厚みのある判定）
                Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                RaycastHit hit;
                if (Physics.SphereCast(ray.origin, 0.05f, ray.direction, out hit, 30f))
                {

                    if (!hit.collider.isTrigger)
                    {
                        if (hit.collider.tag == "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                            betweenObjectFlag = false;
                        }
                        else if (hit.collider.tag != "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                            betweenObjectFlag = true;
                            arrowAnimeFlag = false;
                        }
                    }
                    else
                    {
                        if (hit.collider.tag != "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                            betweenObjectFlag = false;
                            if (betweenObject == null)
                            {
                                betweenObject = hit.collider.gameObject;
                                betweenObject.SetActive(false);
                                arrowAnimeFlag = false;
                            }
                            else
                            {
                                betweenObject.SetActive(true);
                                betweenObject = hit.collider.gameObject;
                                betweenObject.SetActive(false);
                                arrowAnimeFlag = false;
                            }
                        }
                    }
                }
                Quaternion rotation = Quaternion.LookRotation(itemUseDirection);
                if (!audioManager.playerSEs[8].isPlaying) audioManager.PlaySE(AudioManager.SEName.playerSes, 8);
                bow.SetActive(true);
                //進む方向に滑らかに向く。
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            else if (itemUseFlag && !arrowAnimeFlag && arrowCount == 0) itemUseFlag = false;
            // 発射（ボタン離した瞬間）
            if (endUseFlag && arrowCount > 0)
            {
                bow.SetActive(false);
                arrowCount--;
                Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                RaycastHit hit;
                if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 30f))
                {
                    if (!hit.collider.isTrigger && hit.collider.tag == "Button")
                    {
                        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                        betweenObjectFlag = false;
                        arrowAnimeFlag = true;
                    }
                }
                if (betweenObject)
                {
                    betweenObject.SetActive(true);
                    betweenObject = null;
                    betweenObjectFlag = false;
                }
                arrowObject = Instantiate(arrowPrefab, arrowSpawer.transform.position, Quaternion.identity);
                audioManager.StopSE(AudioManager.SEName.playerSes, 8);
                audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 9);
                if (arrowAnimeFlag)
                {
                    arrowObject.GetComponent<ArrowManager>().speed = 10f;
                    arrowObject.GetComponent<ArrowManager>().lostTime = 10f;
                }
                itemUseFlag = false;
                endUseFlag = false;
                itemIntervalFlag = true;
            }
            else if (endUseFlag && arrowCount == 0) endUseFlag = false;
        }
        // --- 投げ縄の使用処理 (Index 1) ---
        else if (canItemFlag[itemSelectNum] && itemSelectNum == 1)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[1];
            itemCountText.SetActive(false);
            if (itemUseFlag)
            {
                animator.SetBool("Move", false);
                animator.SetBool("Idle", true);
                // 縄を振り回す演出
                IdleRopeAnimation();
                RotationRoteTopObject();
                for (int i = 0; i < ropeTargetObjects.Length; i++)
                {
                    Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                    RaycastHit hit;
                    // playerとwoodの間にobjectがないか検知
                    Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);
                    if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 10f))
                    {
                        if (!hit.collider.gameObject.GetComponent<Collider>().isTrigger)
                        {
                            if (hit.collider.gameObject.tag != "Wood")
                            {                                
                                betweenObjectFlag = true;
                            }
                            else if (hit.collider.gameObject.tag == "Wood")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green);
                                betweenObjectFlag = false;
                            }
                        }
                        else
                        {
                            if (hit.collider.gameObject.tag != "Wood" && hit.collider.gameObject.tag != "Rope")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);
                                betweenObjectFlag = true;
                                if (betweenObject == null)
                                {
                                    betweenObject = hit.collider.gameObject;
                                    betweenObject.SetActive(false);
                                }
                                else
                                {
                                    betweenObject.SetActive(true);
                                    betweenObject = hit.collider.gameObject;
                                    betweenObject.SetActive(false);
                                }
                            }
                            else if (hit.collider.gameObject.tag == "Wood" || hit.collider.gameObject.tag == "Rope")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green);
                                betweenObjectFlag = false;
                            }
                        }
                    }
                    // woodを検知
                    if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 10f, woodLayer) && !betweenObjectFlag)
                    {
                        if (hit.collider.gameObject == ropeTargetObjects[i])
                        {
                            rangeRopeTargetObject = ropeTargetObjects[i];
                            rangeRopeTargetPosition = new Vector3(rangeRopeTargetObject.transform.position.x, rangeRopeTargetObject.transform.position.y + 1f, rangeRopeTargetObject.transform.position.z);

                            if (ropeObject == null)
                            {
                                ropeObject = Instantiate(ropePrefab, playerObject.transform.parent.position, Quaternion.identity);
                                ropeObject.transform.position = rangeRopeTargetPosition;
                                ropeObject.SetActive(false);
                            }
                            ropeMoveFlag = true;
                        }
                    }
                    else
                    {
                        if (ropeObject != null) Destroy(ropeObject);
                        rangeRopeTargetObject = null;
                        ropeObject = null;
                        ropeMoveFlag = false;
                    }
                }              
                if (rangeRopeTargetObject != null)
                {
                    rangeRopeTargetPosition = new Vector3(rangeRopeTargetObject.transform.position.x, rangeRopeTargetObject.transform.position.y + 1f, rangeRopeTargetObject.transform.position.z);
                    ropeObject.transform.position = rangeRopeTargetPosition;
                }
                Quaternion rotation = Quaternion.LookRotation(itemUseDirection);
                if (!audioManager.playerSEs[10].isPlaying) audioManager.PlaySE(AudioManager.SEName.playerSes, 10);
                //進む方向に滑らかに向く。
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            else if(!itemUseFlag && betweenObject)
            {
                betweenObject.SetActive(true);
                betweenObject = null;
            }
            // 縄を投げて移動開始
            if (endUseFlag)
            {                
                itemUseFlag = false;
                if (betweenObject != null)
                {
                    betweenObject.SetActive(true);
                    betweenObject = null;
                }
                if (ropeMoveFlag)
                {
                    TargetAnimation();
                    if (ropeMoveTimer == 0)
                    {
                        audioManager.StopSE(AudioManager.SEName.playerSes, 10);
                        audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 11);
                        originPlayerObjectPosition = playerObject.transform.position;
                        ropeObject.SetActive(true);
                        float x = rangeRopeTargetPosition.x - playerObject.transform.position.x;
                        float z = rangeRopeTargetPosition.z - playerObject.transform.position.z;
                        if (Mathf.Abs(x) > Mathf.Abs(z)) // 左右
                        {
                            if (x > 0) // 右
                            {
                                rangeRopeTargetPosition.x -= 1.5f;
                            }
                            else if (x < 0) // 左
                            {
                                rangeRopeTargetPosition.x += 1.5f;
                            }
                        }
                        else if (Mathf.Abs(x) < Mathf.Abs(z)) // 前後
                        {
                            if (z > 0) // 前
                            {
                                rangeRopeTargetPosition.z -= 1.5f;
                            }
                            else if (z < 0) // 後
                            {
                                rangeRopeTargetPosition.z += 1.5f;
                            }
                        }
                    }
                    if (ropeMoveTimer > ropeMoveTime)
                    {
                        ropeMoveTimer = 0;
                        ropeMoveFlag = false;
                        itemIntervalFlag = true;
                    }
                    else
                    {
                        ropeObject.transform.position = new Vector3(rangeRopeTargetObject.transform.position.x, rangeRopeTargetObject.transform.position.y + 1, rangeRopeTargetObject.transform.position.z);
                        float x = Mathf.Lerp(originPlayerObjectPosition.x, rangeRopeTargetPosition.x, ropeMoveTimer / ropeMoveTime);
                        float z = Mathf.Lerp(originPlayerObjectPosition.z, rangeRopeTargetPosition.z, ropeMoveTimer / ropeMoveTime);
                        playerObject.transform.position = new Vector3(x, originPlayerObjectPosition.y, z);
                        ropeMoveTimer += Time.deltaTime;
                    }
                }
                else
                {
                    ropeAnimeObject.SetActive(false);
                    handObject.SetActive(false);
                    line.gameObject.SetActive(false);
                    Destroy(ropeObject);
                    rangeRopeTargetObject = null;
                    ropeObject = null;
                    itemUseFlag = false;
                    endUseFlag = false;
                    betweenObjectFlag = false;
                    audioManager.StopSE(AudioManager.SEName.playerSes, 10);
                }
            }
        }
        // --- 未設定 (Index 2) ---
        else if (canItemFlag[itemSelectNum] && itemSelectNum == 2)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[2];
            itemCountText.SetActive(false);
            if (itemUseFlag)
            {

            }
            if (endUseFlag)
            {
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        else
        {
            itemCountText.SetActive(false);
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[3];
            itemUseFlag = false;
            endUseFlag = false;
        }
    }

    /// <summary>
    /// 投げ縄待機中：縄と手の円運動を計算
    /// </summary>
    void IdleRopeAnimation()
    {
        Vector3 playerPos = playerObject.transform.position;
        Vector3 playerForward = playerObject.transform.forward;
        handObject.transform.position = playerPos + playerForward * 0.5f;
        line.positionCount = 10; // LineRendererを10点で構成
        line.gameObject.transform.position = Vector3.zero;
        handAngle += handSpeed * Time.deltaTime;

        // 手の回転位置
        float handRad = handAngle * Mathf.Deg2Rad;
        Vector3 handPos = handObject.transform.position;
        Vector3 handOffset = new Vector3(Mathf.Cos(handRad) * handRadius + playerPos.x, playerPos.y + 1f, Mathf.Sin(handRad) * handRadius + playerPos.z);
        handObject.transform.position = handOffset;

        // 縄の先端の回転位置（手より少し高い位置）
        ropeAngle += ropeSpeed * Time.deltaTime;
        float ropeRad = ropeAngle * Mathf.Deg2Rad;
        Vector3 ropeOffset = new Vector3(Mathf.Cos(ropeRad) * ropeRadius + playerPos.x, playerObject.transform.position.y + 2f, Mathf.Sin(ropeRad) * ropeRadius + playerPos.z);
        ropeAnimeObject.transform.position = ropeOffset;

        line.useWorldSpace = true;
        line.SetPosition(0, handObject.transform.position);
        line.SetPosition(line.positionCount - 1, ropeAnimeObject.transform.position);

        // 手から先端までをベジェ曲線のように少したわませて描画
        for (int i = 1; i < line.positionCount - 1; i++)
        {
            float t = i / 9f;
            Vector3 pos = Vector3.Lerp(handObject.transform.position, ropeAnimeObject.transform.position, t);
            pos.x += Mathf.Cos(t * Mathf.PI) * 0.2f;
            pos.z += Mathf.Sin(t * Mathf.PI) * 0.2f;
            line.SetPosition(i, pos);
        }
    }

    /// <summary>
    /// ロープの自転
    /// </summary>
    void RotationRoteTopObject()
    {
        ropeAnimeObject.SetActive(true);
        handObject.SetActive(true);
        line.gameObject.SetActive(true);
        ropeAnimeObject.transform.eulerAngles += new Vector3(0f, Time.deltaTime * 200f, 0f);
    }

    /// <summary>
    /// 縄がターゲットにヒットした後の引き寄せ（プレイヤー移動）処理
    /// </summary>
    void TargetAnimation()
    {
        ropeAnimeObject.SetActive(false);
        line.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z);
        Vector3 playerPos = playerObject.transform.position;
        playerPos.y += 1f;
        Vector3 targetPos = rangeRopeTargetObject.transform.position;        
        targetPos.y += 1f;
        line.positionCount = 2; // 移動中は直線にする
        ropeObject.SetActive(true);
        line.useWorldSpace = true;
        line.SetPosition(0, playerPos);
        line.SetPosition(1, targetPos); // ターゲットから自身への相対ベクトル
    }

    /// <summary>
    /// アイテム取得時のステータス更新
    /// </summary>
    /// <param name="itemNum">0:回復(Heart), 1:矢(Arrow)</param>
    public void GetItemControl(int itemNum)
    {
        // ハート取得時：HP回復フラグを立てる（HPControlで処理される）
        if (itemNum == 0)
        {
            recoveryFlag = true;
        }
        // 矢の束取得時：残弾数を5加算（最大値を超えないようクランプ）
        else if (itemNum == 1)
        {
            arrowCount += 5;
            if (arrowCount > maxArrowCount) arrowCount = maxArrowCount;
        }
    }

    /// <summary>
    /// 矢が飛んでいる間のカメラ演出と状態遷移
    /// </summary>
    void ArrowAnime()
    {
        // 矢が存在しない（消失した）場合：通常状態（status:1）に戻す
        if (!arrowObject)
        {
            status = 1;
            arrowAnimeFlag = false;
        }
        // 矢が飛んでいる最中
        else if (arrowObject)
        {
            // 特殊演出状態（status:2）へ移行：プレイヤー操作を制限する
            status = 2;
            // カメラを矢の後方に追従させる
            // 矢の座標から上に10f、手前に2fオフセットした位置にカメラを固定
            Vector3 position = new Vector3(arrowObject.transform.position.x, arrowObject.transform.position.y + 10f, arrowObject.transform.position.z - 2f);
            mainCamera.transform.position = position;
            if (arrowObject.GetComponent<ArrowManager>().hitFlag) arrowObject.GetComponent<ArrowManager>().lostTime = 0.1f;
        }
    }

    /// <summary>
    /// 壁との接触中、壁方向への入力を遮断して食い込みを防止する
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            // プレイヤーと壁の相対距離を計算
            float x = playerObject.transform.position.x - collision.gameObject.transform.position.x;
            float y = playerObject.transform.position.y - collision.gameObject.transform.position.y;
            float z = playerObject.transform.position.z - collision.gameObject.transform.position.z;

            // 足元（y < 0.5f）での接触の場合
            if (y < 0.5f)
            {
                // X軸方向の距離が長い場合は左右入力を、Z軸なら前後入力を0にして移動を制限
                if (Mathf.Abs(x) > Mathf.Abs(z)) playerHorizontal = 0f;
                else if (Mathf.Abs(x) < Mathf.Abs(z)) playerVertical = 0f;
            }
        }
    }

    // ==========================================
    // InputAction管理関数 (Input Systemからのメッセージ)
    // ==========================================

    // ------------------------------------------
    // 新しい
    // ------------------------------------------

    /// <summary>
    /// スティック/十字キーによる移動入力
    /// </summary>
    public void OnPlayerControl(InputValue value)
    {
        // アイテム選択中や使用中でなければ入力を受け付ける
        if (!itemSelectFlag && (!itemUseFlag || !endUseFlag))
        {
            Vector2 input = value.Get<Vector2>();
            playerHorizontal = input.x;
            playerVertical = input.y;
        }
    }

    /// <summary>
    /// ライトボタンの入力（押しっぱなしで拡大、離して縮小開始）
    /// </summary>
    public void OnLightButton(InputValue value)
    {
        if (value.isPressed && playerLightRange == 30) // 押し始め
        {
            onLight = 1; // 拡大モード
            audioManager.PlaySE(AudioManager.SEName.playerSes, 2);
        }
        else if (!value.isPressed && onLight == 1) // 離した時
        {
            lightRangeMinRange = playerLightRange;
            onLight = 0; // 縮小モード
            audioManager.StopSE(AudioManager.SEName.playerSes, 2);
        }
    }

    /// <summary>
    /// 近接攻撃ボタン
    /// </summary>
    public void AttackButton(InputValue value)
    {
        if (value.isPressed && status == 1 && !attackFlag)
        {
            attackFlag = true;
            animator.SetTrigger("Attack");
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 1);
        }
    }

    /// <summary>
    /// アイテム選択メニューの表示切り替え
    /// </summary>
    public void OnItemSelectButton(InputValue value)
    {
        if (value.isPressed && status == 1 && !itemUseFlag && !endUseFlag)
        {
            itemSelectFlag = true;
            endSelectFlag = false;
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 7);
        }
        if (!value.isPressed)
        {
            itemSelectFlag = false;
            startSelectFlag = false;
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 7);
        }
    }

    /// <summary>
    /// アイテム選択メニュー表示中の項目切り替え
    /// </summary>
    public void OnItemSelectControl(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (itemSelectFlag)
        {
            // 移動を止める
            if (input.x == 0) playerHorizontal = 0;

            // スティックの左右で選択番号を増減
            if (value.isPressed && input.x > 0) itemSelectNum--;
            else if (value.isPressed && input.x < 0) itemSelectNum++;

            // インデックスのループ処理 (0-2)
            if (itemSelectNum > 2) itemSelectNum = 0;
            if (itemSelectNum < 0) itemSelectNum = 2;
            if (value.isPressed && input.x != 0) audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 6);
        }
    }

    /// <summary>
    /// アイテム使用ボタン（押しっぱなしで構え、離して発動）
    /// </summary>
    public void OnUseItemButton(InputValue value)
    {
        // 押し始め：使用フラグON
        if (value.isPressed && status == 1 && !itemSelectFlag && !itemIntervalFlag) itemUseFlag = true;
        // 離した時：終了フラグON（PlayerItemUseControl内で実際の使用処理が行われる）
        if (!value.isPressed && status == 1 && !itemSelectFlag && !itemIntervalFlag && itemUseFlag) endUseFlag = true;
    }

    /// <summary>
    /// アイテム使用中（構え中）の方向指定
    /// </summary>
    public void OnUseItemControl(InputValue value)
    {
        if (itemUseFlag && (value.Get<Vector2>().x != 0 || value.Get<Vector2>().y != 0)) itemUseDirection = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
    }

    // -------------------------------------------
    // 古い
    // -------------------------------------------

    /// <summary>
    /// スティック/十字キーによる移動入力
    /// </summary>
    public void InputPlayerControl(InputAction.CallbackContext context)
    {
        // アイテム選択中や使用中でなければ入力を受け付ける
        if (!itemSelectFlag && (!itemUseFlag || !endUseFlag))
        {
            Vector2 input = context.ReadValue<Vector2>();
            playerHorizontal = input.x;
            playerVertical = input.y;
        }
    }

    /// <summary>
    /// ライトボタンの入力（押しっぱなしで拡大、離して縮小開始）
    /// </summary>
    public void InputPlayerLightButton(InputAction.CallbackContext context)
    {
        if (context.started && playerLightRange == 30) // 押し始め
        {
            onLight = 1; // 拡大モード
            audioManager.PlaySE(AudioManager.SEName.playerSes, 2);
        }
        else if (context.canceled && onLight == 1) // 離した時
        {
            lightRangeMinRange = playerLightRange;
            onLight = 0; // 縮小モード
            audioManager.StopSE(AudioManager.SEName.playerSes, 2);
        }
    }

    /// <summary>
    /// 近接攻撃ボタン
    /// </summary>
    public void InputPlayerAttackButton(InputAction.CallbackContext context)
    {
        if (context.started && status == 1 && !attackFlag)
        {
            attackFlag = true;
            animator.SetTrigger("Attack");
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 1);
        }
    }

    /// <summary>
    /// アイテム選択メニューの表示切り替え
    /// </summary>
    public void InputPlayerSelectItemButton(InputAction.CallbackContext context)
    {
        if (context.started && status == 1 && !itemUseFlag && !endUseFlag)
        {
            itemSelectFlag = true;
            endSelectFlag = false;
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 7);
        }
        if (context.canceled) 
        {
            itemSelectFlag = false; 
            startSelectFlag = false;
            audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 7);
        }
    }

    /// <summary>
    /// アイテム選択メニュー表示中の項目切り替え
    /// </summary>
    public void InputPlayerSelectItemControl(InputAction.CallbackContext context)
    {
        if (itemSelectFlag)
        {
            // 移動を止める
            if (context.ReadValue<Vector2>().x == 0) playerHorizontal = 0;

            // スティックの左右で選択番号を増減
            if (context.started && context.ReadValue<Vector2>().x > 0) itemSelectNum--;
            else if (context.started && context.ReadValue<Vector2>().x < 0) itemSelectNum++;

            // インデックスのループ処理 (0-2)
            if (itemSelectNum > 2) itemSelectNum = 0;
            if (itemSelectNum < 0) itemSelectNum = 2;
            if (context.started && context.ReadValue<Vector2>().x != 0) audioManager.PlayOneShotSE(AudioManager.SEName.playerSes, 6);
        }
    }

    /// <summary>
    /// アイテム使用ボタン（押しっぱなしで構え、離して発動）
    /// </summary>
    public void InputPlayerUseItemButton(InputAction.CallbackContext context)
    {
        // 押し始め：使用フラグON
        if (context.started && status == 1 && !itemSelectFlag && !itemIntervalFlag) itemUseFlag = true;
        // 離した時：終了フラグON（PlayerItemUseControl内で実際の使用処理が行われる）
        if (context.canceled && status == 1 && !itemSelectFlag && !itemIntervalFlag && itemUseFlag) endUseFlag = true;
    }

    /// <summary>
    /// アイテム使用中（構え中）の方向指定
    /// </summary>
    public void InputPlayerUseItemControl(InputAction.CallbackContext context)
    {
        if(itemUseFlag && (context.ReadValue<Vector2>().x != 0 || context.ReadValue<Vector2>().y != 0)) itemUseDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
    }

    // ==========================================
    // 外部アクセス用関数
    // ==========================================

    /// <summary>
    /// アニメーションイベント等から攻撃終了を通知するための関数
    /// </summary>
    public void SetAttackFlag(bool flag)
    {
        attackFlag = flag;
    }

    /// <summary>
    /// 外部空ライト範囲を取得できる
    /// </summary>
    /// <returns></returns>
    public float GetPlayerLightRange()
    {
        return lightRangeObject.transform.localScale.x;
    }

}
