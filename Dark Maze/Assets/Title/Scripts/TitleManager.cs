using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    [SerializeField] FadeManager fadeManager;
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
        TitleUIActive(true);
        SelectDataUIActive(false);
        CreateDataUIActive(false);
    }

    void Update()
    {
        DisplayData();
        if(fadeFlag)
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
        dataManager.SaveData(selectDataNum, nameInputField.text, 0);
        nameInputField.text = null;
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
