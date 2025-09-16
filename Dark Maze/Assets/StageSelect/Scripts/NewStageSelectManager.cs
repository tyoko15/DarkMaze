using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] int selectStageNum;
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
        }
        else
        {
            //dataManager.SaveData(dataManager.useDataNum, dataManager.name, totalClearNum);
            clearFieldNum = totalClearNum / 5;
            clearStageNum = totalClearNum % 5;
        }
        if (totalClearNum > 14) totalClearNum = 14;
        selectStageNum = totalClearNum;
        int n = selectStageNum / 5 - 1;
        for (int i = 0; i < stageGroup.transform.childCount; i++)
        {
            float y = -900f + (n * -1000f) + 200f * i;
            stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
        }
        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectStageNum].GetComponent<RectTransform>().anchoredPosition.y);
        stageNameText.GetComponent<TextMeshProUGUI>().text = $"{clearFieldNum + 1} - {clearStageNum + 1}";
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
            int fieldNum = selectStageNum / 5;
            int stageNum = selectStageNum % 5;
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
        selectControl();
    }

    void GetStageObject()
    {
        stageImageObjects = new GameObject[stageGroup.transform.childCount];
        for (int i = 0; i < stageGroup.transform.childCount; i++) stageImageObjects[i] = stageGroup.transform.GetChild(i).gameObject;
    }

    void selectControl()
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
                selectStageNum++;
                if (selectStageNum == 5) changeStageFlag = true;
                else if (selectStageNum == 10) changeStageFlag = true;
                if (selectStageNum > totalClearNum)
                {
                    selectStageNum = totalClearNum;
                    changeStageFlag = false;
                    selectMoveFlag = false;
                }
                else if (selectStageNum > 14)
                {
                    selectStageNum = 14;
                    selectMoveFlag = false;
                }
            }
            else if (selectAction.ReadValue<Vector2>().y < 0 && !selectMoveFlag)
            {
                selectMoveFlag = true;
                selectVector.y = -1f;
                selectStageNum--;
                if (selectStageNum == 4) changeStageFlag = true;
                else if (selectStageNum == 9) changeStageFlag = true;
                if (selectStageNum < 0)
                {
                    selectStageNum = 0;
                    selectMoveFlag = false;
                }
            }
            //else if (selectAction.ReadValue<Vector2>().y == 0)
            //{
            //    selectVector.y = 0f;
            //}
        }


        if (selectMoveFlag)
        {
            if (!changeStageFlag)
            {
                if (selectMoveTimer > selectMoveTime)
                {
                    selectMoveTimer = 0;
                    selectVector.y = 0f;
                    selectMoveFlag = false;

                    int fieldNum = selectStageNum / 5;
                    int stageNum = selectStageNum % 5;
                    stageNameText.GetComponent<TextMeshProUGUI>().text = $"{fieldNum + 1} - {stageNum + 1}";
                }
                else if (selectMoveTimer < selectMoveTime)
                {
                    selectMoveTimer += Time.deltaTime;
                    if (selectVector.y == 1f)
                    {
                        float y = Mathf.Lerp(stageImageObjects[selectStageNum - 1].GetComponent<RectTransform>().anchoredPosition.y, stageImageObjects[selectStageNum].GetComponent<RectTransform>().anchoredPosition.y, selectMoveTimer / selectMoveTime);
                        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, y);
                    }
                    else if (selectVector.y == -1f)
                    {
                        float y = Mathf.Lerp(stageImageObjects[selectStageNum + 1].GetComponent<RectTransform>().anchoredPosition.y, stageImageObjects[selectStageNum].GetComponent<RectTransform>().anchoredPosition.y, selectMoveTimer / selectMoveTime);
                        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, y);
                    }
                }
            }
            else if (changeStageFlag)
            {
                if (changeStageTimer > changeStageTime)
                {
                    changeStageFlag = false;
                    changeStageTimer = 0f;
                }
                else if (changeStageTimer < changeStageTime)
                {
                    changeStageTimer += Time.deltaTime;
                    if (selectVector.y == 1f)
                    {
                        int n = selectStageNum / 5 - 1;
                        for (int i = 0; i < stageGroup.transform.childCount; i++)
                        {
                            float y = Mathf.Lerp(100f + (n * -1000f) + 200f * i, -900f + (n * -1000f) + 200f * i, changeStageTimer / changeStageTime);
                            stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
                        }
                        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectStageNum - 1].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                    else if (selectVector.y == -1f)
                    {
                        int n = selectStageNum / 5;
                        for (int i = 0; i < stageGroup.transform.childCount; i++)
                        {
                            float y = Mathf.Lerp(-900f + (n * -1000f) + 200f * i, 100f + (n * -1000f) + 200f * i, changeStageTimer / changeStageTime);
                            stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(stageImageObjects[i].GetComponent<RectTransform>().anchoredPosition.x, y);
                        }
                        selectObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(selectObject.GetComponent<RectTransform>().anchoredPosition.x, stageImageObjects[selectStageNum + 1].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }
        }
    }

    void LoadClearStage()
    {
        totalClearNum = dataManager.data[dataManager.useDataNum].clearStageNum;
        clearFieldNum = totalClearNum / 5;
        clearStageNum = totalClearNum % 5;
        int dateNum = dataManager.useDataNum;
        int selectFieldNum = dataManager.data[dateNum].selectStageNum / 5;
        int selectStageNum = dataManager.data[dateNum].selectStageNum % 5;
    }
}
