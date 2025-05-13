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
    [SerializeField] GameObject[] FieldObject;
    [SerializeField] int FieldNum;
    [SerializeField] int fadeFieldNum;

    [SerializeField] bool fadeFlag;
    [SerializeField] float fadeIntervalTime;
    [SerializeField] float fadeIntervalTimer;

    [SerializeField] int totalClearNum;
    [SerializeField] int clearFieldNum;    
    [SerializeField] int clearStageNum;

    bool enterFlag;



    void Start()
    {
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
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
            clearFieldNum = totalClearNum / 5;
            clearStageNum = totalClearNum % 5;
        }
        for(int i = 0; i < FieldObject.Length; i++)
        {
            stageSelectManagers[i].selectObject.SetActive(false);
            if(FieldNum == i) stageSelectManagers[i].selectObject.SetActive(true);
        }
    }

    void Update()
    {
        // 画面切り替え後のFade
        //if (!firstFadeFlag) FirstFade();
        if (startFadeFlag && fadeManager.fadeOutFlag && fadeManager.endFlag) startFadeFlag = false;
        else if (endFadeFlag && fadeManager.fadeOutFlag && fadeManager.endFlag) endFadeFlag = false;
        if (startFadeFlag || endFadeFlag) fadeManager.FadeControl();
        // フィールド移動
        if (stageSelectManagers[FieldNum].changeNextFlag)
        {
            fadeFieldNum = FieldNum;
            fadeFlag = true;
        }
        else if (stageSelectManagers[FieldNum].changeReturnFlag)
        {
            fadeFieldNum = FieldNum;
            fadeFlag = true;
        }

        if (fadeFlag) ChangeField();

        if (stageSelectManagers[FieldNum].moveFlag == 0 && !fadeFlag && enterFlag)
        {
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag)
            {
                SceneManager.LoadScene($"{FieldNum + 1}-{stageSelectManagers[FieldNum].selectNum + 1}");
                //Debug.Log($"{FieldNum + 1}-{stageSelectManager[FieldNum].selectNum + 1}");
                enterFlag = false;
            }
            else
            {
                fadeManager.FadeControl();
                fadeManager.fadeIntervalFlag = true;
            }
        }
        else enterFlag = false;
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
                FieldNum++;
                for (int i = 0; i < FieldObject.Length; i++)
                {
                    if (FieldNum != i) FieldObject[i].SetActive(false);
                    else if (FieldNum == i) FieldObject[i].SetActive(true);
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
                stageSelectManagers[FieldNum].moveFlag = 3;
                stageSelectManagers[FieldNum].movePointsNum = 1;
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
                FieldNum--;
                for (int i = 0; i < FieldObject.Length; i++)
                {
                    if (FieldNum != i) FieldObject[i].SetActive(false);
                    else if (FieldNum == i) FieldObject[i].SetActive(true);
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
                stageSelectManagers[FieldNum].moveFlag = -3;
                stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].select5toPoints.Length - 1;
            }
            else if (!fadeManager.fadeInFlag && !fadeManager.fadeOutFlag)
            {
                fadeManager.fadeInFlag = true;
            }
        }
    }

    public void InputSelect(InputAction.CallbackContext context)
    {
        if(!fadeFlag)
        {
            // 右方向入力(進む)
            if (context.ReadValue<Vector2>().x > 0)
            {
                // 選択移動中に入力させるのをはじく
                if (stageSelectManagers[FieldNum].moveFlag == 0)
                {
                    stageSelectManagers[FieldNum].selectNum++;
                    stageSelectManagers[FieldNum].moveFlag = 1;
                    // ステージ移動をクリア数に応じて制限
                    if(FieldNum == clearFieldNum && stageSelectManagers[FieldNum].selectNum == clearStageNum + 1)
                    {
                        stageSelectManagers[FieldNum].selectNum = clearStageNum;
                        stageSelectManagers[FieldNum].moveFlag = 0;
                    }
                    // 次のFeildへ移動
                    if (stageSelectManagers[FieldNum].selectNum == stageSelectManagers[FieldNum].selectPoints.Length && FieldNum != FieldObject.Length - 1)
                    {
                        stageSelectManagers[FieldNum].selectNum = 0;
                        stageSelectManagers[FieldNum].moveFlag = 2;
                        stageSelectManagers[FieldNum].movePointsNum = 1;
                    }
                    stageSelectManagers[FieldNum].movePointsNum = 1;
                    // 端で止める
                    if (stageSelectManagers[FieldNum].selectNum > stageSelectManagers[FieldNum].selectPoints.Length - 1)
                    {
                        stageSelectManagers[FieldNum].selectNum = stageSelectManagers[FieldNum].selectPoints.Length - 1;
                        stageSelectManagers[FieldNum].moveFlag = 0;
                        stageSelectManagers[FieldNum].movePointsNum = 0;
                    }
                }
            }
            // 左方向入力(戻る)  
            else if (context.ReadValue<Vector2>().x < 0)
            {
                // 選択移動中に入力させるのをはじく
                if (stageSelectManagers[FieldNum].moveFlag == 0)
                {
                    stageSelectManagers[FieldNum].selectNum--;
                    stageSelectManagers[FieldNum].moveFlag = -1;
                    // 各ステージの間の最後を取得する
                    if (stageSelectManagers[FieldNum].selectNum == 3) stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].select4to5Points.Length - 1;
                    else if (stageSelectManagers[FieldNum].selectNum == 2) stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].select3to4Points.Length - 1;
                    else if (stageSelectManagers[FieldNum].selectNum == 1) stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].select2to3Points.Length - 1;
                    else if (stageSelectManagers[FieldNum].selectNum == 0) stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].select1to2Points.Length - 1;
                    // 前のFeildへ移動
                    if (stageSelectManagers[FieldNum].selectNum == -1 && FieldNum != 0)
                    {
                        stageSelectManagers[FieldNum - 1].selectNum = stageSelectManagers[FieldNum - 1].selectPoints.Length - 1;
                        stageSelectManagers[FieldNum].selectNum = 0;
                        stageSelectManagers[FieldNum].moveFlag = -2;
                        stageSelectManagers[FieldNum].movePointsNum = stageSelectManagers[FieldNum].selectto1Points.Length - 1;
                    }
                    // 端で止める
                    if (stageSelectManagers[FieldNum].selectNum < 0)
                    {
                        stageSelectManagers[FieldNum].selectNum = 0;
                        stageSelectManagers[FieldNum].moveFlag = 0;
                        stageSelectManagers[FieldNum].movePointsNum = 0;
                    }
                }
            }
        }
    }

    public void InputEnter(InputAction.CallbackContext context)
    {
        if(context.started) enterFlag = true;
    }
}
