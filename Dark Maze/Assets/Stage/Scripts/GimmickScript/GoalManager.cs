using UnityEngine;

/// <summary>
/// ゴール地点の判定とクリアフラグの管理を行うクラス
/// </summary>
public class GoalManager : MonoBehaviour
{
    [Header("状態管理")]
    public bool isGoalFlag;     // ステージクリア（ゴール到達）したか
    bool onPlayerFlag;          // プレイヤーがゴール判定エリア内にいるか

    GameObject goalObject;      // ゴールオブジェクト自身
    GameObject playerObject;    // エリア内にいるプレイヤーへの参照

    void Start()
    {
        // 自身の参照を保持（距離計算に使用）
        goalObject = gameObject;
    }
    void Update()
    {
        // プレイヤーがゴール判定エリア（Trigger）内にいる間、距離をチェックし続ける
        if (onPlayerFlag)
        {
            // 高さ(Y軸)を無視し、平面(X, Z軸)での中心点との距離を計算
            float distance = Vector2.Distance(new Vector2(goalObject.transform.position.x, goalObject.transform.position.z), new Vector2(playerObject.transform.position.x, playerObject.transform.position.z));

            // ゴールの中心から0.6ユニット以内に近づいたらゴール確定
            if (distance < 0.6f) isGoalFlag = true;
        }
    }

    /// <summary>
    /// プレイヤーがゴールの外周エリアに入った時の処理
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            playerObject = other.gameObject;
            onPlayerFlag = true;
        } 
    }

    /// <summary>
    /// プレイヤーがゴールの外周エリアから出た時の処理
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            playerObject = null;
            onPlayerFlag = false;
        }       
    }
}
