using UnityEngine;
using UnityEngine.InputSystem;

public class NewStageSelectManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    InputAction selectAction;
    [SerializeField] GameObject stageGroup;
    [SerializeField] GameObject[] stageImageObjects;

    [SerializeField] GameObject selectObject;
    [SerializeField] bool selectMoveFlag;
    
    [SerializeField] Vector2 selectVector;
    [SerializeField] int selectStageNum;
    [SerializeField] float selectMoveTime;
    float selectMoveTimer;

    bool changeStageFlag;
    [SerializeField] float changeStageTime;
    float changeStageTimer;

    void Start()
    {
        GetStageObject();
        selectAction = playerInput.actions.FindAction("Move");
    }

    void Update()
    {
        selectControl();
    }

    void GetStageObject()
    {
        stageImageObjects = new GameObject[stageGroup.transform.childCount * stageGroup.transform.GetChild(0).childCount];
        for (int i = 0; i < stageGroup.transform.childCount; i++) for (int j = 0; j < stageGroup.transform.GetChild(0).childCount; j++) stageImageObjects[i * stageGroup.transform.GetChild(i).childCount + j] = stageGroup.transform.GetChild(i).transform.GetChild(j).gameObject;
    }

    void selectControl()
    {
        if (selectAction.ReadValue<Vector2>().y > 0 && !selectMoveFlag)
        {
            selectVector.y = 1f;
            selectStageNum++;
            if (selectStageNum == 5) changeStageFlag = true;
            if (selectStageNum > 9) selectStageNum = 9;  
            selectMoveFlag = true;
        }
        else if (selectAction.ReadValue<Vector2>().y < 0 && !selectMoveFlag)
        {
            selectVector.y = -1f;
            selectStageNum--;
            if (selectStageNum < 0) selectStageNum = 0;
            selectMoveFlag = true;
        }
        //else if (selectAction.ReadValue<Vector2>().y == 0)
        //{
        //    selectVector.y = 0f;
        //}

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
                        float y = Mathf.Lerp(0f, -1000f, changeStageTimer / changeStageTime);
                        stageGroup.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(stageGroup.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x, y);
                        stageGroup.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(stageGroup.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x, y);
                    }
                    else if (selectVector.y == -1f)
                    {

                    }
                }
            }

        }
    }
}
