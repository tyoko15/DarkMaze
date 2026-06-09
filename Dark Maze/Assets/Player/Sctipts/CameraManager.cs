using UnityEngine;

public class CameraManager : MonoBehaviour
{
    GameObject camera;
    bool performanceFlag;

    [SerializeField] float time;
    float timer;

    GameObject player;

    public GameObject targetObject { private get ; set; }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void MoveToTarget()
    {
        if (timer > time)
        {
            time = 0f;
            transform.position = targetObject.transform.position;
            transform.eulerAngles = targetObject.transform.eulerAngles;
            performanceFlag = false;
        }
        else
        {
            timer += Time.deltaTime;

            Vector3 playerPos = player.transform.position;
            Vector3 targetPos = player.transform.position;
            Vector3 playerAng = player.transform.eulerAngles;
            Vector3 targetAng = player.transform.eulerAngles;

            float px = Mathf.Lerp(playerPos.x, targetPos.x, timer / time);
            float py = Mathf.Lerp(playerPos.y, targetPos.y, timer / time);
            float pz = Mathf.Lerp(playerPos.z, targetPos.z, timer / time);
            float rx = Mathf.Lerp(playerAng.x, targetAng.x, timer / time);
            float ry = Mathf.Lerp(playerAng.y, targetAng.y, timer / time);
            float rz = Mathf.Lerp(playerAng.z, targetAng.z, timer / time);

            transform.position = new Vector3(px, py, pz);
            transform.eulerAngles = new Vector3(rx, ry, rz);
        }
    }

    void MoveToPlayer()
    {
        if (timer > time)
        {
            time = 0f;
            transform.position = player.transform.position;
            transform.eulerAngles = player.transform.eulerAngles;
            performanceFlag = false;
        }
        else
        {
            timer += Time.deltaTime;

            Vector3 playerPos = player.transform.position;
            Vector3 targetPos = player.transform.position;
            Vector3 playerAng = player.transform.eulerAngles;
            Vector3 targetAng = player.transform.eulerAngles;

            float px = Mathf.Lerp(targetPos.x, playerPos.x, timer / time);
            float py = Mathf.Lerp(targetPos.y, playerPos.y, timer / time);
            float pz = Mathf.Lerp(targetPos.z, playerPos.z, timer / time);
            float rx = Mathf.Lerp(targetAng.x, playerAng.x, timer / time);
            float ry = Mathf.Lerp(targetAng.y, playerAng.y, timer / time);
            float rz = Mathf.Lerp(targetAng.z, playerAng.z, timer / time);

            transform.position = new Vector3(px, py, pz);
            transform.eulerAngles = new Vector3(rx, ry, rz);
        }
    }
}
