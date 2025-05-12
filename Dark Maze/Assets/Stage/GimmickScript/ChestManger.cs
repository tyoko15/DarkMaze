using UnityEngine;

public class ChestManger : MonoBehaviour
{
    [SerializeField] GameObject chestObject;
    [SerializeField] GameObject player;
    [SerializeField] int itemNum;
    bool openFlag;
    [SerializeField] float openedTime;
    float openedTimer;
    void Start()
    {
        
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
