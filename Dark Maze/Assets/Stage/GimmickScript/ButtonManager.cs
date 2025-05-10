using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] public bool buttonFlag;
    [SerializeField] bool hideFlag;
    [SerializeField] bool somethingFlag;
    [SerializeField] bool senceFlag;
    int somethingNum;
    private void Start()
    {
        if(hideFlag) gameObject.SetActive(false);
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
