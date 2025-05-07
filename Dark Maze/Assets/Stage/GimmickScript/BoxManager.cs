using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] GameObject player;
    [SerializeField] float moveTime;
    float moveTimer;
    int directionNum;
    bool lockFlag;
    Vector3 originPosition;
    void Start()
    {
        
    }

    void Update()
    {
        if (lockFlag)
        {
            MoveBox(directionNum);
            gameObject.tag = "Untagged";
        }
        else gameObject.tag = "Box";

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == player && !lockFlag)
        {
            Direction();
        }
    }
    void Direction()
    {
        float x = box.transform.position.x - player.transform.position.x;
        float z = box.transform.position.z - player.transform.position.z;
        // 左右からの衝突
        if(Mathf.Abs(x) > Mathf.Abs(z))
        {            
            if(x > 0) directionNum = 0; // 左            
            else if(x < 0) directionNum = 1; // 右
        }
        // 前後からの衝突
        else if(Mathf.Abs(x) < Mathf.Abs(z))
        {            
            if(z > 0) directionNum = 2; // 前            
            else if(z < 0) directionNum = 3; // 後
        }
        lockFlag = true;
    }

    void MoveBox(int direction)
    {
        // 左へ
        if (direction == 0)
        {
            if(moveTimer == 0) originPosition = box.transform.position;
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
            } 
            else if(moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float x = Mathf.Lerp(originPosition.x, originPosition.x + 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(x, box.transform.position.y, box.transform.position.z);
            }
        }
        // 右へ
        else if (direction == 1)
        {
            if (moveTimer == 0) originPosition = box.transform.position; 
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float x = Mathf.Lerp(originPosition.x, originPosition.x - 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(x, box.transform.position.y, box.transform.position.z);
            }
        }
        // 前へ
        else if (direction == 2)
        {
            if (moveTimer == 0) originPosition = box.transform.position; 
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float z = Mathf.Lerp(originPosition.z, originPosition.z + 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y, z);
            }
        }
        // 後へ
        else if (direction == 3)
        {
            if (moveTimer == 0) originPosition = box.transform.position; 
            if (moveTimer > moveTime)
            {
                moveTimer = 0;
                lockFlag = false;
            }
            else if (moveTimer < moveTime)
            {
                moveTimer += Time.deltaTime;
                float z = Mathf.Lerp(originPosition.z, originPosition.z - 2f, moveTimer / moveTime);
                box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y, z);
            }
        }
    }
}
