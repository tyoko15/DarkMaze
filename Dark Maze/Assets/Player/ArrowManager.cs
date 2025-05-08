using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    [SerializeField] GameObject arrowObject;
    [SerializeField] GameObject playerObject;
    [SerializeField] float speed;
    [SerializeField] Vector3 position;
    bool stopFlag;
    [SerializeField] float lostTime;
    float lostTimer;
    void Start()
    {
        playerObject = GameObject.Find("Player");
        position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y, playerObject.transform.position.z + 1f);
        arrowObject.transform.position = position;
    }

    void Update()
    {
        arrowControl();
    }

    void arrowControl()
    {
        if (!stopFlag)
        {
            float z = speed * Time.deltaTime;

            arrowObject.transform.position += new Vector3(0f, 0, z);
        }
        else
        {
            if (lostTimer > lostTime)
            {
                Destroy(arrowObject);
            }
            else if (lostTimer < lostTime)
            {
                lostTimer += Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Wall") stopFlag = true;      
    }
}
