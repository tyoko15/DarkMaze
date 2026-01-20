using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewStageSelectManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    [Header("Input関連")]
    [SerializeField] PlayerInput playerInput;
    InputAction selectAction;
    InputAction enterAciton;
    [Header("StageImage関連")]
    [SerializeField] GameObject stageGroup;
    GameObject[] stageImageObjects;
    [SerializeField] GameObject selectObject;
    [SerializeField] GameObject returnButton;
    [SerializeField] GameObject stageNameText;
    [SerializeField] GameObject[] windowImages;     // 0.still 1.now 2.clear
    [SerializeField] GameObject[] cloudImages;
    [Header("Fade関連")]
    [SerializeField] GameObject fadeManagerObject;
    FadeManager fadeManager;
    bool fadeFlag;
    bool startFadeFlag;
    bool endFadeFlag;


    [SerializeField] int totalClearNum;
    [SerializeField] int clearFieldNum;
    [SerializeField] int clearStageNum;

    [Header("動きのパラメーター")]
    [SerializeField] bool selectReturnFlag;
    [SerializeField] int selectFieldNum;
    [SerializeField] int selectStageNum;
    [SerializeField] int selectNum;
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

    public bool cloudFlag;
    [SerializeField] float cloudOpenTime;
    float cloudOpenTimer;
    bool cloudDownFlag;
    bool cloudUpFlag;


    void Start()
    {
        GetStageObject();
        selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale *= 1.2f;
        selectAction = playerInput.actions.FindAction("Move");
        enterAciton = playerInput.actions.FindAction("Enter");

        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
        }
        else if (fade != null) fadeManager = fade.GetComponent<FadeManager>();
        fadeManager.AfterFade();
        fadeFlag = true;
        fadeManager.fadeOutFlag = true;
        startFadeFlag = true;

        GameObject DataMana = GameObject.Find("DataManager");
        if (DataMana != null)
        {
            dataManager = DataMana.GetComponent<DataManager>();
            LoadClearStage();
            if (dataManager.nextFieldFlag) cloudFlag = true;
        }
        else
        {
            //dataManager.SaveData(dataManager.useDataNum, dataManager.name, totalClearNum);
            clearFieldNum = totalClearNum / 5;
            clearStageNum = totalClearNum % 5;
        }
        if (totalClearNum > 14) totalClearNum = 14;
        if (dataManager)
        {
            selectNum = dataManager.data[dataManager.useDataNum].selectStageNum;
            selectFieldNum = selectNum / 5 + 1;
            selectStageNum = selectNum % 5 + 1;
        }
        else
        {
            selectNum = totalClearNum;
            selectFieldNum = totalClearNum / 5 + 1;
            selectStageNum = totalClearNum % 5 + 1;
        }
        if (cloudFlag)
        {
            selectNum--;
            selectFieldNum--;
            selectStageNum--;
        }
        int n = selectNum / 5 - 1;
        for (int i = 0; i < stageGroup.transform.childCount; i++)
        {
            float y = -900f + (n * -1000f) + 200f * i;
            stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
        }
        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectNum].GetComponent<RectTransform>().anchoredPosition.y);
        stageNameText.GetComponent<TextMeshProUGUI>().text = $"{clearFieldNum + 1} - {clearStageNum + 1}";
        windowImages[3].SetActive(false);
        WindowControl();
    }

    void Update()
    {
        // フェイドイン終了時
        if (fadeManager.endFlag && fadeManager.fadeInFlag)
        {
            fadeManager.fadeInFlag = false;
            fadeManager.endFlag = false;
            fadeManager.fadeIntervalFlag = true;
            fadeFlag = false;
        }        
        // フェイドインターバル終了時
        else if (endFadeFlag && fadeManager.fadeIntervalFlag && fadeManager.endFlag)
        {
            endFadeFlag = false;
            fadeManager.fadeIntervalFlag = false;
            fadeManager.endFlag = false;
            int fieldNum = selectNum / 5;
            int stageNum = selectNum % 5;
            if (selectReturnFlag)
            {
                SceneManager.LoadScene(0);
                fadeManager.titleFlag = true;
            }
            else SceneManager.LoadScene($"{fieldNum + 1}-{stageNum + 1}");
            
            //SceneManager.LoadScene($"{fieldNum + 1}-{stageSelectManagers[fieldNum].selectNum + 1}");
            //Debug.Log($"{FieldNum + 1}-{stageSelectManager[FieldNum].selectNum + 1}");
            fadeFlag = false;
            enterFlag = false;
        }
        // フェイドアウト終了時
        else if (startFadeFlag && fadeManager.fadeOutFlag && fadeManager.endFlag)
        {
            startFadeFlag = false;
            fadeManager.fadeOutFlag = false;
            fadeManager.endFlag = false;
            fadeFlag = false;
        }

        if (startFadeFlag || endFadeFlag) fadeManager.FadeControl();
        SelectControl();

        //CloudControl();
    }

    void GetStageObject()
    {
        stageImageObjects = new GameObject[stageGroup.transform.childCount];
        for (int i = 0; i < stageGroup.transform.childCount; i++) stageImageObjects[i] = stageGroup.transform.GetChild(i).gameObject;
    }

    void SelectControl()
    {
        // Enterの取得
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
        }

        if (!returnMoveFlag && !fadeFlag)
        {
            if (selectAction.ReadValue<Vector2>().x != 0)
            {
                returnMoveFlag = true;
                if (selectAction.ReadValue<Vector2>().x > 0) selectVector.x = 1f;
                else if (selectAction.ReadValue<Vector2>().x < 0) selectVector.x = -1f;
                if (!selectReturnFlag)
                {
                    returnButton.GetComponent<RectTransform>().localScale *= 1.2f;
                    selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector2.one;
                    selectReturnFlag = true;
                }
                else
                {
                    returnButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale *= 1.2f;
                    selectReturnFlag = false;
                }
            }
        }
        else if (returnMoveFlag)
        {
            if (returnMoveTimer > returnMoveTime)
            {
                returnMoveTimer = 0f;
                returnMoveFlag = false;
            }
            else if (returnMoveTimer < returnMoveTime)
            {
                returnMoveTimer += Time.deltaTime;
            } 
        }

        if (!selectReturnFlag && !fadeFlag)
        {
            if (selectAction.ReadValue<Vector2>().y > 0 && !selectMoveFlag)
            {
                selectMoveFlag = true;
                selectVector.y = 1f;
                selectNum++;
                if (selectNum == 5 || selectNum == 10)
                {
                    changeStageFlag = true;
                    selectFieldNum++;
                    if (selectFieldNum -1 == clearFieldNum) cloudDownFlag = true;
                }
                if (selectNum > totalClearNum)
                {
                    selectNum = totalClearNum;
                    selectFieldNum = selectNum / 5;
                    selectStageNum = selectNum % 5;
                    changeStageFlag = false;
                    selectMoveFlag = false;
                }
                else if (selectNum > 14)
                {
                    selectNum = 14;
                    selectMoveFlag = false;
                }
            }
            else if (selectAction.ReadValue<Vector2>().y < 0 && !selectMoveFlag)
            {
                selectMoveFlag = true;
                selectVector.y = -1f;
                selectNum--;
                if (selectNum == 4 || selectNum == 9)
                {
                    changeStageFlag = true;
                    selectFieldNum--;
                    if (selectFieldNum == clearFieldNum) cloudUpFlag = true;
                }
                if (selectNum < 0)
                {
                    selectNum = 0;
                    selectMoveFlag = false;
                }
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

                        int fieldNum = selectNum / 5;
                        int stageNum = selectNum % 5;
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
                            int n = selectNum / 5 - 1;
                            for (int i = 0; i < stageGroup.transform.childCount; i++)
                            {
                                float y = Mathf.Lerp(100f + (n * -1000f) + 200f * i, -900f + (n * -1000f) + 200f * i, changeStageTimer / changeStageTime);
                                stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
                            }
                            selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectNum - 1].GetComponent<RectTransform>().anchoredPosition.y);
                        }
                        else if (selectVector.y == -1f)
                        {
                            int n = selectNum / 5;
                            for (int i = 0; i < stageGroup.transform.childCount; i++)
                            {
                                float y = Mathf.Lerp(-900f + (n * -1000f) + 200f * i, 100f + (n * -1000f) + 200f * i, changeStageTimer / changeStageTime);
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

    void WindowControl()
    {
        /*
        int selectClearStageNum = 0;
        //if (selectFieldNum == 1) selectClearStageNum = totalClearNum;
        //else if (selectFieldNum > 1) selectClearStageNum = totalClearNum - selectFieldNum * 5;
        selectClearStageNum = totalClearNum - selectFieldNum * 5 + selectStageNum;
        // ClearWindowの設定
        windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 200f * selectClearStageNum);
        float y = windowImages[2].GetComponent<RectTransform>().sizeDelta.y / 2;
        windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
        // NowWindowの設定
        y = y * 2 + 100f;
        windowImages[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
        // StillWindowの設定
        windowImages[0].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, (5 - selectClearStageNum) * 200f);
        y += 100f + ((5 - selectClearStageNum) * 200f / 2);
        windowImages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
        */
        // 新しい

        if (selectFieldNum -1 == clearFieldNum)
        {
            int nowStageNum = clearStageNum;
            windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 200f * nowStageNum);
            float y = windowImages[2].GetComponent<RectTransform>().sizeDelta.y / 2;
            windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
            y = y * 2 + 100f; 
            windowImages[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
            windowImages[0].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, (5 - nowStageNum) * 200f);
            y += 100f + ((5 - nowStageNum) * 200f / 2);
            windowImages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
        }
        else
        {
            windowImages[2].GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 1200f);
            windowImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 600f);
        }
    }

    void CloudControl()
    {
        if (changeStageFlag && !cloudFlag)
        {
            float y = 0f;
            if (cloudUpFlag) y = Mathf.Lerp(550f, 1550f, changeStageTimer / changeStageTime);
            else if (cloudDownFlag) y = Mathf.Lerp(1550f, 550f, changeStageTimer / changeStageTime);
            else y = 550f;
            Color cloudColor = Color.white;
            cloudColor.a = 1f;
            for (int i = 0; i < 3; i++)
            {
                cloudImages[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, y);
                cloudImages[i].SetActive(true);
                cloudImages[i].GetComponent<Image>().color = cloudColor;
            }
            cloudImages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, y);
            cloudImages[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
            cloudImages[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(-250f, y);
        }
        if (cloudFlag && selectVector.y > 0)
        {
            if (cloudOpenTimer > cloudOpenTime)
            {
                cloudOpenTimer = 0;
                cloudFlag = false;
                changeStageFlag = true;
                for (int i = 0; i < 3; i++) cloudImages[i].SetActive(false);
            }
            else if (cloudOpenTimer < cloudOpenTime)
            {
                cloudOpenTimer += Time.deltaTime;
                float[] x = new float[3];
                x[0] = Mathf.Lerp(250f, 750f, cloudOpenTimer / cloudOpenTime);
                x[1] = Mathf.Lerp(0f, 500f, cloudOpenTimer / cloudOpenTime);
                x[2] = Mathf.Lerp(-250f, -750f, cloudOpenTimer / cloudOpenTime);
                float a = Mathf.Lerp(1f, 0.25f, cloudOpenTimer / cloudOpenTime);
                Color cloudColor = Color.white;
                cloudColor.a = a;
                for (int i = 0; i < 3; i++)
                {
                    cloudImages[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x[i], cloudImages[i].GetComponent<RectTransform>().anchoredPosition.y);
                    cloudImages[i].GetComponent<Image>().color = cloudColor;
                }
            }
        }
    }

    void LoadClearStage()
    {
        Debug.Log("セーブデータを読み込みます。");
        totalClearNum = dataManager.data[dataManager.useDataNum].clearStageNum;
        clearFieldNum = totalClearNum / 5;
        clearStageNum = totalClearNum % 5;
        int dateNum = dataManager.useDataNum;
        int selectFieldNum = dataManager.data[dateNum].selectStageNum / 5;
        int selectStageNum = dataManager.data[dateNum].selectStageNum % 5;
    }
}
