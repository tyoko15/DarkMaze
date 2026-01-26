using UnityEngine;

public class MoveObjectManager : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject moveObject;
    [SerializeField] GameObject[] pointsObjects;
    int pointsNum;
    bool moveFlag = true;
    bool returnFlag;
    [SerializeField] float moveTime;
    float moveTimer;
    [SerializeField] float stopTime;
    float stopTimer;
    void Start()
    {
        
    }

    void Update()
    {
        moveControl();
    }

    void moveControl()
    {
        if (!moveFlag)
        {
            if(stopTime == 0)
            {

            }
            if(stopTimer > stopTime)
            {
                moveFlag = true;
                stopTimer = 0;
            }
            else if(stopTimer < stopTime)
            {
                stopTimer += Time.deltaTime;
            }
        }
        else
        {
            if(moveTimer == 0)
            {
                if (pointsObjects.Length - 1 == pointsNum && !returnFlag)
                {
                    pointsNum = pointsObjects.Length - 1;
                    returnFlag = true;
                }
                else if(pointsNum == 0 && returnFlag)
                {
                    returnFlag = false;
                    pointsNum = 0;
                }
            }
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                if (returnFlag) pointsNum--;
                else pointsNum++;
                moveFlag = false;
            }
            else if(moveTimer < moveTime)
            {
                if(returnFlag)
                {
                    moveTimer += Time.deltaTime;
                    float x = Mathf.Lerp(pointsObjects[pointsNum].transform.position.x, pointsObjects[pointsNum-1].transform.position.x, moveTimer / moveTime);
                    float z = Mathf.Lerp(pointsObjects[pointsNum].transform.position.z, pointsObjects[pointsNum-1].transform.position.z, moveTimer / moveTime);
                    moveObject.transform.position = new Vector3(x, moveObject.transform.position.y, z);
                }
                else
                {
                    moveTimer += Time.deltaTime;
                    float x = Mathf.Lerp(pointsObjects[pointsNum].transform.position.x, pointsObjects[pointsNum + 1].transform.position.x, moveTimer / moveTime);
                    float z = Mathf.Lerp(pointsObjects[pointsNum].transform.position.z, pointsObjects[pointsNum + 1].transform.position.z, moveTimer / moveTime);
                    moveObject.transform.position = new Vector3(x, moveObject.transform.position.y, z);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == playerObject) playerObject.transform.parent = moveObject.transform;
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject == playerObject) playerObject.transform.parent = moveObject.transform.parent.parent;
    }
}
