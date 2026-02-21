using UnityEngine;

/// <summary>
/// スイッチ・ボタンギミックを管理するクラス
/// 攻撃や重み（Box）によってフラグを切り替える
/// </summary>
public class ButtonManager : MonoBehaviour
{
    [Header("状態管理フラグ")]
    [SerializeField] public bool buttonFlag;   // 現在スイッチがONかどうか
    [SerializeField] bool hideFlag;           // 最初は隠されている（透明）か
    bool activeFlag;                          // ギミックとして機能しているか
    [SerializeField] bool somethingFlag;      // 一度押したら戻らないタイプか
    [SerializeField] bool senceFlag;          // 重み検知（Boxが乗っている間だけON）タイプか
    [SerializeField] public bool completeFlag; // ギミック全体が完了したか（外部参照用）
    int somethingNum;

    [Header("外見・演出設定")]
    bool moderingFlag;                        // MeshRendererの準備完了フラグ
    bool groundButtonFlag;                    // 地面設置型（マテリアル1つ）かどうか
    GameObject buttonObject;
    GameObject buttonLight;                   // スイッチON時に光る子オブジェクトのライト

    // [0]:ON時の色, [1]:OFF時の色
    [SerializeField] Color[] buttonCrystalColors;
    Material[] buttonMaterials;

    [Header("インターバル設定")]
    bool intervalFlag;                        // 連続入力を防ぐフラグ
    float intervalTime = 1;                   // 再入力までの待機時間
    float intervalTimer;

    private Camera mainCamera;
    private GameObject canvas;                // プレイヤーが近づいた時のガイドUI
    bool canvasFlag;

    private void Start()
    {
        buttonObject = gameObject;

        // --- マテリアルとライトのセットアップ ---
        if (buttonObject.GetComponent<MeshRenderer>())
        {             
            int materialCount = buttonObject.GetComponent<MeshRenderer>().materials.Length;
            buttonMaterials = new Material[materialCount];
            for (int i = 0; i < materialCount; i++) buttonMaterials[i] = buttonObject.GetComponent<MeshRenderer>().materials[i];

            // 子オブジェクトの0番目をライトとして取得
            if (buttonObject.transform.childCount != 0)
            {
                buttonLight = buttonObject.transform.GetChild(0).gameObject;
                buttonLight.SetActive(false);
            }

            // マテリアル数で地面ボタンかスタンドボタンか判別
            if (materialCount == 1) groundButtonFlag = true;

            // 初期状態で隠す設定の場合、アルファ値を0（透明）にする
            if (hideFlag)
            {
                for (int i = 0; i < materialCount; i++)
                {
                    Color c = buttonMaterials[i].color;
                    c.a = 0;
                    buttonMaterials[i].color = c;
                    buttonObject.GetComponent<MeshRenderer>().materials[i] = buttonMaterials[i];
                }
            }
            moderingFlag = true;
        }

        // --- 初期アクティブ状態の設定 ---
        if (hideFlag) 
        {
            activeFlag = false;
            gameObject.SetActive(false); // 非表示からスタート    
        }
        else activeFlag = true;

        // --- ガイドUIの取得 ---
        mainCamera = Camera.main;
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
    }

    void Update()
    {
        // 外見の更新（ON/OFFによる色の変化）
        if (moderingFlag)
        {
            if (!groundButtonFlag)
            {
                if (buttonFlag)
                {
                    buttonMaterials[0].color = buttonCrystalColors[0];
                    buttonLight.SetActive(true);
                }
                else
                {
                    buttonMaterials[0].color = buttonCrystalColors[1];
                    buttonLight.SetActive(false);
                }
            }         
        }

        // 隠れていたボタンが出現した（アルファが1になった）かチェック
        if (!activeFlag)
        {
            if (buttonObject.GetComponent<MeshRenderer>().materials[1].color.a == 1f) activeFlag = true;
        }

        // 入力インターバルのタイマー
        if (intervalFlag)
        {
            if (intervalTimer > intervalTime)
            {
                intervalTimer = 0;
                intervalFlag = false;
            }
            else intervalTimer += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;
        // UIをカメラに向けるビルボード処理
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z);
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    // --- 攻撃（トリガー）による判定 ---
    private void OnTriggerEnter(Collider collision)
    {
        if (!senceFlag && activeFlag)
        {
            // 攻撃や矢が当たった時、まだONでなければONにする
            if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                collision.gameObject.SetActive(false);
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
            else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                somethingNum++;
                collision.gameObject.SetActive(false);
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
        }
    }

    // --- 物理接触による判定（プレイヤーガイド表示など） ---
    private void OnCollisionEnter(Collision collision)
    {
        if (!intervalFlag)
        {
            if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                intervalFlag = true;
                if (collision.gameObject.name == $"Sword") collision.gameObject.SetActive(false);
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
            else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                somethingNum++;
                if (collision.gameObject.name == $"Sword") collision.gameObject.SetActive(false);
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
        }

        // プレイヤーが近づいたら「！」などのUIを表示
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(true);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(false);
    }

    // --- 重み検知（重量スイッチ）用 ---
    private void OnTriggerStay(Collider other)
    {
        // Boxが乗っている間だけON
        if (senceFlag && (other.gameObject.tag == "Box")) buttonFlag = true;
    }
    private void OnTriggerExit(Collider other)
    {
        // Boxが離れたらOFF
        if (senceFlag && (other.gameObject.tag == "Box")) buttonFlag = false;
    }
}
