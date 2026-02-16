using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [Header("参照・設定")]
    [SerializeField] GameObject box;      // 箱本体
    [SerializeField] GameObject player;   // プレイヤー
    [SerializeField] float moveTime;      // 1マス動くのにかかる時間
    [SerializeField] float completeTime;  // スイッチを起動させるまでの静止時間

    float moveTimer;
    float completeTimer;
    int directionNum;                     // 0:左(X+), 1:右(X-), 2:前(Z+), 3:後(Z-)
    bool lockFlag;                        // 移動中フラグ
    bool buttonFlag;                      // ボタンの上に乗っているか
    bool wallFlag;                        // 壁に接しているか
    int[] wallDirectionNum = new int[4];  // どの方向に壁があるか(1が壁あり)
    Vector3 originPosition;               // 移動開始時の座標

    private Camera mainCamera;
    GameObject canvas;                    // 操作ガイドUI
    bool canvasFlag;

    void Start()
    {
        mainCamera = Camera.main;
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
        // UIを少し斜めに傾けて配置
        canvas.transform.eulerAngles = new Vector3(75f, 0, 0);
    }

    void Update()
    {
        // 移動処理
        if (lockFlag && !buttonFlag)
        {
            MoveBox(directionNum);
            gameObject.tag = "Untagged"; // 移動中は他の判定を避ける
        }
        else
        {
            gameObject.tag = "Box";
        }

        // --- プレイヤーの明かりに応じたUI表示 ---
        if (buttonFlag) canvas.SetActive(false);
        else
        {
            // プレイヤーの光の範囲内にいる時だけ「！」などのUIを出す
            Vector3 playerPos = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
            float distance = Vector3.Distance(playerPos, myPos);
            float lightDistance = player.GetComponent<PlayerController>().GetPlayerLightRange() / 2f;
            bool flag = false;
            if (lightDistance > distance) flag = true;
            else flag = false;
            canvas.SetActive(flag);
        }
    }

    // プレイヤーがぶつかった瞬間に方向を計算
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == player && !lockFlag)
        {
            Direction();
        }
    }

    // 壁の検知とスイッチの判定
    private void OnTriggerStay(Collider other)
    {
        // 壁が自分のどっち側にあるかを判定して移動を制限するロジック
        if (other.gameObject.tag == "Wall")
        {
            float x = box.transform.position.x - other.transform.position.x;
            float z = box.transform.position.z - other.transform.position.z;
            if(Mathf.Abs(x) > Mathf.Abs(z))
            {
                if(x > 0)
                {
                    wallDirectionNum[1] = 1;
                    wallDirectionNum[0] = 0;
                }
                else if(x < 0)
                {
                    wallDirectionNum[0] = 1;
                    wallDirectionNum[1] = 0;
                }
            }
            else if (Mathf.Abs(x) < Mathf.Abs(z))
            {
                if(z > 0)
                {
                    wallDirectionNum[3] = 1;
                    wallDirectionNum[2] = 0;
                }
                else if(z < 0)
                {
                    wallDirectionNum[2] = 1;
                    wallDirectionNum[3] = 0;
                }
            }
            wallFlag = true;
        }

        // 地面スイッチ（10個まで対応）の判定
        for (int i = 0; i < 10; i++)
        {
            if (!lockFlag && other.gameObject.name == $"GroundButton ({i})")
            {
                buttonFlag = true;
                if (completeTimer > completeTime) other.gameObject.GetComponent<ButtonManager>().completeFlag = true;
                else if (completeTimer < completeTime) completeTimer += Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player && canvasFlag) canvas.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player && canvasFlag) canvas.SetActive(false);
    }
    void Direction()
    {
        float x = box.transform.position.x - player.transform.position.x;
        float z = box.transform.position.z - player.transform.position.z;
        // 左右からの衝突
        if(Mathf.Abs(x) > Mathf.Abs(z))
        {            
            if(x > 0) directionNum = 0; // 左            
            else if(x < 0) directionNum = 1; // 右
        }
        // 前後からの衝突
        else if(Mathf.Abs(x) < Mathf.Abs(z))
        {            
            if(z > 0) directionNum = 2; // 前            
            else if(z < 0) directionNum = 3; // 後
        }
        if(wallFlag)
        {
            for(int i = 0; i < wallDirectionNum.Length; i++)
            {
                if (directionNum == i && wallDirectionNum[i] == 0) lockFlag = true;
                else if (directionNum == i && wallDirectionNum[i] == 1) lockFlag = false;
            }
        }
        else lockFlag = true;
    }

    // 移動処理（Mathf.Lerpで2ユニット分スライド）
    void MoveBox(int direction)
    {
        // 左へ
        if (direction == 0)
        {
            if (moveTimer == 0)
            {
                originPosition = box.transform.position;
                AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 4); // ズズズ...という音
            }
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
                AudioManager.Instance.StopSE(AudioManager.SEName.gimmickSes, 4);
            }
            else if(moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float x = Mathf.Lerp(originPosition.x, originPosition.x + 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(x, box.transform.position.y, box.transform.position.z);
            }
        }
        // 右へ
        else if (direction == 1)
        {
            if (moveTimer == 0)
            {
                originPosition = box.transform.position;
                AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 4);
            }
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
                AudioManager.Instance.StopSE(AudioManager.SEName.gimmickSes, 4);
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float x = Mathf.Lerp(originPosition.x, originPosition.x - 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(x, box.transform.position.y, box.transform.position.z);
            }
        }
        // 前へ
        else if (direction == 2)
        {
            if (moveTimer == 0)
            {
                originPosition = box.transform.position;
                AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 4);
            }
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
                AudioManager.Instance.StopSE(AudioManager.SEName.gimmickSes, 4);
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float z = Mathf.Lerp(originPosition.z, originPosition.z + 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y, z);
            }
        }
        // 後へ
        else if (direction == 3)
        {
            if (moveTimer == 0)
            {
                originPosition = box.transform.position;
                AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 4);
            }
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
                AudioManager.Instance.StopSE(AudioManager.SEName.gimmickSes, 4);
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float z = Mathf.Lerp(originPosition.z, originPosition.z - 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y, z);
            }
        }
    }
}
