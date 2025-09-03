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
    void Start()
    {
        itemObject = Instantiate(itemObjects[itemNum], new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
        itemObject.transform.parent = transform.parent;
        itemObject.SetActive(false);
    }

    void Update()
    {
        if (destroyFlag)
        {
            animator.SetBool("Destroy", destroyFlag);            
            if (destroyTimer > destroyTime)
            {
                Destroy(gameObject);
            }
            else if (destroyTimer < destroyTime)
            {
                destroyTimer += Time.deltaTime;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Attack")
        {
            itemObject.SetActive(true);
            destroyFlag = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }
    }
}
