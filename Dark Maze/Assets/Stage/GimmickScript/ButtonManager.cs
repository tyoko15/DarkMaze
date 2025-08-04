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
    // Crystal‚ÌOn Off 0.On 1.Off
    [SerializeField] Color[] buttonCrystalColors;
    // 0.Crystal 1.Stand
    Material[] buttonMaterials;

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
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!senceFlag && activeFlag)
        {
            if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
            }
            else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
            {
                buttonFlag = true;
                somethingNum++;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (somethingFlag && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
        {
            buttonFlag = true;
        }
        else if (!somethingFlag && somethingNum == 0 && (collision.gameObject.tag == "Attack" || collision.gameObject.tag == "Arrow") && !buttonFlag)
        {
            buttonFlag = true;
            somethingNum++;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (senceFlag && (other.gameObject.tag == "Player" || other.gameObject.tag == "Box")) buttonFlag = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (senceFlag && (other.gameObject.tag == "Player" || other.gameObject.tag == "Box")) buttonFlag = false;
    }
}
