using UnityEngine;

/// <summary>
/// クリア演出シーンにおいて、ステージに応じた背景切り替えとアニメーションを制御する
/// </summary>
public class PlayerClear : MonoBehaviour
{
    [Header("背景オブジェクトの設定")]
    [SerializeField] GameObject stage1; // ワールド1用の背景
    [SerializeField] GameObject stage2; // ワールド2用の背景

    [Header("アニメーション")]
    [SerializeField] Animator animator;
    void Start()
    {
        // --- クリアSEの再生 ---
        // 重複再生を防ぎつつ、クリア用の効果音を鳴らす
        if (!AudioManager.Instance.gameSEs[3].isPlaying) AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 3);
        
        // --- 現在のステージ情報の取得と背景設定 ---
        GameObject gameManager = GameObject.Find("GameManager").gameObject;
        if (gameManager != null)
        {
            GeneralStageManager manager = gameManager.GetComponent<GeneralStageManager>();
            int stage = manager.stageNum;

            // ステージ番号から「どのワールドか」を計算
            // 例：ステージ0?4ならワールド1、5?9ならワールド2
            int feildNum = stage / 5 + 1;
            int stageNum = stage % 5; // そのワールド内でのステージ番号

            // 計算されたワールドに応じて背景オブジェクトをアクティブにする
            if (feildNum == 1) stage1.SetActive(true);
            else if (feildNum == 2) stage2.SetActive(true);
        }
        else
        {
            // GameManagerが見つからない場合のデフォルト背景
            stage1.SetActive(true);
        }

        // --- クリアアニメーション（ポーズなど）の開始 ---
        animator.SetTrigger("Clear");
    }
}
