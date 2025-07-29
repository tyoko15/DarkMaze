using UnityEngine;

public class ChestManger : MonoBehaviour
{
    [SerializeField] GameObject chestObject;
    GameObject chestTopObject;
    [SerializeField] GameObject player;
    [SerializeField] int itemNum;
    bool openFlag;
    [SerializeField] float openedTime;
    float openedTimer;
    [SerializeField] bool hideFlag;
    void Start()
    {
        if (hideFlag) chestObject.SetActive(false);
        if (chestObject.transform.childCount > 1) chestTopObject = chestObject.transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (!openFlag)
        {

        }
        else
        {
            if(openedTimer > openedTime)
            {
                openedTimer = 0;
                Destroy(chestObject);
            }
            else if(openedTimer < openedTime)
            {
                openedTimer += Time.deltaTime;
                if (chestTopObject != null)
                {
                    if (openedTimer > openedTime * 0.5f) chestTopObject.transform.eulerAngles = new Vector3(290f, 180f, 0f);
                    else if (openedTimer < openedTime * 0.5f)
                    {
                        float x = Mathf.Lerp(360f, 290f, openedTimer / (openedTime * 0.5f));
                        chestTopObject.transform.eulerAngles = new Vector3(x, 180f, 0f);
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Attack")
        {
            if (!openFlag) openFlag = true;
            player.GetComponent<PlayerController>().canItemFlag[itemNum] = true;            
        }
    }
}
