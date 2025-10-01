using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class TitleManager : MonoBehaviour
{
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

    [SerializeField] GameObject inputGroup;
    TextMeshProUGUI[,] inputTexts;
    GameObject[,] inputTextObjects;
    [SerializeField] GameObject inputDeleteObject;
    [SerializeField] GameObject selectInputTextObject;
    [SerializeField] string[] alphanumericTexts;
    public Vector2 inputTextVector;
    
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
        SelectInputText();
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
        dataManager.SaveData(selectDataNum, nameInputField.text, 0, 0);
        nameInputField.text = null;
    }

    void GetInputText()
    {
        if (inputGroup != null)
        {
            int w = inputGroup.transform.GetChild(1).childCount;
            int h = inputGroup.transform.GetChild(1).GetChild(0).childCount;
            inputTexts = new TextMeshProUGUI[w,h];
            inputTextObjects = new GameObject[w,h];
            GameObject texts = inputGroup.transform.GetChild(2).gameObject;
            GameObject textObjects = inputGroup.transform.GetChild(1).gameObject;
            for (int i = 0; i < w; i++) for (int j = 0; j < h; j++)
            {
                inputTexts[i, j] = texts.transform.GetChild(i).GetChild(j).GetComponent<TextMeshProUGUI>();
                inputTextObjects[i, j] = textObjects.transform.GetChild(i).GetChild(j).gameObject;
            }
        }
    }

    void SetCharactersInputText()
    {
        // 英数字
        int w = inputGroup.transform.GetChild(1).childCount;
        int h = inputGroup.transform.GetChild(1).GetChild(0).childCount;
        for (int i = 0; i < w; i++) for (int j = 0; j < h; j++) inputTexts[i, j].text = alphanumericTexts[i * h + j];
    }

    void SelectInputText()
    {
        if (inputTextVector.y != -1)
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(-600f + 100f * inputTextVector.x, 50f + -100f * inputTextVector.y);
        }
        else
        {
            selectInputTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
            selectInputTextObject.GetComponent<RectTransform>().localPosition = new Vector2(550f, 150f);
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
