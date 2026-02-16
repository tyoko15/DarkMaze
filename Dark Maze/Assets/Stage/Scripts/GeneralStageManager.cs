using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 全ステージの基盤となる親クラス。
/// ギミックの基本動作、UI管理、ステート制御（開始・停止・クリア等）を共通化する。
/// </summary>
public class GeneralStageManager : MonoBehaviour
{
    [Header("システム・マネージャー参照")]
    [SerializeField] public int stageNum;               // ステージ番号 (1-1なら11など)
    [SerializeField] public GameObject fadeManagerObject;
    public FadeManager fadeManager;                    // 画面フェード制御
    [SerializeField] GameObject audioManagerObject;
    AudioManager audioManager;                         // 音響制御
    public bool fadeFlag;                              // 現在フェード中かどうか

    // ゲームの進行状態を定義
    public enum GameStatus
    {
        start, // 開始演出中
        play,  // プレイ中
        stop,  // ギミック演出などで一時停止中
        menu,  // メニュー表示中
        over,  // ゲームオーバー
        clear, // ステージクリア演出中
    }
    public GameStatus status = GameStatus.start;

    [Header("プレイヤー・ナビゲーション")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] public GameObject player;
    [SerializeField] public NavMeshSurface stageNav;    // 床が動いた後のAI経路再計算用

    [Header("ステージ構成オブジェクト配列")]
    [SerializeField] public GameObject[] areas;            // 回転させる床などのエリア単位
    [SerializeField] public GameObject startObject;        // 開始地点
    [SerializeField] public GameObject goalObject;         // ゴール地点
    [SerializeField] public GameObject[] buttonObjects;    // 物理スイッチ
    [SerializeField] public GameObject[] gateObjects;      // 門（ギミックで動く壁）
    [SerializeField] public GameObject[] activeObject;     // 出現/消失する足場など
    [SerializeField] public GameObject[] areaLightObjects; // 各エリアを照らす環境光
    [SerializeField] public GameObject[] lightObjects;     // 演出用のポイントライト等
    [SerializeField] public GameObject mainCamera;         // メインカメラ
    [SerializeField] public GameObject[] cameraPointObjects; // 演出時にカメラが移動する座標
    [SerializeField] public GameObject[] enemys;           // 各エリアの敵グループ（子オブジェクトの数で判定）

    [Header("開始演出設定")]
    [SerializeField] public float startTime = 1.5f;        // 開始時の待機時間
    float startTimer;

    [Header("演出・ギミック用タイマーとフラグ")]
    // カメラワーク（最大10個までの並行/連続演出を想定）
    float[] cameraTimer = new float[10];
    Vector3 cameraPosi;                                    // カメラ移動の開始座標保存用
    Vector3 cameraRota;                                    // カメラ回転の開始角度保存用
    bool[] cameraWorkStartFlag = new bool[10];
    bool[] cameraWorkEndFlag = new bool[10];

    // 地形回転ギミック
    public float originDegree;                             // 回転開始前の角度
    [SerializeField] float[] rotationTimer;                // 各エリアの回転にかかっている時間
    [SerializeField] bool rotationFlag;                    // 回転中フラグ

    // 門の開閉ギミック
    [SerializeField] float[] openTimer;
    float nowHeight;                                       // 現在の門の高さ
    bool oldOpenFlag;                                      // 前フレームの状態

    // 出現・消失（フェードイン/アウト）ギミック
    [SerializeField] float limitActiveObTime;              // 制限時間付き出現の保持時間
    bool endFadeInFlag;
    [SerializeField] float[] limitActiveObTimer;           // 制限時間計測用配列
    [SerializeField] float[] activeObTimer;                // 通常出現のフェード時間
    [SerializeField] float[] activeLightTimer;             // ライト点灯の演出時間
    [SerializeField] bool[] activeFlag;                    // 動作中フラグ

    // エリア進入・攻略フラグ
    [SerializeField] public EnterArea[] enterArea;         // プレイヤーのエリア進入判定
    [SerializeField] public bool[] defeatGateFlag;         // 敵全滅による門開放済みフラグ

    [Header("UI・キャンバス情報")]
    [SerializeField] public GameObject playUI;             // プレイ中のHUD
    [SerializeField] public GameObject startUI;            // 開始ロゴ等
    [SerializeField] public GameObject menuUI;             // ポーズ画面
    [SerializeField] public GameObject overUI;             // 死亡画面
    [SerializeField] public GameObject clearUI;            // クリア画面

    [Header("UIフェード演出用")]
    public bool fadeMenuFlag;
    [SerializeField] public float fadeMenuTime = 0.2f;
    float fadeMenuTimer;
    public bool fadeOverFlag;
    [SerializeField] public float fadeOverTime = 0.2f;
    float fadeOverTimer;
    public bool fadeClearFlag;
    [SerializeField] public float fadeClearTime = 0.2f;
    float fadeClearTimer;

    [Header("UIテキスト・選択管理")]
    [SerializeField] public GameObject startText;
    [SerializeField] public GameObject[] menuTexts;        // メニューの選択項目
    [SerializeField] public GameObject[] overTexts;        // オーバー画面の項目
    [SerializeField] public GameObject[] clearTexts;       // クリア画面の項目

    // 操作フラグ
    [SerializeField] public bool menuFlag;                 // メニューを開く入力があったか
    [SerializeField] public bool overFlag;                 // 死亡が確定したか
    [SerializeField] public bool clearFlag;                // クリアが確定したか
    [SerializeField] public bool enterFlag;                // 決定キー入力
    [SerializeField] public int menuSelectNum;             // 現在のメニュー選択位置
    [SerializeField] public int overSelectNum;             // 現在のオーバー画面選択位置

    [Header("クリア演出設定")]
    bool clearAnimeFlag;
    [SerializeField] public float clearAnimeTime = 2.5f;   // クリア時の演出時間
    float clearAnimeTimer;

    void Start()
    {
        rotationTimer = new float[areas.Length];
        openTimer = new float[gateObjects.Length];
        activeObTimer = new float[activeObject.Length];
        activeLightTimer = new float[lightObjects.Length];
        activeFlag = new bool[activeObject.Length];
        defeatGateFlag = new bool[enemys.Length];
        menuTexts = new GameObject[3];
    }

    /// <summary>
    /// オーディオマネージャーの準備を待ってからBGMを再生する
    /// </summary>
    IEnumerator GetAudio()
    {
        // Instanceが生成され、準備完了(isReady)になるまで待機
        yield return new WaitUntil(() =>
            AudioManager.Instance != null & AudioManager.Instance.isReady
        );
        // ゲーム用BGMの1曲目(インデックス0)を再生
        audioManager.PlayBGM(AudioManager.BGMName.gameBgms, 0);
    }

    /// <summary>
    /// ステージ開始時のデータ初期化処理
    /// フェード、セーブデータ、オーディオのセットアップを行う
    /// </summary>
    public void StartData()
    {
        GetUIandPlayer();　// UIとプレイヤーの参照を取得

        // フェードマネージャーの生成と取得
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
            fadeManager.AfterFade(); // 初期化後の処理
        }
        else if (fade != null) fadeManager = fade.GetComponent<FadeManager>();
        fadeManager.fadeOutFlag = true; // 暗転から明転へのフラグを立てる
        fadeFlag = true;

        // 敵全滅による門開放フラグを初期化
        for (int i = 0; i < defeatGateFlag.Length; i++)
        {
            defeatGateFlag[i] = true;
        }

        // セーブデータ（DataManager）からクリア状況を読み込む
        if (GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        // データマネージャーがない場合は、現在のステージの1つ前までクリアしていると仮定
        else if (GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = stageNum - 1;

        // オーディオマネージャーのシングルトン取得または生成
        if (GameObject.Find("AudioManager") != null)
        {
            audioManager = AudioManager.Instance; 
        }
        else
        {
            GameObject ob = Instantiate(audioManagerObject, Vector3.zero, Quaternion.identity);
            audioManager = ob.GetComponent<AudioManager>();
        }
        StartCoroutine(GetAudio()); // BGM再生開始
    }

    /// <summary>
    /// ステージ開始時の演出（フェードアウト後のテキスト表示）
    /// </summary>
    public void StartAnime()
    {
        if (fadeFlag)
        {
            // フェード演出が終わるまで待機
            if (fadeManager.fadeOutFlag && fadeManager.endFlag)
            {
                fadeManager.fadeOutFlag = false;
                fadeManager.endFlag = false;
                fadeFlag = false;
            }
            fadeManager.FadeControl();
        }
        else
        {
            // プレイヤーをスタート地点に微調整して配置
            player.transform.position = new Vector3(startObject.transform.position.x, startObject.transform.position.y + 0.25f, startObject.transform.position.z);
            if (startTimer > startTime)
            {
                // 演出時間終了、ゲームプレイ開始
                startTimer = 0;
                startUI.SetActive(false);
                status = GameStatus.play;                
            }
            else if (startTimer < startTime)
            {
                startTimer += Time.deltaTime;
                startUI.SetActive(true);

                // ステージ番号を「ワールド-エリア」形式に計算 (例: stageNum 5 -> 2 - 1)
                int f = f = stageNum / 4;
                int s = 0;
                if (stageNum % 4 == 0)
                {
                    f--;
                    s = 4;
                }
                else s = stageNum - f * 4;

                // 前半は「ステージ 1-1」、後半は「スタート」とテキストを切り替える
                TextMeshProUGUI text = startText.GetComponent<TextMeshProUGUI>();
                if (startTimer < startTime / 2) text.text = $"ステージ\n{f + 1} - {s}\n";
                else text.text = $"スタート\n";
            }
        }
    }

    /// <summary>
    /// ステージクリア時の演出とデータ保存
    /// </summary>
    public void EndAnime()
    {
        clearUI.SetActive(true);
        if (fadeFlag)
        {
            // 暗転終了を待つ
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
            fadeManager.FadeControl();
        }
        else if (!fadeFlag && clearAnimeFlag)
        {
            // クリアデータの書き込みとステージ選択画面への遷移
            if (GameObject.Find("DataManager") != null)
            {
                DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                int dataNum = dataManager.useDataNum;
                if (stageNum != 8) // 全8ステージ想定。最終ステージ以外なら進行度を更新
                {
                    if (dataManager.data[dataNum].clearStageNum == stageNum - 1) dataManager.data[dataNum].clearStageNum = stageNum;
                    dataManager.SaveData(dataNum, dataManager.data[dataNum].playerName, dataManager.data[dataNum].clearStageNum, stageNum - 1);
                }
            }
            SceneManager.LoadScene("StageSelect");
        }
        else if (!fadeFlag && !clearAnimeFlag)
        {
            // クリア後の待機時間計測
            if (clearAnimeTimer > clearAnimeTime)
            {
                clearAnimeTimer = 0f;
                clearAnimeFlag = true;
                fadeFlag = true;
                fadeManager.fadeInFlag = true; // 最後にフェードイン（暗転）させて遷移へ
            }
            else if (clearAnimeTimer < clearAnimeTime)
            {
                clearAnimeTimer += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// ゴール判定の監視
    /// </summary>
    public void Goal()
    {
        if (goalObject.GetComponent<GoalManager>().isGoalFlag)
        {
            status = GameStatus.clear;
            // クリア画面を重ねて表示
            SceneManager.LoadScene("Clear", LoadSceneMode.Additive);
            clearUI.SetActive(true);
            playUI.SetActive(false);
            audioManager.StopBGM(AudioManager.BGMName.gameBgms, 0);
        }
    }

    /// <summary>
    /// プレイヤーの死亡判定
    /// </summary>
    public void JudgeOver()
    {
        if (playerController.playerHP <= 0)
        {
            status = GameStatus.over;
            overFlag = true;
            audioManager.PlayOneShotSE(AudioManager.SEName.gameSes, 4);
        }
    }

    /// <summary>
    /// ゲームオーバー状態のUI制御
    /// </summary>
    public void Over()
    {
        overUI.SetActive(true);
    }

    /// <summary>
    /// 地形回転ギミックのコアロジック
    /// カメラ演出、回転アニメーション、NavMeshの再構築を連続して行う
    /// </summary>
    /// <param name="area">回転させるGameObject</param>
    /// <param name="light">演出用ライト（任意）</param>
    /// <param name="cameraPoint">ギミック注視用のカメラ座標</param>
    /// <param name="direction">回転方向 (1 or -1)</param>
    /// <param name="degree">回転する角度</param>
    /// <param name="time">回転にかかる時間</param>
    /// <param name="i">管理用インデックス</param>
    /// <param name="end">終了時にフラグを倒すか</param>
    /// <param name="flag">ギミックの実行フラグ（ref渡し）</param>
    public void AreaRotation(GameObject area, GameObject light, GameObject cameraPoint, int direction, int degree, float time, int i, bool end, ref bool flag)
    {
        if (flag)
        {
            // --- A: カメラ演出（注視点）がある場合 ---
            if (cameraPoint)
            {
                // 1. 演出開始の初期化
                if (rotationTimer[i] == 0f && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop; // ゲームを一時停止
                    originDegree = area.transform.localEulerAngles.y; // 開始時の角度を記録

                    // 演出用ライトの設定（スポットライトを広げて強調）
                    if (light != null) 
                    {
                        light.SetActive(true);
                        Light L = light.GetComponent<Light>();
                        L.innerSpotAngle = 180f;
                        L.spotAngle = 180f;
                        L.intensity = 30f;
                    }
                    // 現在のカメラ位置と回転をバックアップ
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 2. 最初のカメラ移動（プレイヤー → ギミック地点）
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    // 移動完了
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                    audioManager.PlaySE(AudioManager.SEName.gimmickSes, 1); // 回転音再生
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    // Lerpによる滑らかなカメラ移動
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    // 角度の正規化（180度を超えた場合の補正）
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }

                // 3. メインのエリア回転アニメーション
                if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    // 回転完了
                    rotationTimer[i] = 0;
                    area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);

                    // 戻るためのカメラ位置を再設定
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f); // 基本の俯瞰角度
                    cameraWorkEndFlag[i] = true;
                    audioManager.StopSE(AudioManager.SEName.gimmickSes, 1);
                }
                else if (rotationTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    // Lerpによる滑らかな地形回転
                    rotationTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
                    area.transform.rotation = Quaternion.Euler(0, y, 0);
                }

                // 4. 最後のカメラ移動（ギミック地点 → プレイヤー）
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    // すべての演出が終了
                    status = GameStatus.play;
                    mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraPosi = Vector3.zero;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) 
                    {
                        flag = false;
                        // 【重要】地形が変わったのでNavMesh（AIの道）を焼き直す
                        stageNav.RemoveData();
                        stageNav.BuildNavMesh();
                    }
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            // --- B: カメラ演出がない場合の簡易処理（ロジックはAと同様だがカメラ移動をスキップ） ---
            else
            {
                // スタート
                if (rotationTimer[i] == 0f && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    originDegree = area.transform.localEulerAngles.y;
                    if (light != null) light.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                // エリア回転
                if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] = 0;
                    area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
                    cameraWorkEndFlag[i] = true;
                }
                else if (rotationTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
                    area.transform.rotation = Quaternion.Euler(0, y, 0);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end)
                    {
                        flag = false;
                        stageNav.RemoveData();
                        stageNav.BuildNavMesh();
                    }
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
    }

    /// <summary>
    /// ゲート（門）の昇降ギミック。
    /// カメラ演出の有無、開閉状態の反転、途中からの動作再開に対応。
    /// </summary>
    /// <param name="gate">動かすゲートのオブジェクト</param>
    /// <param name="light">演出用スポットライト</param>
    /// <param name="cameraPoint">注視カメラの座標（nullならカメラ移動なし）</param>
    /// <param name="open">trueで開く(-2.1f)、falseで閉じる(0f)</param>
    /// <param name="complete">ギミックが完了済みか（外部フラグ）</param>
    /// <param name="time">開閉にかかる時間</param>
    /// <param name="i">管理用インデックス</param>
    public void SenceGate(GameObject gate, GameObject light, GameObject cameraPoint, bool open, bool complete, float time, int i)
    {
        if (!complete)
        {
            // 1. 状態が切り替わった瞬間の処理（タイマーの逆算）
            if (open != oldOpenFlag)
            {
                float a;
                nowHeight = gate.transform.position.y;
                if (open)
                {
                    // 現在の高さから、全開(-2.1f)までの進捗率を計算してタイマーをセット
                    a = Mathf.InverseLerp(0f, -2.1f, nowHeight);
                    openTimer[i] = a * time;
                }
                else if (!open)
                {
                    // 現在の高さから、全閉(0f)までの進捗率を計算してタイマーをセット
                    a = Mathf.InverseLerp(-2.1f, 0f, nowHeight);
                    openTimer[i] = a * time;
                }
            }

            // 2. カメラ演出がある場合
            if (cameraPoint != null)
            {
                // 閉じる
                if (!open)
                {
                    // --- 演出開始（カメラ移動・ライト点灯） ---
                    if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        if (light != null)
                        {
                            light.SetActive(true);
                            Light L = light.GetComponent<Light>();
                            L.innerSpotAngle = 60f;
                            L.spotAngle = 60f;
                            L.intensity = 30f;
                        }
                        status = GameStatus.stop; // プレイヤー入力を停止
                        cameraWorkStartFlag[i] = true;
                        cameraPosi = mainCamera.transform.position;
                        cameraRota = mainCamera.transform.eulerAngles;
                    }
                    // --- 最初のカメラ移動（Lerp） ---
                    if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        audioManager.PlaySE(AudioManager.SEName.gimmickSes, 5); // 門の駆動音
                        cameraWorkStartFlag[i] = false;
                        cameraTimer[i] = 0f;
                        if (cameraPoint.transform.eulerAngles.y >= 180f) mainCamera.transform.position = cameraPoint.transform.position;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                    }
                    else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                    // --- ゲートの昇降本体 ---
                    if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        audioManager.StopSE(AudioManager.SEName.gimmickSes, 5);
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                        cameraWorkEndFlag[i] = true;
                    }
                    else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, 0f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                    // --- 演出終了（カメラを戻す） ---
                    if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f && cameraPoint != null)
                    {
                        status = GameStatus.play;
                        if (light != null) light.SetActive(false);
                        mainCamera.transform.position = cameraPosi;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                        cameraWorkEndFlag[i] = false;
                        cameraTimer[i] = 0f;
                    }
                    else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                }
                // 3. カメラ演出がない場合（移動ロジックのみ実行）
                else
                {
                    if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        if (light != null)
                        {
                            light.SetActive(true);
                            Light L = light.GetComponent<Light>();
                            L.innerSpotAngle = 60f;
                            L.spotAngle = 60f;
                            L.intensity = 30f;
                        }
                        status = GameStatus.stop;
                        cameraWorkStartFlag[i] = true;
                        cameraPosi = mainCamera.transform.position;
                        cameraRota = mainCamera.transform.eulerAngles;
                    }
                    // 最初のカメラ移動
                    if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        audioManager.PlaySE(AudioManager.SEName.gimmickSes, 5);                        
                        cameraWorkStartFlag[i] = false;
                        cameraTimer[i] = 0f;
                        if (cameraPoint.transform.eulerAngles.y >= 180f) mainCamera.transform.position = cameraPoint.transform.position;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                    }
                    else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                    if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        audioManager.StopSE(AudioManager.SEName.gimmickSes, 5);
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                        cameraWorkEndFlag[i] = true;
                    }
                    else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, -2.1f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                    // 最後のカメラ移動
                    if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        status = GameStatus.play;
                        if (light != null) light.SetActive(false);
                        mainCamera.transform.position = cameraPosi;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                        cameraWorkEndFlag[i] = false;
                        cameraTimer[i] = 0f;
                    }
                    else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                }
                oldOpenFlag = open;
            }
            else if (cameraPoint == null)
            {
                // 閉じる
                if (!open)
                {
                    if (openTimer[i] > time)
                    {
                        status = GameStatus.play;
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    }
                    else if (openTimer[i] < time)
                    {
                        status = GameStatus.stop;
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, 0f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                }
                // 開ける
                else
                {
                    if (openTimer[i] > time)
                    {
                        status = GameStatus.play;
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    }
                    else if (openTimer[i] < time)
                    {
                        status = GameStatus.stop;
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, -2.1f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                }
                oldOpenFlag = open;
            }
        }
    }

    /// <summary>
    /// イベント用ゲート開閉ギミック
    /// 動作完了時にフラグを倒し、一連の演出を完結させる
    /// </summary>
    public void Gate(GameObject gate, GameObject light, GameObject cameraPoint, bool open, float time, int i, bool end, ref bool flag)
    {
        // --- 【開く動作】 ---
        if (open)
        {
            if (cameraPoint != null)
            {
                // 1. 演出開始の初期化
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    gate.SetActive(true);
                    if (light != null)
                    {
                        light.SetActive(true);
                        Light L = light.GetComponent<Light>();
                        L.innerSpotAngle = 60f;
                        L.spotAngle = 60f;
                        L.intensity = 30f;
                    }
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }

                // 2. 最初のカメラ移動（Lerp）
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    audioManager.PlaySE(AudioManager.SEName.gimmickSes, 5);
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    // 角度の補正（180度を超えた場合の処理）
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles; 
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    cameraPoint.transform.eulerAngles = caPoRota;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }

                // 3. ゲートオープン本体（0f -> -2.1f）
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    audioManager.StopSE(AudioManager.SEName.gimmickSes, 5);
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(false); // 開ききったら非アクティブ化して通行可能に
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }

                // 4. 最後のカメラ移動（戻り）
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    if (light != null) light.SetActive(false);
                    mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if(end) flag = false; // ギミック終了フラグ
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if (cameraPoint == null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(true);
                    openTimer[i] = 0f;
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    if (light != null)
                    {
                        light.SetActive(true);
                        Light L = light.GetComponent<Light>();
                        L.innerSpotAngle = 60f;
                        L.spotAngle = 60f;
                        L.intensity = 30f;
                    }
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
        // --- 【閉じる動作】 ---
        else if (!open)
        {
            if (cameraPoint != null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    gate.SetActive(true);
                    if (light != null)
                    {
                        light.SetActive(true);
                        Light L = light.GetComponent<Light>();
                        L.innerSpotAngle = 60f;
                        L.spotAngle = 60f;
                        L.intensity = 30f;
                    }
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    audioManager.PlaySE(AudioManager.SEName.gimmickSes, 5);
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateClose
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    audioManager.StopSE(AudioManager.SEName.gimmickSes, 5);
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    mainCamera.transform.position = cameraPosi;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if (cameraPoint == null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    gate.SetActive(true);
                    openTimer[i] = 0f;
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    if (light != null)
                    {
                        light.SetActive(true);
                        Light L = light.GetComponent<Light>();
                        L.innerSpotAngle = 60f;
                        L.spotAngle = 60f;
                        L.intensity = 30f;
                    }
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
    }

    /// <summary>
    /// 制限時間付きのオブジェクト出現ギミック。
    /// 透明度を操作してフェードイン/アウトし、終了間際に警告音を鳴らす。
    /// </summary>
    public void LimitActiveObject(GameObject activeOb, GameObject light, int i, bool end, ref bool flag)
    {
        int activeObParentCount = activeOb.transform.childCount;
        Material[] activeObMaterials = new Material[activeObParentCount];

        // 1. 出現の瞬間（初期化）
        if (limitActiveObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            // 子要素（パーツ）すべてのマテリアルを透明(Alpha=0)にしてから開始
            for (int n = 0; n < activeObParentCount; n++)
            {
                activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                color.a = 0f;
                activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
            }
            activeOb.SetActive(true); // オブジェクト自体を有効化
            if (light != null)
            {
                light.SetActive(true);
                Light L = light.GetComponent<Light>();
                L.innerSpotAngle = 60f;
                L.spotAngle = 60f;
                L.intensity = 30f;
            }
            cameraWorkStartFlag[i] = true;
        }

        // 2. カメラ演出のウェイト（0.5秒）
        if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
        {
            cameraTimer[i] = 0f;
            cameraWorkStartFlag[i] = false;
        }
        else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
        }

        // 3. タイムアップ処理（消失）
        if (limitActiveObTimer[i] > limitActiveObTime && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            for (int n = 0; n < activeObParentCount; n++)
            {
                activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                color.a = 0f;
                activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
            }
            // ...透明にして非アクティブ化...
            activeOb.SetActive(false);
            limitActiveObTimer[i] = 0;
            cameraWorkEndFlag[i] = true; 
            audioManager.StopSE(AudioManager.SEName.gimmickSes, 3); // 警告音停止
        }
        // 4. 出現中の更新処理
        else if (limitActiveObTimer[i] < limitActiveObTime && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            // --- サウンド演出 ---
            // 出現音(2番)を鳴らし、終了1.1秒前になったら警告音(3番)に切り替える
            if (limitActiveObTimer[i] == 0) audioManager.PlaySE(AudioManager.SEName.gimmickSes, 2);
            else if (limitActiveObTimer[i] > limitActiveObTime - 1.1f)
            {
                if (!audioManager.gimmickSEs[3].isPlaying) audioManager.PlaySE(AudioManager.SEName.gimmickSes, 3);
                if (audioManager.gimmickSEs[2].isPlaying) audioManager.StopSE(AudioManager.SEName.gimmickSes, 2);                
            }

            // --- フェードイン演出 (開始0.2秒間) ---
            if (limitActiveObTimer[i] < 0.2f && !endFadeInFlag)
            {
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    float a = Mathf.Lerp(0f, 1f, limitActiveObTimer[i] / limitActiveObTime);
                    color.a = a;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
            }
            else if (limitActiveObTimer[i] > 0.2f && !endFadeInFlag)
            {
                // 完全に不透明にして固定
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    color.a = 1f;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
                endFadeInFlag = true;
            }

            // --- フェードアウト演出 (終了0.2秒前から) ---
            if (limitActiveObTimer[i] > limitActiveObTime - 0.2f)
            {
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    float a = Mathf.Lerp(1f, 0f, limitActiveObTimer[i] / limitActiveObTime);
                    color.a = a;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
            }
            limitActiveObTimer[i] += Time.deltaTime;
        }

        // 5. 後処理
        if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
        {
            cameraTimer[i] = 0f;
            if (light != null) light.SetActive(false);
            if (end) flag = false;
            endFadeInFlag = false;
            cameraWorkEndFlag[i] = false;
        }
        else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
        }       
    }

    /// <summary>
    /// 透明なオブジェクトを徐々に実体化（可視化）させるギミック。
    /// cameraPointが指定されている場合、その地点へカメラを移動させる演出が入る。
    /// </summary>
    public void ActiveObject(GameObject activeOb, GameObject light, GameObject cameraPoint, float time, int i, bool end, ref bool flag)
    {
        // --- パターンA：カメラ演出なし（その場で出現） ---
        if (cameraPoint == null)
        {
            // 1. 出現開始時の初期化
            if (activeObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                status = GameStatus.stop; // プレイヤーの操作を停止
                activeOb.SetActive(true);

                // マテリアルのアルファ値を0（透明）に初期化
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];                        
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                if (light != null)
                {
                    light.SetActive(true);
                    Light L = light.GetComponent<Light>();
                    L.innerSpotAngle = 60f;
                    L.spotAngle = 60f;
                    L.intensity = 30f;
                }
                cameraWorkStartFlag[i] = true;
            }

            // 2. 開始ウェイト（0.5秒の猶予）
            if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
            {
                cameraWorkStartFlag[i] = false;
                cameraTimer[i] = 0f;
            }
            else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
            }

            // 3. フェードイン処理完了（実体化終了）
            if (activeObTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f; // 完全に不透明にする
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else if (!activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                activeObTimer[i] = time;
            }
            // 4. フェードイン継続中
            else if (activeObTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                activeObTimer[i] += Time.deltaTime;
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
            }

            // 5. 終了処理（操作権の返却）
            if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
            {
                status = GameStatus.play;
                cameraTimer[i] = 0f;
                cameraWorkEndFlag[i] = true;
                if (end) flag = false;
                if (light != null) light.SetActive(false);
            }
            else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
            }
        }
        // --- パターンB：カメラ演出あり（注目演出） ---
        else if (cameraPoint != null)
        {
            // 1. 演出開始：カメラの現在地を記録し、移動フラグを立てる
            if (activeObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                status = GameStatus.stop;
                activeOb.SetActive(true);
                // マテリアルの取得
                Material[] activeMaterials;
                int index = 0;
                if (activeOb.GetComponent<ChestManger>() == null)
                {
                    activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                    }
                    // 
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        if (activeOb.GetComponent<MeshRenderer>().materials[n] != null) activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    for (int m = 0; m < activeOb.transform.childCount; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>() != null) index++;
                    }
                    activeMaterials = new Material[index];
                    for (int m = 0; m < activeMaterials.Length; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material != null) activeMaterials[m] = activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material;
                    }
                    // 
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        if (activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material != null) activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                
                if (light != null)
                {
                    light.SetActive(true);
                    Light L = light.GetComponent<Light>();
                    L.innerSpotAngle = 60f;
                    L.spotAngle = 60f;
                    L.intensity = 30f;
                }
                // 元のカメラ位置を保存（演出終了後に戻すため）
                cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                cameraRota = mainCamera.transform.eulerAngles;
                cameraWorkStartFlag[i] = true;
            }

            // 2. カメラ移動（プレイヤー → 注視点へ）
            if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
            {
                // 到着：位置を完全に固定
                cameraWorkStartFlag[i] = false;
                cameraTimer[i] = 0f;
                mainCamera.transform.position = cameraPoint.transform.position;
                mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
            }
            else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
            {
                // Lerpによる滑らかなカメラ移動
                cameraTimer[i] += Time.deltaTime;
                Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                mainCamera.transform.position = posi;
                mainCamera.transform.rotation = Quaternion.Euler(rota);
            }

            // 3. 実体化フェーズ（マテリアルのアルファ値をLerpで1.0へ）
            if (activeObTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                // マテリアルの取得
                Material[] activeMaterials;
                int index = 0;
                if (activeOb.GetComponent<ChestManger>() == null)
                {
                    activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                    }

                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    for (int m = 0; m < activeOb.transform.childCount; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>() != null) index++;
                    }
                    activeMaterials = new Material[index];
                    for (int m = 0; m < activeMaterials.Length; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material != null) activeMaterials[m] = activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material;
                    }

                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }

                activeObTimer[i] = 0f;
                cameraWorkEndFlag[i] = true;
            }
            else if (activeObTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                activeObTimer[i] += Time.deltaTime;
                // マテリアルの取得
                Material[] activeMaterials;
                int index = 0;
                if (activeOb.GetComponent<ChestManger>() == null)
                {
                    activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++) activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                    }
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    for (int m = 0; m < activeOb.transform.childCount; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>() != null) index++;
                    }
                    activeMaterials = new Material[index];
                    for (int m = 0; m < activeMaterials.Length; m++)
                    {
                        if (activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material != null) activeMaterials[m] = activeOb.transform.GetChild(m).GetComponent<MeshRenderer>().material;
                    }

                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
            }

            // 4. 終了演出：カメラ移動（注視点 → プレイヤーへ戻る）
            if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
            {
                status = GameStatus.play;
                if (light != null) light.SetActive(false);
                mainCamera.transform.position = cameraPosi; // 元の位置に戻す
                mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                cameraWorkEndFlag[i] = false;
                cameraTimer[i] = 0f;
                if(end) flag = false;
            }
            else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
                Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                mainCamera.transform.position = posi;
                mainCamera.transform.rotation = Quaternion.Euler(rota);
            }
        }
    }

    /// <summary>
    /// ライトを円状に広げながら点灯させる演出ギミック
    /// </summary>
    public void ActiveLight(GameObject lightOb, float time, int i, bool end, ref bool flag)
    {
        // 点灯開始：SE再生と初期輝度の設定
        if (activeLightTimer[i] == 0)
        {
            audioManager.PlayOneShotSE(AudioManager.SEName.gimmickSes, 6);
            lightOb.SetActive(true);
            lightOb.GetComponent<Light>().intensity = 30f;
        }
        // 規定時間に達したら、ライトの角度を最大(180)で固定
        if (activeLightTimer[i] > time)
        {
            activeLightTimer[i] = time;
            lightOb.GetComponent<Light>().spotAngle = 180f;
            lightOb.GetComponent<Light>().innerSpotAngle = 180f;
            if (end) flag = false; // 演出終了フラグ
        }
        // 指定時間かけてスポットライトの角度を0→180へ広げる(Lerp演出)
        else if (activeLightTimer[i] < time)
        {
            activeLightTimer[i] += Time.deltaTime;
            float range = Mathf.Lerp(0f, 180f, activeLightTimer[i] / time);
            lightOb.GetComponent<Light>().spotAngle = range;
            lightOb.GetComponent<Light>().innerSpotAngle = range;
        }
    }

    /// <summary>
    /// 実行時に必要なプレイヤーと各種UI要素を自動で取得・紐付けする
    /// </summary>
    public void GetUIandPlayer()
    {
        GameObject playerSet = GameObject.Find("Player (Set)");
        player = playerSet.transform.GetChild(1).gameObject;
        playerController = playerSet.transform.GetChild(1).GetComponent<PlayerController>();

        // 階層構造から各UI(Play, Start, Menu, Over, Clear)を特定
        playUI = playerSet.transform.GetChild(0).transform.GetChild(0).gameObject;
        startUI = playerSet.transform.GetChild(0).transform.GetChild(1).gameObject;
        menuUI = playerSet.transform.GetChild(0).transform.GetChild(2).gameObject;
        overUI = playerSet.transform.GetChild(0).transform.GetChild(3).gameObject;
        clearUI = playerSet.transform.GetChild(0).transform.GetChild(4).gameObject;

        // テキスト要素の配列化（アニメーション操作用）s
        startText = startUI.transform.GetChild(0).gameObject;
        menuTexts = new GameObject[3];
        menuTexts[0] = menuUI.transform.GetChild(2).gameObject;
        menuTexts[1] = menuUI.transform.GetChild(3).gameObject;
        menuTexts[2] = menuUI.transform.GetChild(4).gameObject;
        overTexts = new GameObject[2];
        overTexts[0] = overUI.transform.GetChild(2).gameObject;
        overTexts[1] = overUI.transform.GetChild(3).gameObject;
        clearTexts = new GameObject[2];
        //clearTexts[0] = clearUI.transform.GetChild(2).gameObject;
        //clearTexts[1] = clearUI.transform.GetChild(3).gameObject;
    }

    /// <summary>
    /// メニュー画面の開閉アニメーションと選択項目のロジック制御
    /// </summary>
    public void MenuUIControl()
    {
        // メニューを開く時のズームイン演出
        if (fadeMenuFlag && menuSelectNum == 0)
        {
            if (fadeMenuTimer > fadeMenuTime)
            {
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                fadeMenuTimer = 0;
                fadeMenuFlag = false;
            }
            else if (fadeMenuTimer < fadeMenuTime)
            {
                fadeMenuTimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeMenuTimer / fadeMenuTime);
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1f);
            }
        }
        else if (fadeMenuFlag && menuSelectNum == 2)
        {
            if (fadeMenuTimer > fadeMenuTime)
            {
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
                fadeMenuTimer = 0;
                fadeMenuFlag = false;

                menuSelectNum = 0;
                playUI.SetActive(true);
                menuUI.SetActive(false);
                menuSelectNum = 0;
                for (int i = 0; i < menuTexts.Length; i++) TextAnime(menuTexts[i], false);
                menuFlag = false;
                audioManager.ResumeAll();
            }
            else if (fadeMenuTimer < fadeMenuTime)
            {
                fadeMenuTimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeMenuTimer / fadeMenuTime);
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            for (int i = 0; i < menuTexts.Length; i++)
            {
                if (menuSelectNum == i) TextAnime(menuTexts[i], true);
                else if (menuSelectNum != i) TextAnime(menuTexts[i], false);
            }
            if (enterFlag)
            {
                if (fadeFlag)
                {
                    if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
                    fadeManager.FadeControl();
                }
                else
                {
                    if (menuSelectNum == 0)
                    {
                        int f = f = stageNum / 4;
                        int s = 0;
                        if (stageNum % 4 == 0)
                        {
                            f--;
                            s = 4;
                        }
                        else s = stageNum - f * 4;
                        SceneManager.LoadScene($"{f + 1}-{s}");
                    }
                    else if (menuSelectNum == 1)
                    {
                        if (GameObject.Find("DataManager") != null)
                        {
                            DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                            int dataNum = dataManager.useDataNum;
                            dataManager.data[dataNum].selectStageNum = 0;
                        }
                        SceneManager.LoadScene("StageSelect");
                    }
                    else if (menuSelectNum == 2)
                    {
                        fadeMenuFlag = true;
                    }
                    enterFlag = false;
                }
            }
        }
    }

    /// <summary>
    /// ゲームオーバー時の選択操作とシーン遷移
    /// </summary>
    public void OverUIControl()
    {
        if (overSelectNum == 0)
        {
            TextAnime(overTexts[0], true);
            TextAnime(overTexts[1], false);
        }
        else if (overSelectNum == 1)
        {
            TextAnime(overTexts[0], false);
            TextAnime(overTexts[1], true);
        }
        // フェードアウト後のシーンロード
        if (fadeFlag)
        {
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag)
            {
                fadeManager.fadeIntervalFlag = false;
                fadeManager.endFlag = false;
                fadeFlag = false;
            }
            fadeManager.FadeControl();
            if (!fadeFlag)
            {
                int fieldNum = stageNum / 4;
                int number = (stageNum % 4 == 0) ? 4 : stageNum % 4;
                if (stageNum % 4 == 0) fieldNum--;
                if (overSelectNum == 0) SceneManager.LoadScene($"{fieldNum + 1}-{number}");
                else if (overSelectNum == 1) SceneManager.LoadScene("StageSelect");
            }
        }
        // 決定ボタン待機
        if (enterFlag)
        {
            fadeManager.fadeInFlag = true;
            fadeFlag = true;
            enterFlag = false;            
        }
    }

    /// <summary>
    /// 選択中のテキストサイズを変更し、プレイヤーに視覚的なフィードバックを与える
    /// </summary>
    public void TextAnime(GameObject textOb, bool flag)
    {
        TextMeshProUGUI text = textOb.GetComponent<TextMeshProUGUI>();
        // 元のサイズ
        if (!flag) text.fontSize = 100f;
        // 拡大
        else text.fontSize = 120f; // 選択中は20%拡大
    }

    // --- 入力コールバック群 ---

    // ゲームオーバー関数
    public void OverUIControl(InputAction.CallbackContext context)
    {
        if (overFlag && !fadeFlag)
        {
            if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                overSelectNum--;
                if (overSelectNum < 0)
                {
                    overSelectNum = 0;
                }
            }
            else if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                overSelectNum++;
                if (overSelectNum > 1)
                {
                    overSelectNum = 1;
                }
            }
        }
    }

    // メニューボタンが押された時
    public void InputMenuButton(InputAction.CallbackContext context)
    {
        if (context.started && !menuFlag && status == GameStatus.play)
        {
            // ポーズ処理・UI表示
            menuFlag = true;
            fadeMenuFlag = true;
            playUI.SetActive(false);
            menuUI.SetActive(true);
            menuUI.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
            audioManager.PauseAll();
        }
        else if (context.started && menuFlag && status == GameStatus.menu)
        {
            // メニューを閉じて再開
            menuSelectNum = 2;
            enterFlag = true;
            audioManager.ResumeAll();
        }
    }

    // 決定ボタン（Enter/Space等）が押された時
    public void InputEnterButton(InputAction.CallbackContext context)
    {
        if (menuFlag && context.started && !enterFlag && !fadeMenuFlag)
        {
            enterFlag = true;
            // フェードが必要な遷移（リトライ等）ならフェード開始
            if (menuSelectNum != 2)
            {
                fadeManager.fadeInFlag = true;
                fadeFlag = true;
            }
        }
        else if (overFlag) enterFlag = true;
    }

    // スティック/方向キーによる項目選択
    public void InputSelectControl(InputAction.CallbackContext context)
    {
        if (menuFlag && !fadeFlag)
        {
            if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                menuSelectNum--;
                if (menuSelectNum < 0)
                {
                    menuSelectNum = 0;
                }
            }
            else if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                menuSelectNum++;
                if (menuSelectNum > 2)
                {
                    menuSelectNum = 2;
                }
            }
        }
    }
}