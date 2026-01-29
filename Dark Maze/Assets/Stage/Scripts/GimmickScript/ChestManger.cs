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

    private Camera mainCamera;
    GameObject canvas;
    bool canvasFlag;
    void Start()
    {
        if (hideFlag) chestObject.SetActive(false);
        if (chestObject.transform.childCount > 1) chestTopObject = chestObject.transform.GetChild(0).gameObject;

        mainCamera = Camera.main;
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
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

    private void LateUpdate()
    {
        if (mainCamera == null && !canvasFlag) return;

        // ƒJƒƒ‰‚Ì•ûŒü‚ðŒü‚­
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z);
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Attack")
        {
            if (!openFlag) openFlag = true;
            player.GetComponent<PlayerController>().canItemFlag[itemNum] = true;            
        }
        if (collision.gameObject == player && canvasFlag) canvas.SetActive(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == player && canvasFlag) canvas.SetActive(false);
    }
}
