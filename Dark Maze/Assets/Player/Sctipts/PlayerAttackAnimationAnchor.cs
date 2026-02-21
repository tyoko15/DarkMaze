using UnityEngine;

/// <summary>
/// プレイヤーの攻撃アニメーション中の速度変化や、判定用武器モデルの表示・非表示を管理する
/// </summary>
public class PlayerAttackAnimationAnchor : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] GameObject sword; // 攻撃判定を持つ剣のモデル（またはコライダー）
    Animator animator;

    [Header("アニメーション速度の緩急設定")]
    [SerializeField] float startAttackSpeed; // 振りかぶり期の速度
    [SerializeField] float midAttackSpeed;   // 振り抜き期（攻撃発生中）の速度
    [SerializeField] float endAttackSpeed;   // 攻撃後の硬直期の速度
    void Start()
    {
        // 親オブジェクトにあるPlayerControllerを取得
        playerController = transform.parent.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 攻撃開始：振りかぶり
    /// </summary>
    public void StartAttackSpeed()
    {     
        animator.SetFloat("AttackSpeed", startAttackSpeed);
        animator.SetBool("AttackFlag", true); // アニメーション遷移用のフラグ
    }

    /// <summary>
    /// 攻撃中：ここで剣の判定を有効化する
    /// </summary>
    public void MidAttackSpeed()
    {
        sword.SetActive(true); // 剣（コライダー）を出現させる
        animator.SetFloat("AttackSpeed", midAttackSpeed);
    }

    /// <summary>
    /// 攻撃終了：判定を消し、硬直へ
    /// </summary>
    public void EndAttackSpeed()
    {
        sword.SetActive(false); // 剣（コライダー）を消す
        animator.SetFloat("AttackSpeed", endAttackSpeed);
    }

    /// <summary>
    /// モーション完全終了
    /// </summary>
    public void EndAttack()
    {
        // プレイヤーの操作禁止状態などを解除する
        playerController.SetAttackFlag(false);
        animator.SetBool("AttackFlag", false);
    }
}
