using TMPro;
using UnityEngine;

public class TitleDebugManager : MonoBehaviour
{
    public static TitleDebugManager instance;

    [SerializeField] TitleManager titleManager;
    [SerializeField] TitleButtonManager titleButtonManager;

    [SerializeField] FadeManager fadeManager;

    bool debugFlag;
    [SerializeField] GameObject debugUI;
    TextMeshProUGUI[] debugTexts;
    TextMeshProUGUI[] openDebugTexts;

    private void Awake()
    {
         instance = this;
    }

    void Start()
    {
        if (GameObject.Find("FadeManager") != null) fadeManager = GameObject.Find("FadeManager").GetComponent<FadeManager>();   
        debugTexts = new TextMeshProUGUI[debugUI.transform.GetChild(0).transform.childCount];
        openDebugTexts = new TextMeshProUGUI[debugUI.transform.GetChild(1).transform.childCount];
        for (int i = 0; i < debugUI.transform.GetChild(0).transform.childCount; i++)
        {
            debugTexts[i] = debugUI.transform.GetChild(0).GetChild(i).GetComponent<TextMeshProUGUI>();
        }
        for (int i = 0; i < debugUI.transform.GetChild(1).transform.childCount; i++)
        {
            openDebugTexts[i] = debugUI.transform.GetChild(1).GetChild(i).GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (GameObject.Find("FadeManager") != null && fadeManager == null) fadeManager = GameObject.Find("FadeManager").GetComponent<FadeManager>();

        DisplayDebug();
    }

    void DisplayDebug()
    {
        if (Input.GetKeyDown(KeyCode.L)) debugFlag = (!debugFlag) ? true : false;


        debugUI.SetActive(debugFlag);

        debugTexts[0].text = $"SelectNum : {titleButtonManager.GetSelectNum()}"; 
        debugTexts[1].text = $"SelectVector : {titleButtonManager.GetSelectVector()}"; 
        debugTexts[2].text = $"ProgressNum : {titleManager.progressNum}";
        if (GameObject.Find("FadeManager") != null) debugTexts[3].text = $"FadeFlag : {fadeManager.fadeFlag}";
        debugTexts[4].text = $"IntervalFlag : {titleButtonManager.GetIntervalFlag()}";
        debugTexts[5].text = $"Title : {titleManager.progressNum == 0 && !fadeManager.fadeFlag && titleButtonManager.GetIntervalFlag()}";
    }

    public TextMeshProUGUI OpenDebugTexts(int i)
    {
        return openDebugTexts[i]; 
    }
}
