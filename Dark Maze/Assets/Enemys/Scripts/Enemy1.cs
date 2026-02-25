using UnityEngine;
using UnityEngine.AI;
using static GeneralStageManager;

/// <summary>
/// 基本的な敵AI：徘徊、追跡、攻撃、ダメージ、スタン（混乱）処理を管理
/// </summary>
public class Enemy1 : MonoBehaviour
{
    [Header("システム参照")]
    [SerializeField] GeneralStageManager gameManager;
    [SerializeField] EnemyHpUIManager hpUIManager;
    [SerializeField] NavMeshAgent agent;
    Rigidbody rb;
    [SerializeField] GameObject player;

    [Header("移動・巡回設定")]
    [SerializeField] GameObject[] wanderPoints;    // 徘徊する地点のリスト
    [SerializeField] int wanderPointNum;           // 現在目指している地点の番号
    [SerializeField] float wanderingSpeed;         // 徘徊時の移動速度
    [SerializeField] float trackingRange;          // 追跡を開始する距離
    [SerializeField] float trackingSpeed;          // 追跡時の移動速度


    [Header("状態フラグ")]
    public bool moveFlag;      // ライトに照らされて動ける状態か
    public bool stanFlag;      // スタン（混乱）中か
    bool dieFlag;              // 死亡済みか
    bool trackFlag;            // 追跡中か
    [SerializeField] bool isDamageFlag; // ダメージ硬直中か
    [SerializeField] bool isAttackFlag; // 攻撃アニメーション中か



    [Header("ステータス")]
    [SerializeField] float maxEnemyHP = 3;
    float enemyHP;
    [SerializeField] float enemyDamage;      // プレイヤーに与えるダメージ
    [SerializeField] float knockbackPower;   // ノックバックの強さ
    [SerializeField] float stanTime;         // スタン持続時間
    float stanTimer;
    [SerializeField] float isDamageTime;     // 被ダメージ後の硬直時間
    float isDamageTimer;

    [Header("演出関連")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject confusionObject; // スタン中のピヨピヨ演出など
    [SerializeField] GameObject dieEffect;
    [SerializeField] GameObject damageEffect;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyHP = maxEnemyHP;
        hpUIManager.GetMaxHp(maxEnemyHP);
    }

    void Update()
    {
        // 移動SEの管理
        if (moveFlag) AudioManager.Instance.PlaySE(AudioManager.SEName.enemySes, 0);
        else AudioManager.Instance.StopSE(AudioManager.SEName.enemySes, 0);

        // ゲーム全体の進行状況（GameStatus）に合わせて処理を分岐
        switch (gameManager.status) 
        {
            case GameStatus.play:
                if (!dieFlag) EnemyControl();
                EnemyHpGaugeControl();
                animator.speed = 1f;
                break;
            case GameStatus.stop:
            case GameStatus.menu:
            case GameStatus.over:
            case GameStatus.clear:
                if (agent.enabled) agent.isStopped = true;
                animator.speed = 0f; // アニメーション一時停止
                break;
        }        
    }

    /// <summary>
    /// 敵の行動ロジック（ダメージ・スタン・通常行動）
    /// </summary>
    void EnemyControl()
    {
        // 1. 被ダメージ硬直
        if (isDamageFlag)
        {
            EnemyDamage();
        }
        // 2. スタン状態（混乱中
        else if (stanFlag)
        {
            confusionObject.SetActive(true);
            agent.isStopped = true;
            animator.SetBool("Down", true);
            animator.SetBool("Move", false);
            animator.SetBool("Attack", false);
            if (!AudioManager.Instance.enemySEs[2].isPlaying) AudioManager.Instance.PlaySE(AudioManager.SEName.enemySes, 2);
            isAttackFlag = false;
            if (stanTimer > stanTime)
            {
                stanFlag = false;
                stanTimer = 0;
                if (AudioManager.Instance.enemySEs[2].isPlaying) AudioManager.Instance.StopSE(AudioManager.SEName.enemySes, 2);
            }
            else
            {
                stanTimer += Time.deltaTime;
            }
        }
        // 3. 通常の状態
        else if (!stanFlag)
        {
            confusionObject.SetActive(false);
            animator.SetBool("Down", false);

            if (moveFlag) // ライトに照らされている（動ける）
            {
                animator.SetBool("Move", true);
                AudioManager.Instance.PlaySE(AudioManager.SEName.enemySes, 0);
                // 追跡モード
                if (trackFlag)
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.destination = player.transform.position;
                    agent.speed = trackingSpeed;
                    float trackDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(transform.position.x, transform.position.z));
                    if (trackDistance < 1f)
                    {
                        animator.SetBool("Attack", true);
                        if (!AudioManager.Instance.enemySEs[1].isPlaying) AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.enemySes, 1);
                        isAttackFlag = true;
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        isAttackFlag = false;
                    }
                }
                // 徘徊モード
                else
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.destination = wanderPoints[wanderPointNum].transform.position;
                    agent.speed = wanderingSpeed;
                    float wanderPointsDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(wanderPoints[wanderPointNum].transform.position.x, 0, wanderPoints[wanderPointNum].transform.position.z));
                    if (wanderPointsDistance < 0.5f)
                    {
                        wanderPointNum++;
                        if (wanderPointNum == wanderPoints.Length) wanderPointNum = 0;
                        agent.destination = wanderPoints[wanderPointNum].transform.position;
                    }
                    float trackDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(transform.position.x, transform.position.z));
                    if (trackDistance < trackingRange) trackFlag = true;
                }
            }
            // ライトの外（停止
            else if (!moveFlag)
            {
                agent.isStopped = true;
                animator.SetBool("Move", false);
                animator.SetBool("Attack", false);
                if (AudioManager.Instance.enemySEs[0].isPlaying) AudioManager.Instance.StopSE(AudioManager.SEName.enemySes, 0);
            }
        }            
    }

    /// <summary>
    /// HPゲージの表示/非表示と残量更新
    /// </summary>
    void EnemyHpGaugeControl()
    {
        Vector3 playerPos = new Vector3(player.transform.position.x, 0 , player.transform.position.z);
        Vector3 enemyPos = new Vector3(transform.position.x, 0, transform.position.z);
        float distance = Vector3.Distance(playerPos, enemyPos);
        float lightDistance = player.GetComponent<PlayerController>().GetPlayerLightRange() / 2f;
        bool flag = false;
        if (lightDistance > distance) flag = true;
        else flag = false;

        // プレイヤーのライトの範囲内ならHPを表示
        hpUIManager.HpActive(flag);
        hpUIManager.HpControl(enemyHP);
    }

    /// <summary>
    /// ダメージを受けた瞬間の処理
    /// </summary>
    void EnemyDamageHit()
    {
        enemyHP--;
        confusionObject.SetActive(false);
        if (!hpUIManager.damageFlag) hpUIManager.damageFlag = true;
        if (enemyHP <= 0)
        {
            animator.SetTrigger("Die");
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.enemySes, 3);
            GameObject effect = Instantiate(dieEffect, transform.position, Quaternion.identity);
            effect.transform.parent = transform;
            effect.transform.eulerAngles = new Vector3(-90, 0, 0);
            enemyHP = -1;
            agent.enabled = false;
            dieFlag = true;
        }
        else if (enemyHP > 0)
        {
            // ダメージ演出とノックバック
            GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            effect.transform.parent = transform;
            effect.transform.localScale = new Vector3(2, 2, 2);
            Vector3 knockback = (transform.position - player.transform.position).normalized;
            rb.AddForce(knockback * knockbackPower * Time.deltaTime, ForceMode.Impulse);
        }
    }

    void EnemyDamage()
    {
        if (isDamageTimer > isDamageTime)
        {
            agent.enabled = true;
            isDamageFlag = false;
            isDamageTimer = 0f;
        }
        else isDamageTimer += Time.deltaTime;
    }

    public void EnemyDestroy()
    {
        Destroy(gameObject);
        AudioManager.Instance.StopEnemySE();
    }

    // --- 衝突検知 ---
    private void OnCollisionEnter(Collision collision)
    {
        if (!dieFlag)
        {
            // プレイヤーへのダメージ判定
            if (collision.gameObject.tag == "Player" && isAttackFlag)
            {
                if (collision.gameObject.GetComponent<PlayerController>().playerHP > 0)
                {
                    collision.gameObject.GetComponent<PlayerController>().damageFlag = true;
                    collision.gameObject.GetComponent<PlayerController>().damageAmount = enemyDamage;
                    Vector3 knockback = (collision.gameObject.transform.position - transform.position).normalized;
                    rb.AddForce(knockback * knockbackPower * Time.deltaTime, ForceMode.Impulse);
                }
            }
            // 矢が当たった
            else if (collision.gameObject.tag == "Arrow" && !isDamageFlag)
            {
                isDamageFlag = true;
                EnemyDamageHit();
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && isAttackFlag && !dieFlag)
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (!playerController.damageFlag)
            {
                if (collision.gameObject.GetComponent<PlayerController>().playerHP > 0)
                {
                    playerController.damageAmount = enemyDamage;
                    playerController.damageFlag = true;
                    Vector3 knockback = (collision.gameObject.transform.position - transform.position).normalized;
                    rb.AddForce(knockback * knockbackPower * Time.deltaTime, ForceMode.Impulse);
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack" && !isDamageFlag) 
        {
            isDamageFlag = true; 
            EnemyDamageHit();            
        }
    }
    private void OnTriggerStay(Collider other)
    {
        // 照らされている判定（トリガーに入っている間は移動可能）
        if (other.gameObject.tag == "LightRange") moveFlag = true;
        // スタン判定（罠など）
        else if (other.gameObject.tag == "StanRange")
        {
            stanFlag = true;
            moveFlag = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "LightRange") moveFlag = false;
    }
}
