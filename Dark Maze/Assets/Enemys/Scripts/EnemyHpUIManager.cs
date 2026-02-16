using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵の頭上HPバーを管理するクラス
/// ダメージ時に時間差で減少するゲージ（ダメージゲージ）を制御
/// </summary>
public class EnemyHpUIManager : MonoBehaviour
{
    private Camera mainCamera;

    [Header("UIパーツ（階層構造から自動取得）")]
    Image backGround;    // 外枠・背景
    Image damageGauge;   // ダメージ時に残る中間ゲージ（白や赤）
    Image hpGauge;       // 現在のHPゲージ（緑など）

    float maxHp;
    float enemyHp;

    [Header("ダメージ演出設定")]
    public bool damageFlag;           // ダメージ演出中か
    float damageGaugeAmount;          // 演出開始時のダメージゲージの量
    [SerializeField] float damageTime; // どのくらいの時間をかけてダメージゲージを追いつかせるか
    float damageTimer;

    private void Awake()
    {
        mainCamera = Camera.main;

        // 子オブジェクトから各パーツを取得（階層に依存：背景 > ダメージ用 > 現在HP用）
        backGround = transform.GetChild(0).GetComponent<Image>();
        damageGauge = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        hpGauge = transform.GetChild(0).GetChild(1).GetComponent<Image>();

        backGround.gameObject.SetActive(false);// 初期状態は非表示
        damageGaugeAmount = damageGauge.fillAmount;
    }

    private void Update()
    {
        // ダメージフラグが立っている間、中間ゲージをゆっくり減らす
        if (damageFlag) DamageControl();
    }

    /// <summary>
    /// UIを常にカメラの方向へ向かせる（ビルボード処理）
    /// </summary>
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // カメラの方向を向く
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    /// <summary>
    /// HPバー自体の表示・非表示を切り替え
    /// </summary>
    public void HpActive(bool flag)
    {
        backGround.gameObject.SetActive(flag);
    }

    /// <summary>
    /// 最大HPのセット（Enemy1のStartなどで呼ばれる）
    /// </summary>
    public void GetMaxHp(float max)
    {
        maxHp = max;
    }

    /// <summary>
    /// 現在のHPに合わせてメインゲージを即座に更新
    /// </summary>
    public void HpControl(float hp)
    {
        enemyHp = hp;

        // 0~maxHpの値を 0~1 の範囲に変換してImage.fillAmountに適用
        float range = Mathf.InverseLerp(0, maxHp, hp);
        hpGauge.fillAmount = range;
    }

    /// <summary>
    /// ダメージゲージを補間（Lerp）を使って滑らかに減少させる
    /// </summary>
    void DamageControl()
    {
        // 演出終了
        if (damageTimer > damageTime)
        {
            damageFlag = false;
            damageGaugeAmount = damageGauge.fillAmount;
            damageTimer = 0;
        }
        else 
        {
            // 目標とするHP割合（現在のHP）
            float hp = Mathf.InverseLerp(0, maxHp, enemyHp);

            // Lerp(開始時の量, 目標の量, 経過時間割合) で滑らかに移動
            float range = Mathf.Lerp(damageGaugeAmount, hp, damageTimer / damageTime);
            damageGauge.fillAmount = range;
            damageTimer += Time.deltaTime;
        }
    }
}
