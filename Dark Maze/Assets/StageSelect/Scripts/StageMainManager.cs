using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StageMainManager : MonoBehaviour
{
    // スクリプトの取得
    [SerializeField] DataManager dataManager;
    [SerializeField] StageSelectManager[] stageSelectManagers;
    [SerializeField] GameObject fadeManagerObject;
    FadeManager fadeManager;
    bool startFadeFlag;
    bool endFadeFlag;

    // 各Stageの取得
    [SerializeField] GameObject[] fieldObjects;
    [SerializeField] int fieldNum;
    [SerializeField] int fadeFieldNum;

    [SerializeField] bool fadeFlag;
    [SerializeField] float fadeIntervalTime;
    [SerializeField] float fadeIntervalTimer;

    [SerializeField] int totalClearNum;
    [SerializeField] int clearFieldNum;    
    [SerializeField] int clearStageNum;

    bool enterFlag;

    [SerializeField] GameObject selectUI;
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject[] menuTexts;
    bool startMenuFlag;
    [SerializeField] float startMenuTime;
    float startMenuTimer;
    [SerializeField] bool menuFlag;
    [SerializeField] int menuSelectNum;
    void Start()
    {
        selectUI.SetActive(true);
        menuUI.SetActive(false);
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
        }
        else if (fade != null) fadeManager = fade.GetComponent<FadeManager>();
        fadeManager.AfterFade();
        fadeManager.fadeOutFlag = true;
        startFadeFlag = true;

        GameObject DataMana = GameObject.Find("DataManager");
        if(DataMana != null)
        {
            dataManager = DataMana.GetComponent<DataManager>();
            LoadClearStage();
        }
        else
        {
            //dataManager.SaveData(dataManager.useDataNum, dataManager.name, totalClearNum);
            clearFieldNum = totalClearNum / 5;
            clearStageNum = totalClearNum % 5;
            fieldNum = clearFieldNum;
            for (int i = 0; i < fieldObjects.Length; i++)
            {
                stageSelectManagers[i].selectObject.SetActive(false);
                if (fieldNum == i) stageSelectManagers[i].selectObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        if(!menuFlag) SelectControl();
        else MenuControl();     
    }

    void FirstFade()
    {
        // フェイドインターバル終了時
        if (fadeManager.endFlag && fadeManager.fadeIntervalFlag)
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
            //firstFadeFlag = true;
        }
        else if (!fadeManager.fadeInFlag && !fadeManager.fadeIntervalFlag && !fadeManager.fadeOutFlag)
        {
            fadeManager.fadeInFlag = true;
        }
    }

    void LoadClearStage()
    {
        totalClearNum = dataManager.data[dataManager.useDataNum].clearStageNum;
        clearFieldNum = totalClearNum / 5;
        clearStageNum = totalClearNum % 5;
        fieldNum = clearFieldNum;
        for(int i = 0; i < fieldObjects.Length; i++)
        {
            if (clearFieldNum == i)
            {
                fieldObjects[i].SetActive(true);
                stageSelectManagers[i].selectObject.SetActive(true);
            }
            else if (clearFieldNum != i)
            {
                fieldObjects[i].SetActive(false);
                stageSelectManagers[i].selectObject.SetActive(false);
            }
        }
        stageSelectManagers[clearFieldNum].selectNum = clearStageNum;
    }

    void SelectControl()
    {
        // 画面切り替え後のFade
        //if (!firstFadeFlag) FirstFade();
        if (startFadeFlag && fadeManager.fadeOutFlag && fadeManager.endFlag)
        {
            startFadeFlag = false;
            fadeManager.fadeOutFlag = false;
            fadeManager.endFlag = false;
        }
        else if (endFadeFlag && fadeManager.fadeIntervalFlag && fadeManager.endFlag)
        {
            endFadeFlag = false;
            fadeManager.fadeIntervalFlag = false;
            fadeManager.endFlag = false;
            SceneManager.LoadScene($"{fieldNum + 1}-{stageSelectManagers[fieldNum].selectNum + 1}");
            //Debug.Log($"{FieldNum + 1}-{stageSelectManager[FieldNum].selectNum + 1}");
            enterFlag = false;
        }
        if (startFadeFlag || endFadeFlag) fadeManager.FadeControl();
        // フィールド移動
        if (stageSelectManagers[fieldNum].changeNextFlag)
        {
            fadeFieldNum = fieldNum;
            fadeFlag = true;
        }
        else if (stageSelectManagers[fieldNum].changeReturnFlag)
        {
            fadeFieldNum = fieldNum;
            fadeFlag = true;
        }

        if (fadeFlag) ChangeField();

        if (stageSelectManagers[fieldNum].moveFlag == 0 && !fadeFlag && !endFadeFlag && enterFlag)
        {
            endFadeFlag = true;
            fadeManager.fadeInFlag = true;
        }
        else enterFlag = false;
    }
    // フィールドを移動する
    void ChangeField()
    {
        // 次のフィールドへ移動
        if (stageSelectManagers[fadeFieldNum].changeNextFlag)
        {
            // フェイドイン終了時
            if (fadeManager.endFlag && fadeManager.fadeInFlag)
            {
                fadeManager.fadeInFlag = false;
                fadeManager.endFlag = false;
                fadeManager.fadeIntervalFlag = true;
            }
            // フェイドインターバル終了時
            else if(fadeManager.endFlag && fadeManager.fadeIntervalFlag)
            {
                fieldNum++;
                for (int i = 0; i < fieldObjects.Length; i++)
                {
                    if (fieldNum != i) fieldObjects[i].SetActive(false);
                    else if (fieldNum == i) fieldObjects[i].SetActive(true);
                }
                fadeManager.fadeIntervalFlag = false;
                fadeManager.endFlag = false;
                fadeManager.fadeOutFlag = true;
            }
            // フェイドアウト終了時
            else if (fadeManager.endFlag && fadeManager.fadeOutFlag)
            {
                fadeManager.fadeOutFlag = false;
                fadeManager.endFlag = false;

                stageSelectManagers[fadeFieldNum].changeNextFlag = false;
                fadeFieldNum = 0;
                fadeFlag = false;
                stageSelectManagers[fieldNum].moveFlag = 3;
                stageSelectManagers[fieldNum].movePointsNum = 1;
            }
            else if (!fadeManager.fadeInFlag && !fadeManager.fadeIntervalFlag && !fadeManager.fadeOutFlag)
            {
                fadeManager.fadeInFlag = true;
            }
        }
        // 前のフィールドへ移動
        else if (stageSelectManagers[fadeFieldNum].changeReturnFlag)
        {
            // フェイドイン終了時
            if (fadeManager.endFlag && fadeManager.fadeInFlag)
            {
                fadeManager.fadeInFlag = false;
                fadeManager.endFlag = false;
                fadeManager.fadeIntervalFlag = true;
            }
            // フェイドインターバル終了時
            else if (fadeManager.endFlag && fadeManager.fadeIntervalFlag)
            {
                fieldNum--;
                for (int i = 0; i < fieldObjects.Length; i++)
                {
                    if (fieldNum != i) fieldObjects[i].SetActive(false);
                    else if (fieldNum == i) fieldObjects[i].SetActive(true);
                }
                fadeManager.fadeIntervalFlag = false;
                fadeManager.endFlag = false;
                fadeManager.fadeOutFlag = true;
            }
            // フェイドアウト終了時
            else if (fadeManager.endFlag && fadeManager.fadeOutFlag)
            {
                fadeManager.fadeOutFlag = false;
                fadeManager.endFlag = false;

                stageSelectManagers[fadeFieldNum].changeReturnFlag = false;
                fadeFieldNum = 0;
                fadeFlag = false;
                stageSelectManagers[fieldNum].moveFlag = -3;
                stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].select5toPoints.Length - 1;
            }
            else if (!fadeManager.fadeInFlag && !fadeManager.fadeOutFlag)
            {
                fadeManager.fadeInFlag = true;
            }
        }
    }

    void MenuControl()
    {
        if (startMenuFlag)
        {
            if(startMenuTimer > startMenuTime)
            {
                startMenuTimer = 0;
                startMenuFlag = false;
                menuUI.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 1f);
            }
            else if(startMenuTimer < startMenuTime)
            {
                float scale = Mathf.Lerp(0f, 1f, startMenuTimer / startMenuTime);
                menuUI.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);
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
                        SceneManager.LoadScene("Title");
                        GameObject fadeObject = GameObject.Find("FadeManager");
                        Destroy(fadeObject);
                        if(GameObject.Find("DataManager")) Destroy(GameObject.Find("DataManager"));
                    }
                    else if (menuSelectNum == 1)
                    {
                        selectUI.SetActive(true);
                        menuUI.SetActive(false);
                        menuSelectNum = 0;
                        for (int i = 0; i < menuTexts.Length; i++) TextAnime(menuTexts[i], false);
                        menuFlag = false;
                    }
                    enterFlag = false;
                }
            }
        }
    }
    void TextAnime(GameObject textOb, bool flag)
    {
        TextMeshProUGUI text = textOb.GetComponent<TextMeshProUGUI>();
        if (!flag) text.fontSize = 100f;
        else text.fontSize = 120f;

    }
    public void InputSelect(InputAction.CallbackContext context)
    {
        if(!fadeFlag)
        {
            // 右方向入力(進む)
            if (context.ReadValue<Vector2>().x > 0)
            {
                // 選択移動中に入力させるのをはじく
                if (stageSelectManagers[fieldNum].moveFlag == 0)
                {
                    stageSelectManagers[fieldNum].selectNum++;
                    stageSelectManagers[fieldNum].moveFlag = 1;
                    // ステージ移動をクリア数に応じて制限
                    if(fieldNum == clearFieldNum && stageSelectManagers[fieldNum].selectNum == clearStageNum + 1)
                    {
                        stageSelectManagers[fieldNum].selectNum = clearStageNum;
                        stageSelectManagers[fieldNum].moveFlag = 0;
                    }
                    // 次のFeildへ移動
                    if (stageSelectManagers[fieldNum].selectNum == stageSelectManagers[fieldNum].selectPoints.Length && fieldNum != fieldObjects.Length - 1)
                    {
                        stageSelectManagers[fieldNum].selectNum = 0;
                        stageSelectManagers[fieldNum].moveFlag = 2;
                        stageSelectManagers[fieldNum].movePointsNum = 1;
                    }
                    stageSelectManagers[fieldNum].movePointsNum = 1;
                    // 端で止める
                    if (stageSelectManagers[fieldNum].selectNum > stageSelectManagers[fieldNum].selectPoints.Length - 1)
                    {
                        stageSelectManagers[fieldNum].selectNum = stageSelectManagers[fieldNum].selectPoints.Length - 1;
                        stageSelectManagers[fieldNum].moveFlag = 0;
                        stageSelectManagers[fieldNum].movePointsNum = 0;
                    }
                }
            }
            // 左方向入力(戻る)  
            else if (context.ReadValue<Vector2>().x < 0)
            {
                // 選択移動中に入力させるのをはじく
                if (stageSelectManagers[fieldNum].moveFlag == 0)
                {
                    stageSelectManagers[fieldNum].selectNum--;
                    stageSelectManagers[fieldNum].moveFlag = -1;
                    // 各ステージの間の最後を取得する
                    if (stageSelectManagers[fieldNum].selectNum == 3) stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].select4to5Points.Length - 1;
                    else if (stageSelectManagers[fieldNum].selectNum == 2) stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].select3to4Points.Length - 1;
                    else if (stageSelectManagers[fieldNum].selectNum == 1) stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].select2to3Points.Length - 1;
                    else if (stageSelectManagers[fieldNum].selectNum == 0) stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].select1to2Points.Length - 1;
                    // 前のFeildへ移動
                    if (stageSelectManagers[fieldNum].selectNum == -1 && fieldNum != 0)
                    {
                        stageSelectManagers[fieldNum - 1].selectNum = stageSelectManagers[fieldNum - 1].selectPoints.Length - 1;
                        stageSelectManagers[fieldNum].selectNum = 0;
                        stageSelectManagers[fieldNum].moveFlag = -2;
                        stageSelectManagers[fieldNum].movePointsNum = stageSelectManagers[fieldNum].selectto1Points.Length - 1;
                    }
                    // 端で止める
                    if (stageSelectManagers[fieldNum].selectNum < 0)
                    {
                        stageSelectManagers[fieldNum].selectNum = 0;
                        stageSelectManagers[fieldNum].moveFlag = 0;
                        stageSelectManagers[fieldNum].movePointsNum = 0;
                    }
                }
            }
        }
    }

    public void InputEnter(InputAction.CallbackContext context)
    {
        if(context.started) enterFlag = true;
        if(menuFlag && menuSelectNum == 0)
        {
            fadeFlag = true;
            fadeManager.fadeInFlag = true;
        }
    }
    public void InputMenuEnter(InputAction.CallbackContext context)
    {
        if (context.started && !menuFlag)
        {
            menuUI.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
            selectUI.SetActive(false);
            menuUI.SetActive(true);
            menuFlag = true;
        }
    }
    public void InputMenuSelectControl(InputAction.CallbackContext context)
    {
        if(menuFlag)
        {
            if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                menuSelectNum++;
                if (menuSelectNum > 1) menuSelectNum = 0;
            }
            else if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                menuSelectNum--;
                if (menuSelectNum < 0) menuSelectNum = 1;
            }
        }
    }
}
