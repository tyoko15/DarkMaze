using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] public int itemNum;
    [SerializeField] GameObject itemObject;
    [SerializeField] float rotateTime;
    float rotateTimer;
    bool endFlag;
    void Start()
    {
        
    }

    void Update()
    {
        if (rotateTimer > rotateTime)
        {
            rotateTimer = 0;
        }
        else if (rotateTimer < rotateTime)
        {
            rotateTimer += Time.deltaTime;
            float y = Mathf.Lerp(0f, 360f, rotateTimer / rotateTime);
            itemObject.transform.eulerAngles = new Vector3(0f, y, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !endFlag)
        {
            GameObject player = GameObject.Find("Player");
            player.GetComponent<PlayerController>().GetItemControl(itemNum);
            Destroy(itemObject);
            endFlag = true;
        }
    }
}
