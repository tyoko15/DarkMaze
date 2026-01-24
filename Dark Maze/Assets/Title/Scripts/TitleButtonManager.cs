using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


/// <summary>
/// タイトル画面のボタン操作を管理するクラス
/// ・コントローラー入力によるUI選択
/// ・タイトル / データ選択 / データ作成の分岐制御
/// ・決定UIの表示状態管理
/// </summary>
public class TitleButtonManager : MonoBehaviour
{
    // ==============================
    // 他スクリプト参照
    // ==============================
    [Header("スクリプトの取得")]
    [SerializeField] TitleManager titleManager;   // タイトル画面の進行状態を管理
    [SerializeField] DataManager dataManager;     // セーブデータ管理
    [SerializeField] FadeManager fadeManager;     // フェード演出管理

    // ==============================
    // UIオブジェクト参照
    // ==============================
    [Header("Buttonの取得")]
    [SerializeField] GameObject[] titleUIButtons;                 // タイトル画面のボタン群
    [SerializeField] GameObject[] selectDataUIButtons;            // データ選択UIのボタン群
    [SerializeField] GameObject selectDataDecisionUI;             // データ選択決定UI
    [SerializeField] GameObject[] selectDataDecisionUIButtons;    // データ選択決定UI内ボタン
    [SerializeField] GameObject[] createDataUIButtons;            // データ作成UIのボタン群
    [SerializeField] GameObject createDataDecisionUI;             // データ作成決定UI
    [SerializeField] GameObject[] createDataDecisionUIButtons;    // データ作成決定UI内ボタン


    // ==============================
    // アニメーション制御用変数
    // ==============================
    [Header("Anime情報")]
    int animeFlag;                         // 再生するアニメーションの種類
    bool decisionFlag;                     // 決定UIが表示中かどうか
    [SerializeField] float fadeInDecisionUITime;
    [SerializeField] float fadeInDecisionUITimer;
    [SerializeField] float fadeOutDecisionUITime;
    [SerializeField] float fadeOutDecisionUITimer;

    // ==============================
    // 入力制御関連
    // ==============================
    int inputDirectionNum;                 // 入力方向（上下左右）
    bool inputIntervalFlag;                // 入力受付制限フラグ
    [SerializeField] float inputIntervalTime;
    float inputIntervalTimer;

    [Header("コントローラー情報")]
    [SerializeField] int selectNum;        // 現在選択中の番号
    int oldSelectNum;                      // 1つ前の選択番号

    bool oneFlag;                          // 選択変更時に一度だけ処理するためのフラグ
    bool EnterFlag;                        // 決定入力が行われたかどうか

    // ==============================
    // 初期化処理
    // ==============================
    void Start()
    {
        var controllers = Input.GetJoystickNames();
        if (controllers != null)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ControllerSelect();
        }
        else if (controllers == null)
        {
            Cursor.visible = true;
        }
        fadeManager = GameObject.Find("FadeManager").GetComponent<FadeManager>();
    }

    // ==============================
    // 毎フレーム処理
    // ==============================
    void Update()
    {
        // コントローラー接続状態によるカーソル制御
        if (dataManager == null) dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        var controllers = Input.GetJoystickNames();
        if (controllers != null)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ControllerSelect();
        }
        else if (controllers == null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // 入力による選択番号更新
        InputSelectControl();

        // 決定UIのフェードアニメーション制御
        if (animeFlag == 1) SelectDataDecisionUIAnime(selectDataDecisionUI, true);
        else if(animeFlag == 2) SelectDataDecisionUIAnime(selectDataDecisionUI, false);
        else if(animeFlag == 3) CreateDataDecisionUIAnime(createDataDecisionUI, true);
        else if(animeFlag == 4) CreateDataDecisionUIAnime(createDataDecisionUI, false);

    }

    // ==============================
    // コントローラー操作による選択制御
    // ==============================
    void ControllerSelect()
    {
        // progressNum によって現在のUI状態を判定
        // 0 : タイトル画面
        // 1 : データ選択画面
        // 2 : データ作成画面

        if (titleManager.progressNum == 0)
        {
            // ---------- タイトル画面 ----------
            if (oneFlag)
            {
                if (selectNum == 0)
                {
                    EnterStartButton();
                    if (oldSelectNum == 1) ExitEndButton();
                }
                if (selectNum == 1)
                {
                    EnterEndButton();
                    if (oldSelectNum == 0) ExitStartButton();
                }
                oldSelectNum = selectNum;
                oneFlag = false;
            }

            // 決定入力処理
            if (EnterFlag)
            {
                if (selectNum == 0) ClickStartButton();
                else if (selectNum == 1) ClickEndButton();

                // 選択状態リセット
                selectNum = 0;
                oldSelectNum = 0;
                EnterFlag = false;
                oneFlag = true;
            }
        }
        else if (titleManager.progressNum == 1)
        {
            // ---------- データ選択画面 ----------
            if (!decisionFlag)
            {
                // 通常選択中
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterData1();
                        if (oldSelectNum == 1) ExitData2();
                        else if (oldSelectNum == 3) ExitSelectDataUIReturnButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterData2();
                        if (oldSelectNum == 0) ExitData1();
                        else if (oldSelectNum == 2) ExitData3();
                    }
                    else if (selectNum == 2)
                    {
                        EnterData3();
                        if (oldSelectNum == 1) ExitData2();
                        else if (oldSelectNum == 3) ExitSelectDataUIReturnButton();
                    }
                    else if (selectNum == 3)
                    {
                        EnterSelectDataUIReturnButton();
                        if (oldSelectNum == 0) ExitData1();
                        else if (oldSelectNum == 2) ExitData3();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }

                // 決定入力
                if (EnterFlag)
                {
                    if (selectNum == 0) ClickData1();
                    else if (selectNum == 1) ClickData2();
                    else if (selectNum == 2) ClickData3();
                    else if (selectNum == 3) ClickSelectDataUIReturnButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
            else
            {
                // 決定確認UI表示中
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterSelectDataReturnButton();
                        if (oldSelectNum == 1) ExitSelectDataDecisionButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterSelectDataDecisionButton();
                        if (oldSelectNum == 0) ExitSelectDataReturnButton();
                    }                    
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }
                if (EnterFlag)
                {
                    if (selectNum == 0) ClickSelectDataReturnButton();
                    else if (selectNum == 1) ClickSelectDataDecisionButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
        }
        else if(titleManager.progressNum == 2)
        {
            // ---------- データ作成画面 ----------
            if (decisionFlag)
            {
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterCreateDataDecisionUIReturnButton();
                        if (oldSelectNum == 1) ExitCreateDataDecisionUIDecisionButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterCreateDataDecisionUIDecisionButton();
                        if (oldSelectNum == 0) ExitCreateDataDecisionUIReturnButton();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }
                if (EnterFlag)
                {
                    if (selectNum == 0) ClickCreateDataDecisionUIReturnButton();
                    else if (selectNum == 1) ClickCreateDataDecisionUIDecisionButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }            
        }
    }

    // ==============================
    // TitleUI 関連
    // ==============================

    /// <summary>
    /// タイトルUIのテキストサイズを変更する演出
    /// 選択中は大きく、非選択時は元のサイズに戻す
    /// </summary>
    void TextAnime(ref GameObject[] buttons, int i, bool flag)
    {
        if(flag) buttons[i].GetComponent<TextMeshProUGUI>().fontSize = 120f;
        if(!flag) buttons[i].GetComponent<TextMeshProUGUI>().fontSize = 100f;
    }

    /// <summary>
    /// ボタンの拡大・縮小による選択演出
    /// </summary>
    void ButtonAnime(ref GameObject[] buttons, int i, bool flag)
    {

        if (flag) buttons[i].GetComponent<RectTransform>().localScale = new Vector2(1.1f, 1.1f);
        if(!flag) buttons[i].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);

    }

    /// <summary>
    /// 「START」ボタンにカーソルが乗ったときの処理
    /// </summary>
    public void EnterStartButton()
    {
        titleManager.enterTitleUINum = 1;
        titleUIButtons[0].SetActive(true);
        titleUIButtons[1].SetActive(false);
        //TextAnime(ref titleUIButtons, 0, true);
    }

    /// <summary>
    /// 「END」ボタンにカーソルが乗ったときの処理
    /// </summary>
    public void EnterEndButton()
    {
        titleManager.enterTitleUINum = 2;
        titleUIButtons[0].SetActive(false);
        titleUIButtons[1].SetActive(true);
        //TextAnime(ref titleUIButtons, 1, true);
    }

    /// <summary>
    /// 「START」ボタン決定時の処理
    /// データ選択画面へフェード遷移
    /// </summary>
    public void ClickStartButton()
    {
        //titleManager.TitleUIActive(false);
        //titleManager.SelectDataUIActive(true);
        //TextAnime(ref titleUIButtons, 0, false);
        titleManager.fadeFlag = true;
        titleManager.progressNum = 1;
    }

    /// <summary>
    /// 「END」ボタン決定時の処理
    /// ゲーム終了用フェード開始
    /// </summary>
    public void ClickEndButton()
    {
        fadeManager.finishFlag = true;
        fadeManager.fadeInFlag = true;
    }

    /// <summary>
    /// 「START」ボタンからカーソルが外れたときの処理
    /// </summary>
    public void ExitStartButton()
    {
        //TextAnime(ref titleUIButtons, 0, false);
    }

    /// <summary>
    /// 「END」ボタンからカーソルが外れたときの処理
    /// </summary>
    public void ExitEndButton()
    {
        //TextAnime(ref titleUIButtons, 1, false);
    }


    // ==============================
    // SelectDataUI 関連
    // ==============================

    /// <summary>
    /// データ選択の決定確認UIの拡大・縮小アニメーション
    /// flag=true  : フェードイン
    /// flag=false : フェードアウト
    /// </summary>
    void SelectDataDecisionUIAnime(GameObject UI, bool flag)
    {
        if (flag)
        {
            // フェードイン開始時
            if (fadeInDecisionUITimer == 0)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                titleManager.SelectDataDecisionUIActive(true);            
            }

            // フェードイン完了
            if (fadeInDecisionUITimer > fadeInDecisionUITime)
            {
                UI.transform.localScale = new Vector3(1f, 1f, 1f);
                fadeInDecisionUITimer = 0f;
                animeFlag = 0;
                decisionFlag = true;
            }
            else if (fadeInDecisionUITimer < fadeInDecisionUITime)
            {
                // 拡大アニメーション中
                fadeInDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeInDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        else
        {
            // フェードアウト開始時
            if (fadeOutDecisionUITimer == 0) UI.transform.localScale = new Vector3(1f, 1f, 1f);

            // フェードアウト完了
            if (fadeOutDecisionUITimer > fadeOutDecisionUITime)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                fadeOutDecisionUITimer = 0f;
                titleManager.SelectDataDecisionUIActive(false);
                animeFlag = 0;
            }
            else if (fadeOutDecisionUITimer < fadeInDecisionUITime)
            {
                // 縮小アニメーション中
                fadeOutDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeOutDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    /// <summary>
    /// データ選択画面の「戻る」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterSelectDataUIReturnButton()
    {
        ButtonAnime(ref selectDataUIButtons, 3, true);
    }

    /// <summary>
    /// データ選択画面の「戻る」ボタン決定時
    /// タイトル画面へ戻る
    /// </summary>
    public void ClickSelectDataUIReturnButton()
    {
        titleManager.fadeFlag = true;
        titleManager.progressNum = 0;
    }

    /// <summary>
    /// データ選択画面の「戻る」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitSelectDataUIReturnButton()
    {
        ButtonAnime(ref selectDataUIButtons, 3, false);
    }

    /// <summary>
    /// セーブデータ1にカーソルが乗ったとき
    /// </summary>
    public void EnterData1()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 0, true);
    }

    /// <summary>
    /// セーブデータ2にカーソルが乗ったとき
    /// </summary>
    public void EnterData2()
    {
        titleManager.enterSelectDataNum = 1;
        ButtonAnime(ref selectDataUIButtons, 1, true);
    }

    /// <summary>
    /// セーブデータ3にカーソルが乗ったとき
    /// </summary>
    public void EnterData3()
    {
        titleManager.enterSelectDataNum = 2;
        ButtonAnime(ref selectDataUIButtons, 2, true);
    }

    /// <summary>
    /// セーブデータ1決定時の処理
    /// </summary>
    public void ClickData1()
    {
        titleManager.selectDataNum = 0;
        animeFlag = 1;
        if (titleManager.dataNumFlag[0]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 0, false);
    }

    /// <summary>
    /// セーブデータ2決定時の処理
    /// </summary>
    public void ClickData2()
    {
        titleManager.selectDataNum = 1;
        animeFlag = 1;
        if (titleManager.dataNumFlag[1]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 1, false);
    }

    /// <summary>
    /// セーブデータ3決定時の処理
    /// </summary>
    public void ClickData3()
    {
        titleManager.selectDataNum = 2;
        animeFlag = 1;
        if (titleManager.dataNumFlag[2]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 2, false);
    }

    /// <summary>
    /// セーブデータ1からカーソルが外れたとき
    /// </summary>
    public void ExitData1()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 0, false);
    }

    /// <summary>
    /// セーブデータ2からカーソルが外れたとき
    /// </summary>
    public void ExitData2()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 1, false);
    }

    /// <summary>
    /// セーブデータ3からカーソルが外れたとき
    /// </summary>
    public void ExitData3()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 2, false);
    }

    // ==============================
    // SelectDataDecisionUI（データ決定確認UI）
    // ==============================

    /// <summary>
    /// データ決定UIの「決定」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterSelectDataDecisionButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 0, true);
    }

    /// <summary>
    /// データ決定UIの「戻る」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterSelectDataReturnButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 1, true);
    }

    /// <summary>
    /// データ決定UIで「決定」を選択したときの処理
    /// ・新規データ → データ作成画面
    /// ・既存データ → ステージ選択へ
    /// </summary>
    public void ClickSelectDataDecisionButton()
    {
        titleManager.SelectDataDecisionUIActive(false);

        if (titleManager.dataNumFlag[titleManager.selectDataNum])
        {
            // 新規データ作成へ
            TextAnime(ref selectDataDecisionUIButtons, 0, false);
            titleManager.progressNum = 2;
            titleManager.fadeFlag = true;
        }
        else
        {
            // 既存データでゲーム開始
            TextAnime(ref selectDataDecisionUIButtons, 0, false);
            titleManager.progressNum = 3;
            titleManager.fadeFlag = true;     // ステージ選択
        }

        // 使用するセーブデータ番号を記録
        dataManager.GetuseDataNum(titleManager.selectDataNum);
        decisionFlag = false;
    }

    /// <summary>
    /// データ決定UIで「戻る」を選択したときの処理
    /// </summary>
    public void ClickSelectDataReturnButton()
    {
        animeFlag = 2;

        // データ選択ボタンの入力を再有効化
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = true;
        decisionFlag = false;
    }

    /// <summary>
    /// データ決定UIの「決定」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitSelectDataDecisionButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 0, false);
    }

    /// <summary>
    /// データ決定UIの「戻る」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitSelectDataReturnButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 1, false);
    }

    // ==============================
    // CreateDataUI（データ作成画面）
    // ==============================

    /// <summary>
    /// データ作成決定UIの拡大・縮小アニメーション
    /// </summary>
    void CreateDataDecisionUIAnime(GameObject UI, bool flag)
    {
        if (flag)
        {
            // フェードイン開始
            if (fadeInDecisionUITimer == 0)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                titleManager.CreateDataDecisionUIActive(true);
            }

            // フェードイン完了
            if (fadeInDecisionUITimer > fadeInDecisionUITime)
            {
                UI.transform.localScale = new Vector3(1f, 1f, 1f);
                fadeInDecisionUITimer = 0f;
                animeFlag = 0;
                decisionFlag = true;
            }
            else if (fadeInDecisionUITimer < fadeInDecisionUITime)
            {
                // 拡大アニメーション中
                fadeInDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeInDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        else
        {
            // フェードアウト開始
            if (fadeOutDecisionUITimer == 0) UI.transform.localScale = new Vector3(1f, 1f, 1f);

            // フェードアウト完了
            if (fadeOutDecisionUITimer > fadeOutDecisionUITime)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                fadeOutDecisionUITimer = 0f;
                titleManager.CreateDataDecisionUIActive(false);
                animeFlag = 0;
            }
            else if (fadeOutDecisionUITimer < fadeInDecisionUITime)
            {
                // 縮小アニメーション中
                fadeOutDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeOutDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    /// <summary>
    /// データ作成画面の「決定」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterCreateDataDecisionButton()
    {
        TextAnime(ref createDataUIButtons, 0, true);
    }

    /// <summary>
    /// データ作成画面の「戻る」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterCreateDataReturnButton()
    {
        TextAnime(ref createDataUIButtons, 1, true);
    }

    /// <summary>
    /// データ作成画面で「決定」を選択したとき
    /// 決定確認UIを表示
    /// </summary>
    public void ClickCreateDataDecisionButton()
    {
        animeFlag = 3;
        titleManager.createNameText.text = $"プレイヤー名: {titleManager.nameText.GetComponent<TextMeshProUGUI>().text}";
    }
    public void ClickCreateDataReturnButton()
    {
        titleManager.progressNum = 1;
        titleManager.fadeFlag = true;

        // データ選択UIの入力を再有効化
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = true;
        
        // 入力中の名前をリセット
        titleManager.nameText.GetComponent<TextMeshProUGUI>().text = "";
    }

    /// <summary>
    /// データ作成画面の「決定」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitCreateDataDecisionButton()
    {
        TextAnime(ref createDataUIButtons, 0, false);
    }

    /// <summary>
    /// データ作成画面の「戻る」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitCreateDataReturnButton()
    {
        TextAnime(ref createDataUIButtons, 1, false);
    }

    /// <summary>
    /// データ作成決定UIの「決定」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterCreateDataDecisionUIDecisionButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 0, true);
    }

    /// <summary>
    /// データ作成決定UIの「戻る」ボタンにカーソルが乗ったとき
    /// </summary>
    public void EnterCreateDataDecisionUIReturnButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 1, true);
    }

    /// <summary>
    /// データ作成決定UIで「決定」を選択したとき
    /// セーブデータ作成後、ステージ選択へ
    /// </summary>
    public void ClickCreateDataDecisionUIDecisionButton()
    {
        titleManager.CreateName();
        titleManager.fadeFlag = true;
        titleManager.progressNum = 3;
        decisionFlag = false;
    }

    /// <summary>
    /// データ作成決定UIで「戻る」を選択したとき
    /// </summary>
    public void ClickCreateDataDecisionUIReturnButton()
    {
        animeFlag = 4;
        decisionFlag = false;
    }

    /// <summary>
    /// データ作成決定UIの「決定」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitCreateDataDecisionUIDecisionButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 0, false);
    }

    /// <summary>
    /// データ作成決定UIの「戻る」ボタンからカーソルが外れたとき
    /// </summary>
    public void ExitCreateDataDecisionUIReturnButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 1, false);
    }

    // ==============================
    // 入力制御（データ作成画面・カーソル移動）
    // ==============================

    /// <summary>
    /// データ作成画面での文字選択カーソル入力制御
    /// 入力間隔を設けて、連続入力を防止している
    /// </summary>
    public void InputSelectControl()
    {
        // CreateDataUI のときのみ処理
        if (titleManager.progressNum == 2)
        {
            if (!decisionFlag)
            {
                // 入力インターバル中
                if (inputIntervalFlag)
                {
                    if (inputIntervalTimer > inputIntervalTime)
                    {
                        inputIntervalTimer = 0;
                        inputIntervalFlag = false;
                    }
                    else if (inputIntervalTimer < inputIntervalTime) inputIntervalTimer += Time.deltaTime;
                }
                // 入力受付状態
                else
                {
                    // カーソルが端に到達した場合は入力方向をリセット
                    if ((inputDirectionNum == 1 || inputDirectionNum == 2) && titleManager.inputTextVector.y == -1)
                    {
                        inputDirectionNum = 0;
                        inputIntervalFlag = true;
                    }
                    else if ((inputDirectionNum == 3 || inputDirectionNum == 4) && (titleManager.inputTextVector.x == -1 || titleManager.inputTextVector.x == 13)) 
                    {
                        inputDirectionNum = 0;
                        inputIntervalFlag = true;
                    }

                    // 右入力
                    if (inputDirectionNum == 1)
                    {
                        titleManager.inputTextVector.x++;
                        if (titleManager.inputTextVector.x > 13) titleManager.inputTextVector.x = 13;
                        inputIntervalFlag = true;
                    }
                    // 左入力
                    else if (inputDirectionNum == 2)
                    {
                        titleManager.inputTextVector.x--;
                        if (titleManager.inputTextVector.x < -1) titleManager.inputTextVector.x = -1;
                        inputIntervalFlag = true;
                    }
                    // 上入力
                    if (inputDirectionNum == 3)
                    {
                        titleManager.inputTextVector.y++;
                        if (titleManager.inputTextVector.y > 4) titleManager.inputTextVector.y = 4;
                        inputIntervalFlag = true;
                    }
                    // 下入力
                    else if (inputDirectionNum == 4)
                    {
                        titleManager.inputTextVector.y--;
                        if (titleManager.inputTextVector.y < -1) titleManager.inputTextVector.y = -1;
                        inputIntervalFlag = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 方向入力によるUI選択番号の変更
    /// 各 progressNum ごとに操作対象が変わる
    /// </summary>
    public void InputSelectNum(InputAction.CallbackContext context)
    {
        // ==============================
        // TitleUI
        // ==============================
        if (titleManager.progressNum == 0)
        {
            if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                selectNum++;
                if (selectNum > 1) selectNum = 0;
                oneFlag = true;
            }
            else if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                selectNum--;
                if (selectNum < 0) selectNum = 1;
                oneFlag = true;
            }
        }
        // ==============================
        // SelectDataUI
        // ==============================
        else if (titleManager.progressNum == 1)
        {
            // 通常選択中
            if (!decisionFlag)
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 3) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 3;
                    oneFlag = true;
                }
            }
            // 決定確認UI中
            else
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 1) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 1;
                    oneFlag = true;
                }
            }
        }
        // ==============================
        // CreateDataUI
        // ==============================
        else if (titleManager.progressNum == 2)
        {
            // 名前入力中
            if (!decisionFlag)
            {
                // 方向入力を数値として保存
                if (context.ReadValue<Vector2>().x > 0) inputDirectionNum = 1;
                else if (context.ReadValue<Vector2>().x < 0) inputDirectionNum = 2;
                if (context.ReadValue<Vector2>().y < 0) inputDirectionNum = 3;
                else if (context.ReadValue<Vector2>().y > 0) inputDirectionNum = 4;

                // 入力解除
                if (context.canceled) inputDirectionNum = 0;               
            }
            // 決定確認UI中
            else
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 1) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 1;
                    oneFlag = true;
                }
            }
        }
    }

    /// <summary>
    /// 決定（Enter）入力処理
    /// </summary>
    public void InputEnter(InputAction.CallbackContext context)
    {
        // タイトル画面・データ選択画面
        if (titleManager.progressNum == 0 || titleManager.progressNum == 1)
        {
            if (context.started) EnterFlag = true;
        }
        // データ作成画面
        else if (titleManager.progressNum == 2)
        {
            // 名前入力中
            if (context.started && !decisionFlag) titleManager.enterInputFlag = true;
            // 決定確認UI中
            if (context.started && decisionFlag) EnterFlag = true;
        }
    }
}
