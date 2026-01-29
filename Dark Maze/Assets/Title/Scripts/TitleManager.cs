using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトル画面全体の進行管理を行うクラス
/// ・タイトルUI / セーブデータ選択UI / データ作成UI の切り替え
/// ・フェード演出の制御
/// ・プレイヤー名入力（疑似キーボード）の制御
/// </summary>
public class TitleManager : MonoBehaviour
{
    // ===== 他スクリプト参照 =====
    [SerializeField] TitleButtonManager titleButtonManager; // タイトル画面のボタン制御
    [SerializeField] DataManager dataManager;               // セーブデータ管理
    [SerializeField] FadeManager fadeManager;               // フェード演出制御
    [SerializeField] GameObject fadeManagerObject;          // FadeManager生成用Prefab
    [SerializeField] GameObject dataManagerObject;          // DataManager生成用Prefab

    // ===== UI参照 =====
    [Header("UIの取得")]
    [SerializeField] GameObject titleUIObject;               // タイトルUI
    [SerializeField] GameObject selectDataUIObject;          // データ選択UI
    [SerializeField] TextMeshProUGUI[] selectDataText;       // 各データ表示テキスト
    [SerializeField] GameObject selectDataDecisionUIObject;  // データ決定UI
    [SerializeField] public TextMeshProUGUI selectDataDecisionText;

    [SerializeField] GameObject createDataUIObject;          // データ作成UI
    [SerializeField] public TMP_InputField nameInputField;
    [SerializeField] public TextMeshProUGUI createNameText;
    [SerializeField] GameObject createDataDecisionUIObject;

    // ===== 名前入力関連 =====
    int inputProgressNum;                                    // 入力状態の進行管理
    [SerializeField] public GameObject nameText;             // 入力中の名前表示
    [SerializeField] GameObject inputGroup;                  // 入力文字全体の親

    TextMeshProUGUI[,] inputTexts;                            // 文字表示用配列
    GameObject[,] inputTextObjects;                           // 文字オブジェクト配列

    [SerializeField] GameObject inputDeleteObject;            // Deleteボタン
    [SerializeField] GameObject selectInputTextObject;        // 選択中カーソル

    [SerializeField] string[] alphanumericSmallTexts;         // 小文字英数字
    [SerializeField] string[] alphanumericLargeTexts;         // 大文字英数字

    bool changeSizeFlag = true;                               // 大文字 / 小文字切り替え
    public Vector2 inputTextVector;                           // 選択中の文字位置
    public bool enterInputFlag;                               // 決定入力フラグ

    // ===== データ選択関連 =====
    public int enterTitleUINum;
    public int enterSelectDataNum;
    public int selectDataNum;
    public bool[] dataNumFlag;                                // データ有無フラグ

    // ===== 画面進行 =====
    // 0: TitleUI
    // 1: SelectDataUI
    // 2: CreateDataUI
    // 3: Scene遷移
    public int progressNum;
    public bool fadeFlag;                                     // フェード処理中か

    void Start()
    {
        // FadeManagerが存在しない場合は生成する
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
        }
        else
        {
            fadeManager = fade.GetComponent<FadeManager>();
        }
        // DataManagerが存在しない場合は生成する
        GameObject data = GameObject.Find("DataManager");
        if (data == null)
        {
            data = Instantiate(dataManagerObject);
            data.gameObject.name = "DataManager";
            dataManager = data.GetComponent<DataManager>();
        }
        else
        {
            dataManager = data.GetComponent<DataManager>();
        }

        // 初期UI状態
        TitleUIActive(true);
        SelectDataUIActive(false);
        CreateDataUIActive(false);

        // タイトル復帰時のフェード処理
        if (fadeManager.titleFlag)
        {
            fadeManager.AfterFade();
            fadeFlag = true;
            fadeManager.fadeOutFlag = true;
        }

        // 名前入力UIの初期化
        GetInputText();
        SetCharactersInputText();
    }

    void Update()
    {
        DisplayData();              // セーブデータ表示更新
        SelectInputTextControl();   // 名前入力制御

        if (fadeFlag)
        {
            FadeControl();          // フェード演出制御
        }
    }

    /// <summary>
    /// セーブデータ情報をUIに反映する
    /// </summary>
    void DisplayData()
    {
        for (int i = 0; i < 3; i++)
        {
            if (dataManager.data[i].playerName == "")
            {
                selectDataText[i].text = "データなし";
                dataNumFlag[i] = true;
            }
            else
            {
                selectDataText[i].text =
                    $"プレイヤー名: {dataManager.data[i].playerName}\n" +
                    $"クリアステージ数: {dataManager.data[i].clearStageNum}\n";
                dataNumFlag[i] = false;
            }
        }
    }

    /// <summary>
    /// フェードの状態遷移とUI切り替えを制御
    /// </summary>
    void FadeControl()
    {
        // フェードイン終了
        if (fadeManager.endFlag && fadeManager.fadeInFlag)
        {
            fadeManager.fadeInFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeIntervalFlag = true;

            // 進行状態に応じたUI切り替え
            if (progressNum == 0)
            {
                TitleUIActive(true);
                SelectDataUIActive(false);
            }
            else if (progressNum == 1)
            {
                TitleUIActive(false);
                SelectDataUIActive(true);
                CreateDataUIActive(false);
            }
            else if (progressNum == 2)
            {
                SelectDataUIActive(false);
                CreateDataUIActive(true);
            }
            else if (progressNum == 3)
            {
                SceneManager.LoadScene("StageSelect");
            }
        }
        // フェードインターバル終了
        else if (fadeManager.endFlag && fadeManager.fadeIntervalFlag)
        {
            fadeManager.fadeIntervalFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeOutFlag = true;
        }
        // フェードアウト終了
        else if (fadeManager.endFlag && fadeManager.fadeOutFlag)
        {
            fadeManager.fadeOutFlag = false;
            fadeManager.endFlag = false;
            fadeFlag = false;
        }
        // フェード開始
        else if (!fadeManager.fadeInFlag && !fadeManager.fadeIntervalFlag && !fadeManager.fadeOutFlag)
        {
            fadeManager.fadeInFlag = true;
        }
    }

    /// <summary>
    /// 入力された名前をセーブデータとして保存
    /// </summary>
    public void CreateName()
    {
        string name = nameText.GetComponent<TextMeshProUGUI>().text;
        dataManager.SaveData(selectDataNum, name, 0, 0);
        nameText.GetComponent<TextMeshProUGUI>().text = "";
    }

    /// <summary>
    /// 名前入力用UIの文字参照を配列に取得
    /// </summary>
    void GetInputText()
    {
        if (inputGroup == null) return;

        int h = inputGroup.transform.GetChild(1).childCount;
        int w = inputGroup.transform.GetChild(1).GetChild(0).childCount;

        inputTexts = new TextMeshProUGUI[h, w];
        inputTextObjects = new GameObject[h, w];

        GameObject texts = inputGroup.transform.GetChild(2).gameObject;
        GameObject textObjects = inputGroup.transform.GetChild(1).gameObject;

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                inputTexts[i, j] = texts.transform.GetChild(i).GetChild(j).GetComponent<TextMeshProUGUI>();
                inputTextObjects[i, j] = textObjects.transform.GetChild(i).GetChild(j).gameObject;
            }
        }
    }

    /// <summary>
    /// 入力文字（大文字 / 小文字）を切り替えて表示
    /// </summary>
    void SetCharactersInputText()
    {
        int h = inputTexts.GetLength(0);
        int w = inputTexts.GetLength(1);

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                int index = i * w + j;
                inputTexts[i, j].text = changeSizeFlag ? alphanumericLargeTexts[index] : alphanumericSmallTexts[index];
            }
        }
    }

    /// <summary>
    /// 名前入力UIのカーソル移動・決定処理
    /// </summary>
    void SelectInputTextControl()
    {
        if (inputProgressNum != 0) return;

        // カーソル表示制御（通常文字 / Delete / 戻る / 決定）
        // ※ UI座標はレイアウトに依存
        if (inputTextVector.y != -1 && inputTextVector.x > -1 && inputTextVector.x < 13)
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(-600f + 100f * inputTextVector.x, 50f - 100f * inputTextVector.y);
        }
        else if (inputTextVector.y == -1)
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(550f, 150f);
        }
        else if (inputTextVector.y != -1 && inputTextVector.x == 13)
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 300f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(750f, -50f);
        }
        else if (inputTextVector.y != -1 && inputTextVector.x == -1)
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 200f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(-700f, 0f);
        }

        // 決定入力処理
        if (!enterInputFlag) return;

        // 文字追加
        if (nameText.GetComponent<TextMeshProUGUI>().text.Length < 8 &&
            inputTextVector.y != -1 &&
            inputTextVector.x >= 0 && inputTextVector.x < 12)
        {
            int index = (int)inputTextVector.y * inputTexts.GetLength(1) + (int)inputTextVector.x;

            string addChar = changeSizeFlag ? alphanumericLargeTexts[index] : alphanumericSmallTexts[index];

            if (addChar != "")
            {
                nameText.GetComponent<TextMeshProUGUI>().text += addChar;
            }
        }
        // Delete
        else if (inputTextVector.y == -1 && nameText.GetComponent<TextMeshProUGUI>().text.Length > 0)
        {
            string text = nameText.GetComponent<TextMeshProUGUI>().text;
            nameText.GetComponent<TextMeshProUGUI>().text = text.Remove(text.Length - 1);
        }
        // 決定
        else if (inputTextVector.x == 13 && nameText.GetComponent<TextMeshProUGUI>().text.Length > 0)
        {
            inputTextVector = Vector2.zero;
            titleButtonManager.ClickCreateDataDecisionButton();
        }
        // 戻る
        else if (inputTextVector.x == -1)
        {
            titleButtonManager.ClickCreateDataReturnButton();
        }
        // 大文字 / 小文字切り替え
        else if (inputTextVector.x == 12 && inputTextVector.y == 0)
        {
            changeSizeFlag = !changeSizeFlag;
            SetCharactersInputText();
        }

        enterInputFlag = false;
    }

    // ===== UI表示切り替え =====
    public void TitleUIActive(bool flag) => titleUIObject.SetActive(flag);
    public void SelectDataUIActive(bool flag) => selectDataUIObject.SetActive(flag);
    public void SelectDataDecisionUIActive(bool flag) => selectDataDecisionUIObject.SetActive(flag);
    public void CreateDataUIActive(bool flag) => createDataUIObject.SetActive(flag);
    public void CreateDataDecisionUIActive(bool flag) => createDataDecisionUIObject.SetActive(flag);
}
