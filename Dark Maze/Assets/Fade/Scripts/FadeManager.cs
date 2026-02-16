using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// シーン遷移時のフェード演出（イン・インターバル・アウト）を管理するクラス
/// </summary>
public class FadeManager : MonoBehaviour
{
    [Header("フェイドUIの取得")]
    [SerializeField] GameObject[] fadeObjects;      // スライドさせる複数のフェード用パネル
    [SerializeField] List<Sprite> fadeBannerSpriteList; // ロード中に表示する画像リスト
    [SerializeField] int fadeBannerSpriteNum;      // 現在表示中のバナー番号
    [SerializeField] float fadeObjectWidth;        // フェードオブジェクトの横幅
    public bool fadeFlag;                          // いずれかのフェードが動作中か

    [Header("フェイドイン関連 (画面が隠れる動き)")]
    [SerializeField] float fadeInSecond;           // 全体が隠れるまでの秒数
    float fadeInTimer;                             // オブジェクト1つあたりの移動時間
    public bool fadeInFlag;

    [Header("フェイドアウト関連 (画面が見える動き)")]
    [SerializeField] float fadeOutSecond;          // 全体がハケるまでの秒数
    float fadeOutTimer;
    public bool fadeOutFlag;

    [Header("フェイド中間関連 (ロード中演出)")]
    [SerializeField, Range(2f, 10f)] float fadeIntervalSecond; // 最低保持時間
    [SerializeField] float fadeIntervalTimer;
    public bool fadeIntervalFlag;
    [SerializeField] Image fadeBannerObject;       // バナーを表示するUI要素
    [SerializeField] Color fadeBannerColor;        // バナーの透過度管理用
    [SerializeField] float fadeBannerFadeTimer;
    [SerializeField] float fadeBannerFadeInSecond; // バナーがふわっと出る時間
    public bool fadeBannerFadeInFlag;
    [SerializeField] float fadeBannerFadeOutSecond;// バナーがふわっと消える時間
    public bool fadeBannerFadeOutFlag;

    [Header("フェイド共通関連")]
    bool startFlag;                                // 各フェード開始時のSE再生用
    public bool endFlag;                           // 各フェード工程の完了通知
    float[] timer;                                 // 各パネル個別のタイマー
    float[] moveX;                                 // 各パネルの現在のX座標

    public bool titleFlag;
    public bool finishFlag;                        // アプリ終了処理中か

    public bool saveDirection;                     // セーブ中テキストを表示するか
    [SerializeField] GameObject saveText;          // 「セーブ中」UI

    // Input System
    public InputActionAsset inputActions;
    InputAction enterAction;
    void Start()
    {
        // シーンを跨いでも破棄しない
        DontDestroyOnLoad(this.gameObject);

        // 初期設定
        fadeObjectWidth = fadeObjects[0].GetComponent<RectTransform>().sizeDelta.x;
        fadeBannerObject.enabled = false;

        timer = new float[fadeObjects.Length];
        moveX = new float[fadeObjects.Length];

        // パネルごとの時間配分を計算
        fadeInTimer = fadeInSecond / fadeObjects.Length;
        fadeOutTimer = fadeInSecond / fadeObjects.Length;

        // 開始時間をずらすことで時間差（ディレイ）を作る
        for (int i = 0; i < fadeObjects.Length; i++) timer[i] -= i * fadeInTimer;

        // 入力アクションの取得
        enterAction = inputActions.FindActionMap("Player")["Enter"];
    }

    void Update()
    {
        // 動作状況の集約
        if (fadeInFlag || fadeIntervalFlag || fadeOutFlag) fadeFlag = true;
        else if (!fadeInFlag && !fadeIntervalFlag && !fadeOutFlag) fadeFlag = false;

        // 各フェード状態に応じたメソッドの呼び出し
        if (fadeInFlag)
        {
            fadeOutFlag = false;
            fadeIntervalFlag = false;
            InFade();
        }
        else if (fadeIntervalFlag)
        {
            fadeInFlag = false;
            fadeOutFlag = false;
            IntervalFade();

            // 特定のステージ名の場合にセーブ中演出を表示
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (SceneManager.GetActiveScene().name == $"{i}-{j}") saveDirection = true;
                    if (saveDirection) break;
                }
            }
            if (SceneManager.GetActiveScene().name == $"StageSelect") saveDirection = true;
            if (saveDirection)
            {
                saveText.SetActive(true);
            }
            else saveText.SetActive(false);
        }
        else if (fadeOutFlag)
        {
            saveText.SetActive(false);
            fadeInFlag = false;
            fadeIntervalFlag = false;
            OutFade();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            finishFlag = true;
            fadeInFlag = true;
            DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
            if (dataManager != null)
            {
                int i  = dataManager.useDataNum;
                dataManager.SaveData(i, dataManager.data[i].playerName, dataManager.data[i].clearStageNum, dataManager.data[i].selectStageNum);
            }
        }
        if (finishFlag && endFlag)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        }

        if (enterAction.triggered)
        {
            if (fadeFlag)
            {
                fadeBannerSpriteNum++;
                if (fadeBannerSpriteNum == fadeBannerSpriteList.Count) fadeBannerSpriteNum = 0;
                ChangeBanner();
            }
        }

    }

    /// <summary>
    /// 画面が隠れる演出（左からパネルが流れてくる）
    /// </summary>
    void InFade()
    {
        if (!startFlag)
        {
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 0);
            startFlag = true;
        }
        for (int i = 0; i < fadeObjects.Length; i++)
        {
            // 目標地点（X=0）に到達したか確認
            if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x == 0)
            {
                timer[i] = 0;
                moveX[i] = 0;
                if (i == fadeObjects.Length - 1)　// 最後のパネルが終わったら
                {
                    for(int j = 0; j < fadeObjects.Length; j++)
                    {
                        timer[j] -= j * fadeOutTimer;
                    }
                    //fadeInFlag = false;
                    startFlag = false;
                    endFlag = true;
                }
            }
            else if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x != 0)
            {
                timer[i] += Time.deltaTime;
                // Lerpを使って滑らかにスライド
                moveX[i] = Mathf.Lerp(-fadeObjectWidth - i * 300f, 0, timer[i] / fadeInTimer);
            }
            fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(moveX[i], fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    /// <summary>
    /// 画面が再び見える演出（パネルが左へハケる）
    /// </summary>
    void OutFade()
    {
        if (!startFlag)
        {
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 0);
            startFlag = true;
        }
        for (int i = 0;i < fadeObjects.Length;i++)
        {
            if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x == -fadeObjectWidth - i * 300f)
            {
                timer[i] = 0;
                moveX[i] = -fadeObjectWidth - i * 300f;
                if (i == fadeObjects.Length - 1)
                {
                    for (int j = 0; j < fadeObjects.Length; j++)
                    {
                        timer[j] -= j * fadeInTimer;
                    }
                    //fadeOutFlag = false;
                    startFlag = false;
                    endFlag = true;
                }
            }
            else if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x != -fadeObjectWidth - i * 300f)
            {
                timer[i] += Time.deltaTime;
                moveX[i] = Mathf.Lerp(0, -fadeObjectWidth - i * 300f, timer[i] / fadeOutTimer);
            }
            fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(moveX[i], fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    /// <summary>
    /// フェード中間の待機時間（バナーのフェードイン・アウト処理）
    /// </summary>
    void IntervalFade()
    {
        // 開始時
        if (fadeIntervalTimer == 0)
        {
            fadeBannerFadeInFlag = true;
            fadeBannerObject.enabled = true;
            // フェイドバナーを交換する
            ChangeBanner();
        }
        // 終了間際になったらバナーをフェードアウトさせ始める
        if (fadeIntervalTimer > fadeIntervalSecond - fadeBannerFadeOutSecond && !fadeBannerFadeOutFlag) fadeBannerFadeOutFlag = true;
        fadeIntervalTimer += Time.deltaTime;

        // バナーのアルファ値制御（イン）
        if (fadeBannerFadeInFlag)
        {
            fadeBannerFadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, fadeBannerFadeTimer / fadeBannerFadeInSecond);
            fadeBannerColor.a = a;
            fadeBannerObject.color = fadeBannerColor;
            if (fadeBannerFadeTimer > fadeBannerFadeInSecond)
            {
                fadeBannerColor.a = 1f;
                fadeBannerObject.color = fadeBannerColor; 
                fadeBannerFadeTimer = 0;
                fadeBannerFadeInFlag = false;
            }
        }
        // バナーのアルファ値制御（アウト）
        else if (fadeBannerFadeOutFlag)
        {
            fadeBannerFadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeBannerFadeTimer / fadeBannerFadeOutSecond);
            fadeBannerColor.a = a;
            fadeBannerObject.color = fadeBannerColor;
            if (fadeBannerFadeTimer > fadeBannerFadeOutSecond)
            {
                fadeBannerColor.a = 0;
                fadeBannerObject.color = fadeBannerColor; 
                fadeBannerFadeTimer = 0;
                fadeBannerFadeOutFlag = false;
            }
        }

        // 全工程終了
        if (fadeIntervalTimer > fadeIntervalSecond && !fadeBannerFadeOutFlag)
        {
            fadeBannerObject.enabled = false;
            fadeIntervalTimer = 0;
            endFlag = true;
            fadeBannerSpriteNum++;
            if (fadeBannerSpriteNum == fadeBannerSpriteList.Count) fadeBannerSpriteNum = 0;
        }
    }

    void ChangeBanner()
    {
        fadeBannerObject.sprite = fadeBannerSpriteList[fadeBannerSpriteNum];
    }
    // 外部から一瞬で状態を切り替える用
    public void AfterFade()
    {
        for (int i = 0; i < fadeObjects.Length; i++) fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
    }
    public void BeforeFade()
    {
        for (int i = 0; i < fadeObjects.Length; i++) fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-fadeObjectWidth - i * 300f, fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
    }

    /// <summary>
    /// シーン遷移スクリプトなどから呼ばれ、フェードの工程を次に進める
    /// </summary>
    public void FadeControl()
    {
        // フェイドイン終了時
        if (endFlag && fadeInFlag)
        {
            fadeInFlag = false;
            endFlag = false;
            fadeIntervalFlag = true;
        }
        // フェイドインターバル終了時
        else if (endFlag && fadeIntervalFlag)
        {
            fadeIntervalFlag = false;
            endFlag = false;
            fadeOutFlag = true;
        }
        // フェイドアウト終了時
        else if (endFlag && fadeOutFlag)
        {
            fadeOutFlag = false;
            endFlag = false;
        }
    }

}


