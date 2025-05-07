using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] public GameObject[] selectto1Points;
    [SerializeField] public GameObject[] select1to2Points;
    [SerializeField] public GameObject[] select2to3Points;
    [SerializeField] public GameObject[] select3to4Points;
    [SerializeField] public GameObject[] select4to5Points;
    [SerializeField] public GameObject[] select5toPoints;
    [SerializeField] public GameObject[] selectPoints;

    [SerializeField] public GameObject selectObject;
    [SerializeField] public int selectNum;
    public bool changeNextFlag;
    public bool changeReturnFlag;
    public int moveFlag;
    public float moveTimer;
    public int movePointsNum;

    [SerializeField] float moveSecond;
    void Start()
    {
        
    }

    void Update()
    {
        MoveSelect();
    }

    void MoveSelect()
    {
        if (moveFlag == 0)
        {
            selectObject.transform.position = selectPoints[selectNum].transform.position;
        }
        else if(moveFlag == 1)
        {
            moveTimer += Time.deltaTime;
            float x = 0;
            float z = 0;
            if(selectNum == 1)
            {
                x = Mathf.Lerp(select1to2Points[movePointsNum - 1].transform.position.x, select1to2Points[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (select1to2Points.Length - 1)));
                z = Mathf.Lerp(select1to2Points[movePointsNum - 1].transform.position.z, select1to2Points[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (select1to2Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select1to2Points.Length - 1)))
                {
                    movePointsNum++;
                    moveTimer = 0;
                    if (movePointsNum == select1to2Points.Length)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            else if(selectNum == 2)
            {
                x = Mathf.Lerp(select2to3Points[movePointsNum - 1].transform.position.x, select2to3Points[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (select2to3Points.Length - 1)));
                z = Mathf.Lerp(select2to3Points[movePointsNum - 1].transform.position.z, select2to3Points[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (select2to3Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select2to3Points.Length - 1)))
                {
                    movePointsNum++;
                    moveTimer = 0;
                    if (movePointsNum == select2to3Points.Length)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }            
            else if(selectNum == 3)
            {
                x = Mathf.Lerp(select3to4Points[movePointsNum - 1].transform.position.x, select3to4Points[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (select3to4Points.Length - 1)));
                z = Mathf.Lerp(select3to4Points[movePointsNum - 1].transform.position.z, select3to4Points[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (select3to4Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select3to4Points.Length - 1)))
                {
                    movePointsNum++;
                    moveTimer = 0;
                    if (movePointsNum == select3to4Points.Length)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }           
            else if(selectNum == 4)
            {
                x = Mathf.Lerp(select4to5Points[movePointsNum - 1].transform.position.x, select4to5Points[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (select4to5Points.Length - 1)));
                z = Mathf.Lerp(select4to5Points[movePointsNum - 1].transform.position.z, select4to5Points[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (select4to5Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select4to5Points.Length - 1)))
                {
                    movePointsNum++;
                    moveTimer = 0;
                    if (movePointsNum == select4to5Points.Length)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
        else if(moveFlag == 2)
        {
            moveTimer += Time.deltaTime;
            float x = Mathf.Lerp(select5toPoints[movePointsNum - 1].transform.position.x, select5toPoints[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (select5toPoints.Length - 1)));
            float z = Mathf.Lerp(select5toPoints[movePointsNum - 1].transform.position.z, select5toPoints[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (select5toPoints.Length - 1)));
            if(moveTimer > (float)(moveSecond / (select5toPoints.Length - 1)))
            {
                movePointsNum++;
                moveTimer = 0;
                if(movePointsNum == select5toPoints.Length)
                {
                    moveFlag = 0;
                    movePointsNum = 0;
                    // 次のステージへ
                    changeNextFlag = true;
                    selectObject.SetActive(false);
                }
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
        else if(moveFlag == 3)
        {
            selectObject.SetActive(true);
            moveTimer += Time.deltaTime;
            float x = Mathf.Lerp(selectto1Points[movePointsNum - 1].transform.position.x, selectto1Points[movePointsNum].transform.position.x, moveTimer / (float)(moveSecond / (selectto1Points.Length - 1)));
            float z = Mathf.Lerp(selectto1Points[movePointsNum - 1].transform.position.z, selectto1Points[movePointsNum].transform.position.z, moveTimer / (float)(moveSecond / (selectto1Points.Length - 1)));
            if(moveTimer > (float)(moveSecond / (selectto1Points.Length - 1)))
            {
                movePointsNum++;
                moveTimer = 0;
                if(movePointsNum == selectto1Points.Length)
                {
                    moveFlag = 0;
                    movePointsNum = 0;                    
                }
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
        else if(moveFlag == -1)
        {
            moveTimer += Time.deltaTime;
            //float x = Mathf.Lerp(selectPoints[selectNum + 1].transform.position.x, selectPoints[selectNum].transform.position.x, moveTime);
            //float z = Mathf.Lerp(selectPoints[selectNum + 1].transform.position.z, selectPoints[selectNum].transform.position.z, moveTime);
            float x = 0;
            float z = 0;
            if (selectNum == 3)
            {
                x = Mathf.Lerp(select4to5Points[movePointsNum].transform.position.x, select4to5Points[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (select4to5Points.Length - 1)));
                z = Mathf.Lerp(select4to5Points[movePointsNum].transform.position.z, select4to5Points[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (select4to5Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select4to5Points.Length - 1)))
                {
                    movePointsNum--;
                    moveTimer = 0;
                    if (movePointsNum == 0)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            else if (selectNum == 2)
            {
                x = Mathf.Lerp(select3to4Points[movePointsNum].transform.position.x, select3to4Points[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (select3to4Points.Length - 1)));
                z = Mathf.Lerp(select3to4Points[movePointsNum].transform.position.z, select3to4Points[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (select3to4Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select3to4Points.Length - 1)))
                {
                    movePointsNum--;
                    moveTimer = 0;
                    if (movePointsNum == 0)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            else if (selectNum == 1)
            {
                x = Mathf.Lerp(select2to3Points[movePointsNum].transform.position.x, select2to3Points[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (select2to3Points.Length - 1)));
                z = Mathf.Lerp(select2to3Points[movePointsNum].transform.position.z, select2to3Points[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (select2to3Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select2to3Points.Length - 1)))
                {
                    movePointsNum--;
                    moveTimer = 0;
                    if (movePointsNum == 0)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            else if (selectNum == 0)
            {
                x = Mathf.Lerp(select1to2Points[movePointsNum].transform.position.x, select1to2Points[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (select1to2Points.Length - 1)));
                z = Mathf.Lerp(select1to2Points[movePointsNum].transform.position.z, select1to2Points[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (select1to2Points.Length - 1)));
                if (moveTimer > (float)(moveSecond / (select1to2Points.Length - 1)))
                {
                    movePointsNum--;
                    moveTimer = 0;
                    if (movePointsNum == 0)
                    {
                        moveFlag = 0;
                        movePointsNum = 0;
                    }
                }
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
        else if(moveFlag == -2)
        {
            moveTimer += Time.deltaTime;
            float x = Mathf.Lerp(selectto1Points[movePointsNum].transform.position.x, selectto1Points[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (selectto1Points.Length - 1)));
            float z = Mathf.Lerp(selectto1Points[movePointsNum].transform.position.z, selectto1Points[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (selectto1Points.Length - 1)));
            if (moveTimer > (float)(moveSecond / (select1to2Points.Length - 1)))
            {
                movePointsNum--;
                moveTimer = 0;
                if(movePointsNum == 0)
                {
                    moveFlag = 0;
                    movePointsNum = 0;
                    // 前のステージへ
                    changeReturnFlag = true;
                    selectObject.SetActive(false);
                }
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
        else if(moveFlag == -3)
        {
            selectObject.SetActive(true);
            moveTimer += Time.deltaTime;
            float x = Mathf.Lerp(select5toPoints[movePointsNum].transform.position.x, select5toPoints[movePointsNum - 1].transform.position.x, moveTimer / (float)(moveSecond / (select5toPoints.Length - 1)));
            float z = Mathf.Lerp(select5toPoints[movePointsNum].transform.position.z, select5toPoints[movePointsNum - 1].transform.position.z, moveTimer / (float)(moveSecond / (select5toPoints.Length - 1)));
            if(moveTimer > (float)(moveSecond / (select5toPoints.Length - 1))) 
            {
                movePointsNum--;
                moveTimer = 0;
                if(movePointsNum == 0)
                {
                    moveFlag = 0;
                    moveTimer = 0;
                }                
            }
            selectObject.transform.position = new Vector3(x, selectObject.transform.position.y, z);
        }
    }
}
