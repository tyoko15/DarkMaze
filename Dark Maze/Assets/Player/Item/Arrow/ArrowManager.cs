using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    [SerializeField] GameObject arrowObject;
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject spawerObject;
    [SerializeField] Rigidbody rb;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] public float speed;
    [SerializeField] Vector3 position;
    [SerializeField] Vector3 direction;
    [SerializeField] Vector3 rotate;
    public bool stopFlag;
    public bool hitFlag;
    [SerializeField] public float lostTime;
    float lostTimer;
    void Start()
    {
        playerObject = GameObject.Find("Player");
        spawerObject = GameObject.Find("Spawer");
        direction = (spawerObject.transform.position - playerObject.transform.position).normalized;
        rotate = new Vector3(playerObject.transform.eulerAngles.x, playerObject.transform.eulerAngles.y, playerObject.transform.eulerAngles.z);
        arrowObject.transform.rotation = Quaternion.Euler(rotate);
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
            Vector3 force = direction * z;

            //rb.AddForce(force, ForceMode.Impulse);
            rb.linearVelocity += force;
            if (lostTimer > lostTime)
            {
                Destroy(arrowObject);
            }
            else if (lostTimer < lostTime)
            {
                lostTimer += Time.deltaTime;
            }
        }
        else
        {
            arrowObject.transform.position = position;
            boxCollider.isTrigger = false;
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
        if (collision.gameObject.tag == "Button")
        {
            stopFlag = true;
            hitFlag = true;
        }
        else stopFlag = true;
        position = arrowObject.transform.position;
    }
}
