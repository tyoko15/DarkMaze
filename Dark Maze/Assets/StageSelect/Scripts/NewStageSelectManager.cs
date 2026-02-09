using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ選択画面全体を管理するクラス
/// ・入力制御
/// ・ステージカーソル移動
/// ・解放済みステージの表示
/// ・フェード演出
/// ・シーン遷移
/// を一括で制御する
/// </summary>
public class NewStageSelectManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;

    // ==============================
    // Input関連
    // ==============================
    [Header("Input関連")]
    [SerializeField] PlayerInput playerInput;
    InputAction selectAction;   // 移動入力
    InputAction enterAciton;    // 決定入力

    // ==============================
    // StageImage関連
    // ==============================
    [Header("StageImage関連")]
    [SerializeField] GameObject stageGroup;          // ステージ画像の親
    GameObject[] stageImageObjects;                  // 各ステージ画像
    [SerializeField] GameObject selectObject;        // 選択カーソル
    [SerializeField] GameObject returnButton;        // 戻るボタン
    [SerializeField] GameObject stageNameText;       // ステージ名表示
    [SerializeField] GameObject[] windowImages;      // 0:未到達 1:現在 2:クリア 3:遷移用
    [SerializeField] GameObject[] cloudImages;       // 雲演出用

    // ==============================
    // Fade関連
    // ==============================
    [Header("Fade関連")]
    [SerializeField] GameObject fadeManagerObject;
    FadeManager fadeManager;
    bool fadeFlag;
    bool startFadeFlag;
    bool endFadeFlag;

    // ==============================
    // Audio関連
    // ==============================
    [SerializeField] GameObject audioManagerObject;
    public GameObject audioManager;
    public AudioSource[] gameBgms;
    public AudioSource[] gameSes;
    public AudioSource[] playerSes;
    public AudioSource[] enemySes;
    public AudioSource[] gimmickSes;

    // ==============================
    // クリア情報
    // ==============================
    [SerializeField] int totalClearNum;
    [SerializeField] int clearFieldNum;
    [SerializeField] int clearStageNum;
    bool allClearFlag;

    // ==============================
    // 選択・移動パラメータ
    // ==============================
    [Header("動きのパラメーター")]
    [SerializeField] bool selectReturnFlag;   // 戻るボタン選択中か
    [SerializeField] int selectFieldNum;      // 現在のフィールド番号
    [SerializeField] int selectStageNum;      // 現在のステージ番号
    [SerializeField] int selectNum;            // 全体通し番号
    [SerializeField] float selectMoveTime;
    float selectMoveTimer;
    bool changeStageFlag;
    [SerializeField] float changeStageTime;
    float changeStageTimer;

    bool selectMoveFlag;
    bool returnMoveFlag;
    [SerializeField] float returnMoveTime;
    float returnMoveTimer;
    Vector2 selectVector;

    bool enterFlag;

    // ==============================
    // 雲演出
    // ==============================
    public bool cloudFlag;
    [SerializeField] float cloudOpenTime;
    float cloudOpenTimer;
    bool cloudDownFlag;
    bool cloudUpFlag;

    // ==============================
    // 初期化
    // ==============================
    void Start()
    {
        // ステージ画像を配列に格納
        GetStageObject();

        // 選択カーソルを少し拡大
        selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale *= 1.2f;

        // InputAction取得
        selectAction = playerInput.actions.FindAction("Move");
        enterAciton = playerInput.actions.FindAction("Enter");

        // FadeManager生成 or 取得
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
        }
        else fadeManager = fade.GetComponent<FadeManager>();

        // フェード初期設定
        fadeManager.AfterFade();
        fadeFlag = true;
        fadeManager.fadeOutFlag = true;
        startFadeFlag = true;

        // セーブデータ取得
        GameObject DataMana = GameObject.Find("DataManager");
        if (DataMana != null)
        {
            dataManager = DataMana.GetComponent<DataManager>();
            LoadClearStage();
            if (dataManager.nextFieldFlag) cloudFlag = true;
        }
        else
        {
            if (totalClearNum > 7) allClearFlag = true;
            if (totalClearNum > 7) totalClearNum = 7;
            clearFieldNum = totalClearNum / 4 + 1;
            clearStageNum = totalClearNum % 4 + 1;
        }



        // 前回選択ステージを復元
        if (dataManager)
        {
            selectNum = dataManager.data[dataManager.useDataNum].selectStageNum;
            selectFieldNum = selectNum / 4 + 1;
            selectStageNum = selectNum % 4 + 1;
        }
        else
        {
            selectNum = totalClearNum;
            selectFieldNum = totalClearNum / 4 + 1;
            selectStageNum = totalClearNum % 4 + 1;
        }

        // 新フィールド解放演出時の補正
        //if (cloudFlag)
        //{
        //    selectNum--;
        //    selectFieldNum--;
        //    selectStageNum--;
        //}

        // ステージ画像の初期配置
        int n = selectNum / 4 - 1;
        for (int i = 0; i < stageGroup.transform.childCount; i++)
        {
            float y = -875f + (n * -1000f) + 250f * i;
            stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition =
                new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
        }

        // カーソル位置設定
        selectObject.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x,
            stageImageObjects[selectNum].GetComponent<RectTransform>().anchoredPosition.y);

        // ステージ名表示
        stageNameText.GetComponent<TextMeshProUGUI>().text = $"{selectFieldNum} - {selectStageNum}";

        windowImages[3].SetActive(false);
        WindowControl();
    }

    // ==============================
    // 更新処理
    // ==============================
    void Update()
    {
        // フェードイン終了
        if (fadeManager.endFlag && fadeManager.fadeInFlag)
        {
            fadeManager.fadeInFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeIntervalFlag = true;
            fadeFlag = false;
        }
        // フェードインターバル終了 → シーン遷移
        else if (endFadeFlag && fadeManager.fadeIntervalFlag && fadeManager.endFlag)
        {
            endFadeFlag = false;
            fadeManager.fadeIntervalFlag = false;
            fadeManager.endFlag = false;

            int fieldNum = selectNum / 4;
            int stageNum = selectNum % 4;

            if (selectReturnFlag)
            {
                SceneManager.LoadScene(0);
                fadeManager.titleFlag = true;
            }
            else
            {
                SceneManager.LoadScene($"{fieldNum + 1}-{stageNum + 1}");                
            }
            if (dataManager != null) dataManager.SaveData(dataManager.useDataNum, dataManager.data[dataManager.useDataNum].playerName, totalClearNum, selectNum);
            fadeFlag = false;
            enterFlag = false;
        }
        // フェードアウト終了
        else if (startFadeFlag && fadeManager.fadeOutFlag && fadeManager.endFlag)
        {
            startFadeFlag = false;
            fadeManager.fadeOutFlag = false;
            fadeManager.endFlag = false;
            fadeFlag = false;
        }

        if (startFadeFlag || endFadeFlag) fadeManager.FadeControl();

        // 入力・選択制御
        SelectControl();
    }

    /// <summary>
    /// ステージ画像を配列に格納
    /// </summary>
    void GetStageObject()
    {
        stageImageObjects = new GameObject[stageGroup.transform.childCount];
        for (int i = 0; i < stageGroup.transform.childCount; i++) stageImageObjects[i] = stageGroup.transform.GetChild(i).gameObject;
    }

    /// <summary>
    /// ステージ選択操作全般
    /// </summary>
    void SelectControl()
    {
        // 決定入力
        if (enterAciton.ReadValue<float>() > 0 && !fadeFlag)
        {
            enterFlag = true;
        }

        if (enterFlag)
        {
            endFadeFlag = true;
            fadeManager.fadeInFlag = true;
            fadeFlag = true;
            enterFlag = false;
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 1);
        }

        // 左右入力（戻るボタン切り替え）
        if (!returnMoveFlag && !fadeManager.fadeFlag)
        {
            if (selectAction.ReadValue<Vector2>().x != 0)
            {
                bool old = selectReturnFlag;
                returnMoveFlag = true;                
                if (selectAction.ReadValue<Vector2>().x > 0.5f) selectReturnFlag = true;
                else if (selectAction.ReadValue<Vector2>().x < -0.5f) selectReturnFlag = false;

                if (selectReturnFlag)
                {
                    returnButton.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
                    selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector2.one;
                }
                else
                {
                    returnButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
                }
                if (selectReturnFlag != old) AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 1);
            }
        }
        else if (returnMoveFlag)
        {
            if (returnMoveTimer > returnMoveTime)
            {
                returnMoveTimer = 0f;
                returnMoveFlag = false;
            }
            else returnMoveTimer += Time.deltaTime;
        }

        // 上下入力（ステージ移動）
        if (!selectReturnFlag && !fadeManager.fadeFlag)
        {
            if (selectAction.ReadValue<Vector2>().y > 0.5f && !selectMoveFlag)
            {
                int old = selectNum;

                selectMoveFlag = true;
                selectVector.y = 1f;
                selectNum++;

                if (selectNum == 4)
                {
                    changeStageFlag = true;
                    selectFieldNum++;
                }

                if (selectNum > totalClearNum)
                {
                    selectNum = totalClearNum;
                    selectMoveFlag = false;
                }
                else if (selectNum > 7)
                {
                    selectNum = 7;
                    selectMoveFlag = false;
                }
                if (selectNum != old) AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 1);
            }
            else if (selectAction.ReadValue<Vector2>().y < -0.5f && !selectMoveFlag)
            {
                int old = selectNum;
               
                selectMoveFlag = true;
                selectVector.y = -1f;
                selectNum--;

                if (selectNum == 3 || selectNum == 9)
                {
                    changeStageFlag = true;
                    selectFieldNum--;
                }

                if (selectNum < 0)
                {
                    selectNum = 0;
                    selectMoveFlag = false;
                }
                if (selectNum != old) AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gameSes, 1);
            }
        }
        if (selectMoveFlag)
        {
            if (!cloudFlag)
            {
                if (!changeStageFlag)
                {
                    if (selectMoveTimer > selectMoveTime)
                    {
                        selectMoveTimer = 0;
                        selectVector.y = 0f;
                        selectMoveFlag = false;

                        int fieldNum = selectNum / 4;
                        int stageNum = selectNum % 4;
                        stageNameText.GetComponent<TextMeshProUGUI>().text = $"{fieldNum + 1} - {stageNum + 1}";
                    }
                    else if (selectMoveTimer < selectMoveTime)
                    {
                        selectMoveTimer += Time.deltaTime;
                        if (selectVector.y == 1f)
                        {
                            float y = Mathf.Lerp(stageImageObjects[selectNum - 1].GetComponent<RectTransform>().anchoredPosition.y, stageImageObjects[selectNum].GetComponent<RectTransform>().anchoredPosition.y, selectMoveTimer / selectMoveTime);
                            selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, y);
                        }
                        else if (selectVector.y == -1f)
                        {
                            float y = Mathf.Lerp(stageImageObjects[selectNum + 1].GetComponent<RectTransform>().anchoredPosition.y, stageImageObjects[selectNum].GetComponent<RectTransform>().anchoredPosition.y, selectMoveTimer / selectMoveTime);
                            selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, y);
                        }
                    }
                }
                else if (changeStageFlag)
                {
                    if (changeStageTimer > changeStageTime)
                    {
                        windowImages[3].SetActive(false);
                        changeStageFlag = false;
                        changeStageTimer = 0f;
                        cloudDownFlag = false;
                        cloudUpFlag = false;
                    }
                    else if (changeStageTimer < changeStageTime)
                    {
                        windowImages[3].SetActive(true);
                        changeStageTimer += Time.deltaTime;
                        if (selectVector.y == 1f)
                        {
                            int n = selectNum / 4 - 1;
                            for (int i = 0; i < stageGroup.transform.childCount; i++)
                            {
                                float y = Mathf.Lerp(100f + (n * -1000f) + 250f * i, -875f + (n * -1000f) + 250f * i, changeStageTimer / changeStageTime);
                                stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
                            }
                            selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectNum - 1].GetComponent<RectTransform>().anchoredPosition.y);
                        }
                        else if (selectVector.y == -1f)
                        {
                            int n = selectNum / 4;
                            for (int i = 0; i < stageGroup.transform.childCount; i++)
                            {
                                float y = Mathf.Lerp(-875f + (n * -1000f) + 250f * i, 100f + (n * -1000f) + 250f * i, changeStageTimer / changeStageTime);
                                stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
                            }
                            selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectNum + 1].GetComponent<RectTransform>().anchoredPosition.y);
                        }
                        WindowControl();
                    }
                }
            }
        }
    }

    /// <summary>
    /// ステージ解放状態に応じたウィンドウ表示制御
    /// </summary>
    void WindowControl()
    {
        if (allClearFlag)
        {
            windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 1200f);
            windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 600f);
        }
        else if (selectFieldNum == clearFieldNum)
        {
            int nowStageNum = clearStageNum - 1;
            windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 250f * nowStageNum);
            float y = windowImages[2].GetComponent<RectTransform>().sizeDelta.y / 2;
            windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
            y = y * 2 + 100f;
            windowImages[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
            windowImages[0].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, (4 - nowStageNum) * 250f);
            y += 100f + ((4 - nowStageNum) * 250f / 2);
            windowImages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
        }
        else
        {
            windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 1200f);
            windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 600f);
        }
    }

    /// <summary>
    /// セーブデータからクリア情報を読み込む
    /// </summary>
    void LoadClearStage()
    {
        Debug.Log("セーブデータを読み込みます。");
        totalClearNum = dataManager.data[dataManager.useDataNum].clearStageNum;
        int dateNum = dataManager.useDataNum;
        int selectFielddataNum = dataManager.data[dateNum].selectStageNum / 4;
        int selectStagedataNum = dataManager.data[dateNum].selectStageNum % 4;
        selectFieldNum = selectFielddataNum;
        selectStageNum = selectStagedataNum;
        totalClearNum = dataManager.data[dataManager.useDataNum].clearStageNum;
        if (totalClearNum > 7) allClearFlag = true;
        if (totalClearNum > 7) totalClearNum = 7;
        clearFieldNum = totalClearNum / 4 + 1;
        clearStageNum = totalClearNum % 4 + 1;
    }
}
