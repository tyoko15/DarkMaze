using UnityEngine;

public class BarrelManager : MonoBehaviour
{
    [Header("0.Heart 1.Arrow 2. "), SerializeField] int itemNum;
    [SerializeField] GameObject[] itemObjects;
    [SerializeField] Animator animator;
    public bool destroyFlag;
    [SerializeField] float destroyTime;
    float destroyTimer;
    void Start()
    {
        Instantiate(itemObjects[itemNum], new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
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
            destroyFlag = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }
    }
}
