using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] public bool buttonFlag;
    [SerializeField] bool hideFlag;
    bool activeFlag;
    [SerializeField] bool somethingFlag;
    [SerializeField] bool senceFlag;
    [SerializeField] public bool completeFlag;
    int somethingNum;

    bool moderingFlag;
    bool groundButtonFlag;
    GameObject buttonObject;
    GameObject buttonLight;
    // CrystalÇÃOn Off 0.On 1.Off
    [SerializeField] Color[] buttonCrystalColors;
    // 0.Crystal 1.Stand
    Material[] buttonMaterials;

    bool intervalFlag;
    float intervalTime = 1;
    float intervalTimer;

    private Camera mainCamera;
    GameObject canvas;
    bool canvasFlag;

    private void Start()
    {
        buttonObject = gameObject;
        if (buttonObject.GetComponent<MeshRenderer>())
        {             
            int materialCount = buttonObject.GetComponent<MeshRenderer>().materials.Length;
            buttonMaterials = new Material[materialCount];
            for (int i = 0; i < materialCount; i++) buttonMaterials[i] = buttonObject.GetComponent<MeshRenderer>().materials[i];
            if (buttonObject.transform.childCount != 0)
            {
                buttonLight = buttonObject.transform.GetChild(0).gameObject;
                buttonLight.SetActive(false);
            }
            if (materialCount == 1)
            {
                groundButtonFlag = true;
            }
            if (hideFlag)
            {
                for (int i = 0; i < materialCount; i++)
                {
                    Color c = buttonMaterials[i].color;
                    c.a = 0;
                    buttonMaterials[i].color = c;
                    buttonObject.GetComponent<MeshRenderer>().materials[i] = buttonMaterials[i];
                }
            }
            moderingFlag = true;
        }
        if (hideFlag) 
        {
            activeFlag = false;
            gameObject.SetActive(false);            
        }
        else activeFlag = true;

        mainCamera = Camera.main;
        int last = transform.childCount;
        canvas = transform.GetChild(last - 1).gameObject;
        canvas.SetActive(false);
        canvasFlag = true;
    }

    void Update()
    {
        if (moderingFlag)
        {
            if (!groundButtonFlag)
            {
                if (buttonFlag)
                {
                    buttonMaterials[0].color = buttonCrystalColors[0];
                    buttonLight.SetActive(true);
                }
                else
                {
                    buttonMaterials[0].color = buttonCrystalColors[1];
                    buttonLight.SetActive(false);
                }
            }         
        }
        if (!activeFlag)
        {
            if (buttonObject.GetComponent<MeshRenderer>().materials[1].color.a == 1f) activeFlag = true;
        }

        if (intervalFlag)
        {
            if (intervalTimer > intervalTime)
            {
                intervalTimer = 0;
                intervalFlag = false;
            }
            else intervalTimer += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // ÉJÉÅÉâÇÃï˚å¸Çå¸Ç≠
        Vector3 rotation = transform.position - mainCamera.transform.position;
        rotation = new Vector3(0f, rotation.y, rotation.z);
        canvas.transform.rotation = Quaternion.LookRotation(rotation);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!senceFlag && activeFlag)
        {
            if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
            else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                somethingNum++;
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!intervalFlag)
        {
            if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                intervalFlag = true;
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
            else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                somethingNum++;
                AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 0);
            }
        }

        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(true);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canvasFlag) canvas.SetActive(false);
    }
    private void OnTriggerStay(Collider other)
    {
        if (senceFlag && (other.gameObject.tag == "Box"))
        {
            buttonFlag = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (senceFlag && (other.gameObject.tag == "Box"))
        {
            buttonFlag = false;
        }
    }
}
