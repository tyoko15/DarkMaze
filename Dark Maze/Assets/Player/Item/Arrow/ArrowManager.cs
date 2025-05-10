using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    [SerializeField] GameObject arrowObject;
    [SerializeField] GameObject playerObject;
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] Vector3 position;
    [SerializeField] Vector3 rotate;
    bool stopFlag;
    [SerializeField] float lostTime;
    float lostTimer;
    void Start()
    {
        playerObject = GameObject.Find("Player");
        position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z);
        rotate = new Vector3(playerObject.transform.eulerAngles.x, playerObject.transform.eulerAngles.y, playerObject.transform.eulerAngles.z);
        arrowObject.transform.position = position;
        position = playerObject.transform.forward;
        arrowObject.transform.rotation = Quaternion.Euler(rotate);
        arrowObject.GetComponent<BoxCollider>().isTrigger = true;
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
            Vector3 force = position * z;

            rb.AddForce(force, ForceMode.Impulse);
            if (lostTimer > lostTime)
            {
                Destroy(arrowObject);
            }
            else if (lostTimer < lostTime)
            {
                lostTimer += Time.deltaTime;
            }        
            if(lostTimer > 0.15f) arrowObject.GetComponent<BoxCollider>().isTrigger = false;
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
        if (collision.gameObject.tag == "Wall")
        {
            stopFlag = true;
            lostTimer = 0;
            rb.isKinematic = true;
        }
        if(collision.gameObject.tag == "Arrow" && collision.gameObject.GetComponent<ArrowManager>().lostTimer > lostTimer) Destroy(collision.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall") 
        {
            stopFlag = true;
            lostTimer = 0;
            rb.isKinematic = true;
        } 
    }
}
