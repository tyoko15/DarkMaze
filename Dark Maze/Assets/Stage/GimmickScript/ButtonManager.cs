using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] public bool buttonFlag;
    [SerializeField] bool hideFlag;
    [SerializeField] bool somethingFlag;
    [SerializeField] bool senceFlag;
    [SerializeField] public bool completeFlag;
    int somethingNum;

    bool moderingFlag;
    GameObject buttonObject;
    GameObject buttonLight;
    // Crystal‚ÌOn Off 0.On 1.Off
    [SerializeField] Color[] buttonCrystalColors;
    // 0.Stand 1.Crystal
    Material[] buttonMaterials;

    private void Start()
    {
        if(hideFlag) gameObject.SetActive(false);
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
            moderingFlag = true;
        }
    }

    void Update()
    {
        if (moderingFlag)
        {
            if (buttonFlag) 
            {
                buttonMaterials[1].color = buttonCrystalColors[0];
                buttonLight.SetActive(true);
            }            
            else            
            {
                buttonMaterials[1].color = buttonCrystalColors[1];
                buttonLight.SetActive(false);
            }            
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!senceFlag)
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
