using UnityEngine;

/// <summary>
/// 特定のエリアへの進入を検知し、プレイヤーの親子関係やエリアフラグを管理するクラス
/// </summary>
public class EnterArea : MonoBehaviour
{
    [Header("エリア設定")]
    [SerializeField] GameObject area;       // プレイヤーの親となるエリアオブジェクト（動く床など）

    [Header("状態フラグ")]
    public bool enterAreaFlag;             // エリア内にプレイヤーがいるか
    [SerializeField] bool enemyAreaFlag;   // 敵が出現するなどの特殊なエリアか

    int count; // 初回進入判定用のカウンター

    /// <summary>
    /// トリガー（エリア判定）に何かが接触した時の処理
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // --- 1. 敵エリアへの初回進入チェック ---
        // 敵エリア設定がON、かつ未進入（count=0）で、入ってきたのがPlayerの場合
        if (enemyAreaFlag && count == 0 && !enterAreaFlag && other.gameObject.name == "Player")
        {
            enterAreaFlag = true;
            count = 1; // 一度だけ実行されるように制限
            // ここに「ドアを閉める」「敵を出す」などのトリガーを繋げることが可能
        }

        // --- 2. 親子関係の変更（プラットフォーム対応） ---
        // プレイヤーがこのエリアに入ったら、プレイヤーをエリアの子オブジェクトにする
        // これにより、エリア（床）が動いた時にプレイヤーも一緒に移動するようになる
        if (other.gameObject.name == "Player") other.gameObject.transform.parent = area.transform;
    }
}
