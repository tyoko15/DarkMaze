using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] TitleButtonManager titleButtonManager;
    [SerializeField] DataManager dataManager;
    [SerializeField] FadeManager fadeManager;
    [SerializeField] GameObject fadeManagerObject;
    // UI取得
    [Header("UIの取得")]
    [SerializeField] GameObject titleUIObject;
    [SerializeField] GameObject selectDataUIObject;
    [SerializeField] TextMeshProUGUI[] selectDataText;
    [SerializeField] GameObject selectDataDecisionUIObject;
    [SerializeField] public TextMeshProUGUI selectDataDecisionText;
    [SerializeField] GameObject createDataUIObject;
    [SerializeField] public TMP_InputField nameInputField;
    [SerializeField] public TextMeshProUGUI createNameText;
    [SerializeField] GameObject createDataDecisionUIObject;

    int inputProgressNum;
    [SerializeField] public GameObject nameText;
    [SerializeField] GameObject inputGroup;
    TextMeshProUGUI[,] inputTexts;
    GameObject[,] inputTextObjects;
    [SerializeField] GameObject inputDeleteObject;
    [SerializeField] GameObject selectInputTextObject;
    [SerializeField] string[] alphanumericSmallTexts;
    [SerializeField] string[] alphanumericLargeTexts;
    bool changeSizeFlag = true;
    public Vector2 inputTextVector;
    public bool enterInputFlag;
    
    public int enterTitleUINum;

    public int enterSelectDataNum;
    public int selectDataNum;
    public bool[] dataNumFlag;

    // 進行
    // 0:TitleUI
    // 1:SelectDataUI
    // 2:CreateDataUI
    public int progressNum;
    public bool fadeFlag;

    void Start()
    {
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
        }
        else if (fade != null)
        {
            fadeManager = fade.GetComponent<FadeManager>();
        }
        TitleUIActive(true);
        SelectDataUIActive(false);
        CreateDataUIActive(false);

        if (fadeManager.titleFlag)
        {
            fadeManager.AfterFade();
            fadeFlag = true;
            fadeManager.fadeOutFlag = true;
        }

        GetInputText();
        SetCharactersInputText();
    }

    void Update()
    {
        DisplayData();
        SelectInputTextControl();
        if (fadeFlag)
        {
            FadeControl();
        }
    }

    // Data情報の表示
    void DisplayData()
    {     
        for(int i = 0; i < 3 ; i++)
        {
            if (dataManager.data[i].playerName == "")
            {
                selectDataText[i].text = "データなし";
                dataNumFlag[i] = true;
            }
            else
            {
                selectDataText[i].text = $"プレイヤー名: {dataManager.data[i].playerName}\n"
                                       + $"クリアステージ数: {dataManager.data[i].clearStageNum}\n";
                dataNumFlag[i] = false;
            }
        }
    }

    void FadeControl()
    {
        // フェイドイン終了時
        if (fadeManager.endFlag && fadeManager.fadeInFlag)
        {
            fadeManager.fadeInFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeIntervalFlag = true;
            // UI切り替え
            if(progressNum == 0)        // TitleUI
            {
                TitleUIActive(true);
                SelectDataUIActive(false);
            }
            else if(progressNum == 1)   // SelectDataUI
            {
                TitleUIActive(false);
                SelectDataUIActive(true);
                CreateDataUIActive(false);
            }
            else if(progressNum == 2)   // CreateDataUI
            {
                SelectDataUIActive(false);
                CreateDataUIActive(true);
            }
            else if (progressNum == 3) SceneManager.LoadScene("StageSelect");
        }
        // フェイドインターバル終了時
        else if (fadeManager.endFlag && fadeManager.fadeIntervalFlag)
        {            
            fadeManager.fadeIntervalFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeOutFlag = true;
        }
        // フェイドアウト終了時
        else if (fadeManager.endFlag && fadeManager.fadeOutFlag)
        {
            fadeManager.fadeOutFlag = false;
            fadeManager.endFlag = false;
            fadeFlag = false;
        }
        else if (!fadeManager.fadeInFlag && !fadeManager.fadeIntervalFlag && !fadeManager.fadeOutFlag)
        {
            fadeManager.fadeInFlag = true;
        }
    }
    public void CreateName()
    {
        string name = nameText.GetComponent<TextMeshProUGUI>().text;
        dataManager.SaveData(selectDataNum, name, 0, 0);
        nameText.GetComponent<TextMeshProUGUI>().text = "";
    }

    void GetInputText()
    {
        if (inputGroup != null)
        {
            int h = inputGroup.transform.GetChild(1).childCount;
            int w = inputGroup.transform.GetChild(1).GetChild(0).childCount;
            inputTexts = new TextMeshProUGUI[h,w];
            inputTextObjects = new GameObject[h,w];
            GameObject texts = inputGroup.transform.GetChild(2).gameObject;
            GameObject textObjects = inputGroup.transform.GetChild(1).gameObject;
            for (int i = 0; i < h; i++) for (int j = 0; j < w; j++)
            {
                inputTexts[i, j] = texts.transform.GetChild(i).GetChild(j).GetComponent<TextMeshProUGUI>();
                inputTextObjects[i, j] = textObjects.transform.GetChild(i).GetChild(j).gameObject;
            }
        }
    }

    void SetCharactersInputText()
    {
        int h = inputGroup.transform.GetChild(1).childCount;
        int w = inputGroup.transform.GetChild(1).GetChild(0).childCount;
        // 英数字
        if (!changeSizeFlag)    // 小文字
        {
            for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) inputTexts[i, j].text = alphanumericSmallTexts[i * w + j];
        }
        else if (changeSizeFlag)    // 大文字
        {
            for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) inputTexts[i, j].text = alphanumericLargeTexts[i * w + j];
        }
    }

    void SelectInputTextControl()
    {
        if (inputProgressNum == 0)
        {
            // Delete以外
            if (inputTextVector.y != -1 && inputTextVector.x > -1 && inputTextVector.x < 13)
            {
                selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
                selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(-600f + 100f * inputTextVector.x, 50f + -100f * inputTextVector.y);
            }
            // Delete
            else if (inputTextVector.y == -1)
            {
                selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
                selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(550f, 150f);
            }
            // 戻る
            if (inputTextVector.x == -1)
            {
                selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 200f);
                selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(-700f, 0f);
            }
            // 決定
            else if (inputTextVector.x == 13)
            {
                selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 300f);
                selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(750f, -50f);
            }

            // 決定
            if (enterInputFlag)
            {
                //Debug.Log($"1. 空欄ではないか判定{alphanumericSmallTexts[(int)inputTextVector.y * inputGroup.transform.GetChild(1).GetChild(0).childCount + (int)inputTextVector.x] != ""}, 2. deleteボタンではないか判定{inputTextVector.y != -1}, 3. 場内か判定{(inputTextVector.x != 12 && inputTextVector.x != 0)}");
                // 文字を追加
                if (nameText.GetComponent<TextMeshProUGUI>().text.Length < 8 && inputTextVector.y != -1 && (inputTextVector.x < 12 && inputTextVector.x >= 0))
                {
                    // 文字
                    if (alphanumericSmallTexts[(int)inputTextVector.y * inputGroup.transform.GetChild(1).GetChild(0).childCount + (int)inputTextVector.x] != "")
                    {
                        if (!changeSizeFlag)   // 小文字 
                        {
                            nameText.GetComponent<TextMeshProUGUI>().text += alphanumericSmallTexts[(int)inputTextVector.y * inputGroup.transform.GetChild(1).GetChild(0).childCount + (int)inputTextVector.x];
                        }
                        else                   // 大文字
                        {
                            nameText.GetComponent<TextMeshProUGUI>().text += alphanumericLargeTexts[(int)inputTextVector.y * inputGroup.transform.GetChild(1).GetChild(0).childCount + (int)inputTextVector.x];
                        }
                    }
                    // 空
                    else if (alphanumericSmallTexts[(int)inputTextVector.y * inputGroup.transform.GetChild(1).GetChild(0).childCount + (int)inputTextVector.x] == "")
                    {
                        Debug.Log("ない");
                    }
                }
                // 文字を一つ消す
                else if (inputTextVector.y == -1 && nameText.GetComponent<TextMeshProUGUI>().text.Length > 0)
                {
                    Debug.Log("Delete");
                    string text = nameText.GetComponent<TextMeshProUGUI>().text;
                    text = text.Remove(text.Length - 1);
                    nameText.GetComponent<TextMeshProUGUI>().text = text;
                }
                // 決定
                else if (inputTextVector.x == 13 && nameText.GetComponent<TextMeshProUGUI>().text.Length > 0)
                {
                    Debug.Log("Enter");
                    inputTextVector = Vector2.zero;
                    titleButtonManager.ClickCreateDataDecisionButton();
                }
                else if (inputTextVector.x == 13 && nameText.GetComponent<TextMeshProUGUI>().text.Length == 0)
                {
                    Debug.Log("入力させていません");
                }
                // 戻る
                else if (inputTextVector.x == -1)
                {
                    Debug.Log("Return");
                    titleButtonManager.ClickCreateDataReturnButton();
                }
                // 全角半角を変える
                else if (inputTextVector.x == 12 && inputTextVector.y == 0)
                {
                    changeSizeFlag = (!changeSizeFlag) ? true : false;
                    SetCharactersInputText();
                }
                enterInputFlag = false;
            }
        }
        else if (inputProgressNum == 1)
        {

        }
    }

    // UIの表示切り替え
    public void TitleUIActive(bool flag)
    {
        titleUIObject.SetActive(flag);
    }

    public void SelectDataUIActive(bool flag)
    {
        selectDataUIObject.SetActive(flag);
    }

    public void SelectDataDecisionUIActive(bool flag)
    {
        selectDataDecisionUIObject.SetActive(flag);
    }

    public void CreateDataUIActive(bool flag)
    {
        createDataUIObject.SetActive(flag);
    }

    public void CreateDataDecisionUIActive(bool flag)
    {
        createDataDecisionUIObject.SetActive(flag);
    }
}
