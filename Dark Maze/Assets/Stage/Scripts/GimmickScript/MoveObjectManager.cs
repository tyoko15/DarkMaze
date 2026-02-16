using UnityEngine;

/// <summary>
/// 指定されたポイント間を往復移動するオブジェクト（動く床など）を管理するクラス
/// </summary>
public class MoveObjectManager : MonoBehaviour
{
    [SerializeField] GameObject playerObject;   // プレイヤーへの参照
    [SerializeField] GameObject moveObject;     // 動かしたいオブジェクト本体
    [SerializeField] GameObject[] pointsObjects; // 移動先となるポイント（空のGameObject等）

    int pointsNum;           // 現在の目標ポイントのインデックス
    bool moveFlag = true;    // 現在移動中かどうか
    bool returnFlag;         // 往路(false)か復路(true)か

    [SerializeField] float moveTime;  // ポイント間の移動にかける時間
    float moveTimer;

    [SerializeField] float stopTime;  // 到着時の停止（待ち）時間
    float stopTimer;

    void Update()
    {
        moveControl();
    }

    /// <summary>
    /// 移動と停止のサイクルを制御する
    /// </summary>
    void moveControl()
    {
        // --- 停止中の処理 ---
        if (!moveFlag)
        {
            if(stopTimer > stopTime)
            {
                // 待ち時間が経過したら移動を再開
                moveFlag = true;
                stopTimer = 0;
            }
            else if(stopTimer < stopTime)
            {
                // 待ち時間をカウントアップ
                stopTimer += Time.deltaTime;
            }
        }
        // --- 移動中の処理 ---
        else
        {
            // 移動開始の瞬間（タイマー0）に、端に到達したかチェックして折り返し判定を行う
            if (moveTimer == 0)
            {
                // 終点に到達した場合：復路モードへ
                if (pointsObjects.Length - 1 == pointsNum && !returnFlag)
                {
                    pointsNum = pointsObjects.Length - 1;
                    returnFlag = true;
                }
                // 始点に戻ってきた場合：往路モードへ
                else if (pointsNum == 0 && returnFlag)
                {
                    returnFlag = false;
                    pointsNum = 0;
                }
            }

            // ポイント間の移動が完了した場合
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                // 次の目標ポイントをセット（復路なら戻り、往路なら進む）
                if (returnFlag) pointsNum--;
                else pointsNum++;
                moveFlag = false; // 一旦停止状態へ
            }
            // 移動計算（Lerpによる補間）
            else
            {
                // 復路：現在の点(pointsNum)から一つ前の点(pointsNum-1)へ
                if (returnFlag)
                {
                    moveTimer += Time.deltaTime;
                    float x = Mathf.Lerp(pointsObjects[pointsNum].transform.position.x, pointsObjects[pointsNum-1].transform.position.x, moveTimer / moveTime);
                    float z = Mathf.Lerp(pointsObjects[pointsNum].transform.position.z, pointsObjects[pointsNum-1].transform.position.z, moveTimer / moveTime);
                    moveObject.transform.position = new Vector3(x, moveObject.transform.position.y, z);
                }
                // 往路：現在の点(pointsNum)から一つ先の点(pointsNum+1)へ
                else
                {
                    moveTimer += Time.deltaTime;
                    float x = Mathf.Lerp(pointsObjects[pointsNum].transform.position.x, pointsObjects[pointsNum + 1].transform.position.x, moveTimer / moveTime);
                    float z = Mathf.Lerp(pointsObjects[pointsNum].transform.position.z, pointsObjects[pointsNum + 1].transform.position.z, moveTimer / moveTime);
                    moveObject.transform.position = new Vector3(x, moveObject.transform.position.y, z);
                }
            }
        }
    }

    // --- プレイヤーが乗った時の親子関係の制御 ---
    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーが乗ったら、移動する床を親にする（一緒に動くようにする）
        if (collision.gameObject == playerObject) playerObject.transform.parent = moveObject.transform;
    }

    private void OnCollisionExit(Collision collision)
    {
        // プレイヤーが離れたら親子関係を解除する
        // ※元の階層（parent.parent等）に戻すことでシーン上の適切な場所に復帰させる
        if (collision.gameObject == playerObject) playerObject.transform.parent = moveObject.transform.parent.parent;
    }
}
