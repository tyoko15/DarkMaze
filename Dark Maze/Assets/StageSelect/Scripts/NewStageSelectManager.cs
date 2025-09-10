using UnityEngine;
using UnityEngine.InputSystem;

public class NewStageSelectManager : MonoBehaviour
{
    [Header("Input関連")]
    [SerializeField] PlayerInput playerInput;
    InputAction selectAction;
    InputAction enterAciton;
    [Header("StageImage関連")]
    [SerializeField] GameObject stageGroup;
    GameObject[] stageImageObjects;
    [SerializeField] GameObject selectObject;
    [SerializeField] GameObject returnButton;


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

    void Start()
    {
        GetStageObject();
        selectObject.transform.GetChild(0).GetComponent<RectTransform>().localScale *= 1.2f;
        selectAction = playerInput.actions.FindAction("Move");
        enterAciton = playerInput.actions.FindAction("Enter");
    }

    void Update()
    {
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
        //if (enterAciton.started && selectReturnFlag)
        //{

        //} 
        if (!returnMoveFlag)
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

        if (!selectReturnFlag)
        {
            if (selectAction.ReadValue<Vector2>().y > 0 && !selectMoveFlag)
            {
                selectMoveFlag = true;
                selectVector.y = 1f;
                selectStageNum++;
                if (selectStageNum == 5) changeStageFlag = true;
                else if (selectStageNum == 10) changeStageFlag = true;
                if (selectStageNum > 14)
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
}
