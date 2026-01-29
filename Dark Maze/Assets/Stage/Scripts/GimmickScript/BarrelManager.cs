using UnityEngine;

public class BarrelManager : MonoBehaviour
{
    [Header("0.Heart 1.Arrow 2. "), SerializeField] int itemNum;
    [SerializeField] GameObject[] itemObjects;
    GameObject itemObject;
    [SerializeField] Animator animator;
    public bool destroyFlag;
    [SerializeField] float destroyTime;
    float destroyTimer;

    private Camera mainCamera;
    GameObject canvas;
    bool canvasFlag;
    void Start()
    {
        mainCamera = Camera.main;
        itemObject = Instantiate(itemObjects[itemNum], new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
        itemObject.transform.parent = transform.parent;
        itemObject.SetActive(false);
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
    }

    void Update()
    {
        if (destroyFlag)
        {
            animator.SetBool("Destroy", destroyFlag);            
            if (destroyTimer > destroyTime)
            {
                Destroy(gameObject);
                gameObject.GetComponent<Collider>().isTrigger = true;
            }
            else if (destroyTimer < destroyTime)
            {
                destroyTimer += Time.deltaTime;
            }
        }

        if (!canvasFlag) canvas.SetActive(false);
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // ƒJƒƒ‰‚Ì•ûŒü‚ðŒü‚­
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z);
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Attack")
        {
            itemObject.SetActive(true);
            destroyFlag = true;
            canvasFlag = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(true);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(false);
    }
}
