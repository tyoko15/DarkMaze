using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleButtonManager : MonoBehaviour
{
    [Header("スクリプトの取得")]
    [SerializeField] TitleManager titleManager;
    [SerializeField] DataManager dataManager;
    [Header("Buttonの取得")]
    [SerializeField] GameObject[] titleUIButtons;
    [SerializeField] GameObject[] selectDataUIButtons;
    [SerializeField] GameObject selectDataDecisionUI;
    [SerializeField] GameObject[] selectDataDecisionUIButtons;
    [SerializeField] GameObject[] createDataUIButtons;
    [SerializeField] GameObject createDataDecisionUI;
    [SerializeField] GameObject[] createDataDecisionUIButtons;
    [Header("Anime情報")]
    int animeFlag;
    bool decisionFlag;
    [SerializeField] float fadeInDecisionUITime;
    [SerializeField] float fadeInDecisionUITimer;
    [SerializeField] float fadeOutDecisionUITime;
    [SerializeField] float fadeOutDecisionUITimer;

    [Header("コントローラー情報")]
    [SerializeField] bool controllerFlag;
    [SerializeField] int selectNum;
    int oldSelectNum;
    //[SerializeField] int progressNum;

    bool oneFlag;
    bool EnterFlag;
    void Start()
    {
        var controllers = Input.GetJoystickNames();
        if (controllers[0] != "")
        {
            controllerFlag = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ControllerSelect();
        }
        else if (controllers[0] == "")
        {
            controllerFlag = false;
            Cursor.visible = true;
        }
    }
    void Update()
    {
        var controllers = Input.GetJoystickNames();
        if (controllers[0] != "")
        {
            controllerFlag = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ControllerSelect();
        }
        else if (controllers[0] == "")
        {
            controllerFlag = false; 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (animeFlag == 1) SelectDataDecisionUIAnime(selectDataDecisionUI, true);
        else if(animeFlag == 2) SelectDataDecisionUIAnime(selectDataDecisionUI, false);
        else if(animeFlag == 3) CreateDataDecisionUIAnime(createDataDecisionUI, true);
        else if(animeFlag == 4) CreateDataDecisionUIAnime(createDataDecisionUI, false);

    }
    // コントローラー用の選択関数
    void ControllerSelect()
    {
        if (titleManager.progressNum == 0)
        {
            if (oneFlag)
            {
                if (selectNum == 0)
                {
                    EnterStartButton();
                    if (oldSelectNum == 1) ExitEndButton();
                }
                if (selectNum == 1)
                {
                    EnterEndButton();
                    if (oldSelectNum == 0) ExitStartButton();
                }
                oldSelectNum = selectNum;
                oneFlag = false;
            }

            if (EnterFlag)
            {
                if (selectNum == 0) ClickStartButton();
                else if (selectNum == 1) ClickEndButton();
                selectNum = 0;
                oldSelectNum = 0;
                EnterFlag = false;
                oneFlag = true;
            }
        }
        else if (titleManager.progressNum == 1)
        {
            if (!decisionFlag)
            {
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterData1();
                        if (oldSelectNum == 1) ExitData2();
                        else if (oldSelectNum == 3) ExitSelectDataUIReturnButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterData2();
                        if (oldSelectNum == 0) ExitData1();
                        else if (oldSelectNum == 2) ExitData3();
                    }
                    else if (selectNum == 2)
                    {
                        EnterData3();
                        if (oldSelectNum == 1) ExitData2();
                        else if (oldSelectNum == 3) ExitSelectDataUIReturnButton();
                    }
                    else if (selectNum == 3)
                    {
                        EnterSelectDataUIReturnButton();
                        if (oldSelectNum == 0) ExitData1();
                        else if (oldSelectNum == 2) ExitData3();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }
                if (EnterFlag)
                {
                    if (selectNum == 0) ClickData1();
                    else if (selectNum == 1) ClickData2();
                    else if (selectNum == 2) ClickData3();
                    else if (selectNum == 3) ClickSelectDataUIReturnButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
            else
            {
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterSelectDataDecisionButton();
                        if (oldSelectNum == 1) ExitSelectDataReturnButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterSelectDataReturnButton();
                        if (oldSelectNum == 0) ExitSelectDataDecisionButton();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }
                if (EnterFlag)
                {
                    if (selectNum == 0) ClickSelectDataDecisionButton();
                    else if (selectNum == 1) ClickSelectDataReturnButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
        }
        else if(titleManager.progressNum == 2)
        {
            if (!decisionFlag)
            {
                if (oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterCreateDataReturnButton();
                        if (oldSelectNum == 1) titleManager.nameInputField.DeactivateInputField();
                        else if (oldSelectNum == 2) ExitCreateDataDecisionButton();
                    }
                    else if (selectNum == 1)
                    {
                        titleManager.nameInputField.ActivateInputField();
                        if (oldSelectNum == 0) ExitCreateDataReturnButton();
                        else if (oldSelectNum == 2) ExitCreateDataDecisionButton();
                    }
                    else if (selectNum == 2)
                    {
                        EnterCreateDataDecisionButton();
                        if (oldSelectNum == 0) ExitCreateDataReturnButton();
                        else if (oldSelectNum == 1) titleManager.nameInputField.DeactivateInputField();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }

                if (EnterFlag)
                {
                    if (selectNum == 0) ClickCreateDataReturnButton();
                    else if (selectNum == 2) ClickCreateDataDecisionButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
            else
            {
                if(oneFlag)
                {
                    if (selectNum == 0)
                    {
                        EnterCreateDataDecisionUIReturnButton();
                        if (oldSelectNum == 1) ExitCreateDataDecisionUIDecisionButton();
                    }
                    else if (selectNum == 1)
                    {
                        EnterCreateDataDecisionUIDecisionButton();
                        if (oldSelectNum == 0) ExitCreateDataDecisionUIReturnButton();
                    }
                    oldSelectNum = selectNum;
                    oneFlag = false;
                }
                if(EnterFlag)
                {
                    if (selectNum == 0) ClickCreateDataDecisionUIReturnButton();
                    else if (selectNum == 1) ClickCreateDataDecisionUIDecisionButton();
                    selectNum = 0;
                    oldSelectNum = 1;
                    EnterFlag = false;
                    oneFlag = true;
                }
            }
        }
    }

    // TitleUI
    void TextAnime(ref GameObject[] buttons, int i, bool flag)
    {
        if(flag) buttons[i].GetComponent<TextMeshProUGUI>().fontSize = 120f;
        if(!flag) buttons[i].GetComponent<TextMeshProUGUI>().fontSize = 100f;

    }
    void ButtonAnime(ref GameObject[] buttons, int i, bool flag)
    {

        if (flag) buttons[i].GetComponent<RectTransform>().localScale = new Vector2(1.1f, 1.1f);
        if(!flag) buttons[i].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);

    }
    public void EnterStartButton()
    {
        titleManager.enterTitleUINum = 1;
        TextAnime(ref titleUIButtons, 0, true);
    }
    public void EnterEndButton()
    {
        titleManager.enterTitleUINum = 2;
        TextAnime(ref titleUIButtons, 1, true);
    }
    public void ClickStartButton()
    {
        //titleManager.TitleUIActive(false);
        //titleManager.SelectDataUIActive(true);
        //TextAnime(ref titleUIButtons, 0, false);
        titleManager.fadeFlag = true;
        titleManager.progressNum = 1;
    }
    public void ClickEndButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }
    public void ExitStartButton()
    {
        TextAnime(ref titleUIButtons, 0, false);
    }
    public void ExitEndButton()
    {
        TextAnime(ref titleUIButtons, 1, false);
    }

    // SelectDataUI
    void SelectDataDecisionUIAnime(GameObject UI, bool flag)
    {
        if (flag)
        {
            if (fadeInDecisionUITimer == 0)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                titleManager.SelectDataDecisionUIActive(true);            
            }
            if (fadeInDecisionUITimer > fadeInDecisionUITime)
            {
                UI.transform.localScale = new Vector3(1f, 1f, 1f);
                fadeInDecisionUITimer = 0f;
                animeFlag = 0;
                decisionFlag = true;
            }
            else if (fadeInDecisionUITimer < fadeInDecisionUITime)
            {
                fadeInDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeInDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        else
        {
            if (fadeOutDecisionUITimer == 0) UI.transform.localScale = new Vector3(1f, 1f, 1f);
            if (fadeOutDecisionUITimer > fadeOutDecisionUITime)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                fadeOutDecisionUITimer = 0f;
                titleManager.SelectDataDecisionUIActive(false);
                animeFlag = 0;
            }
            else if (fadeOutDecisionUITimer < fadeInDecisionUITime)
            {
                fadeOutDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeOutDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
    public void EnterSelectDataUIReturnButton()
    {
        ButtonAnime(ref selectDataUIButtons, 3, true);
    }
    public void ClickSelectDataUIReturnButton()
    {
        //titleManager.SelectDataUIActive(false);
        //titleManager.TitleUIActive(true);
        titleManager.fadeFlag = true;
        titleManager.progressNum = 0;
    }
    public void ExitSelectDataUIReturnButton()
    {
        ButtonAnime(ref selectDataUIButtons, 3, false);
    }
    public void EnterData1()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 0, true);
    }
    public void EnterData2()
    {
        titleManager.enterSelectDataNum = 1;
        ButtonAnime(ref selectDataUIButtons, 1, true);
    }
    public void EnterData3()
    {
        titleManager.enterSelectDataNum = 2;
        ButtonAnime(ref selectDataUIButtons, 2, true);
    }
    public void ClickData1()
    {
        titleManager.selectDataNum = 0;
        animeFlag = 1;
        if (titleManager.dataNumFlag[0]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 0, false);
    }
    public void ClickData2()
    {
        titleManager.selectDataNum = 1;
        animeFlag = 1;
        if (titleManager.dataNumFlag[1]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 1, false);
    }
    public void ClickData3()
    {
        titleManager.selectDataNum = 2;
        animeFlag = 1;
        if (titleManager.dataNumFlag[2]) titleManager.selectDataDecisionText.text = "新しいデータを作成します。\n";
        else titleManager.selectDataDecisionText.text = "このデータで遊びますか?\n";
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = false;
        ButtonAnime(ref selectDataUIButtons, 2, false);
    }
    public void ExitData1()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 0, false);
    }
    public void ExitData2()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 1, false);
    }
    public void ExitData3()
    {
        titleManager.enterSelectDataNum = 0;
        ButtonAnime(ref selectDataUIButtons, 2, false);
    }
    public void EnterSelectDataDecisionButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 0, true);
    }
    public void EnterSelectDataReturnButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 1, true);
    }
    public void ClickSelectDataDecisionButton()
    {
        titleManager.SelectDataDecisionUIActive(false);
        //titleManager.SelectDataUIActive(false);
        if (titleManager.dataNumFlag[titleManager.selectDataNum])
        {
            //titleManager.CreateDataUIActive(true);
            TextAnime(ref selectDataDecisionUIButtons, 0, false);
            titleManager.progressNum = 2;
            titleManager.fadeFlag = true;
        }
        else
        {
            TextAnime(ref selectDataDecisionUIButtons, 0, false);
            titleManager.progressNum = 3;
            titleManager.fadeFlag = true;     // ステージ選択
        }
        dataManager.GetuseDataNum(titleManager.selectDataNum);
        decisionFlag = false;
    }
    public void ClickSelectDataReturnButton()
    {
        animeFlag = 2;
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = true;
        decisionFlag = false;
        //TextAnime(ref selectDataDecisionUIButtons, 1, false);
    }
    public void ExitSelectDataDecisionButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 0, false);
    }
    public void ExitSelectDataReturnButton()
    {
        TextAnime(ref selectDataDecisionUIButtons, 1, false);
    }

    // CreateDataUI
    void CreateDataDecisionUIAnime(GameObject UI, bool flag)
    {
        if (flag)
        {
            if (fadeInDecisionUITimer == 0)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                titleManager.CreateDataDecisionUIActive(true);
            }
            if (fadeInDecisionUITimer > fadeInDecisionUITime)
            {
                UI.transform.localScale = new Vector3(1f, 1f, 1f);
                fadeInDecisionUITimer = 0f;
                animeFlag = 0;
                decisionFlag = true;
            }
            else if (fadeInDecisionUITimer < fadeInDecisionUITime)
            {
                fadeInDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeInDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        else
        {
            if (fadeOutDecisionUITimer == 0) UI.transform.localScale = new Vector3(1f, 1f, 1f);
            if (fadeOutDecisionUITimer > fadeOutDecisionUITime)
            {
                UI.transform.localScale = new Vector3(0f, 0f, 0f);
                fadeOutDecisionUITimer = 0f;
                titleManager.CreateDataDecisionUIActive(false);
                animeFlag = 0;
            }
            else if (fadeOutDecisionUITimer < fadeInDecisionUITime)
            {
                fadeOutDecisionUITimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeOutDecisionUITimer / fadeInDecisionUITime);
                UI.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
    public void EnterCreateDataDecisionButton()
    {
        TextAnime(ref createDataUIButtons, 0, true);
    }
    public void EnterCreateDataReturnButton()
    {
        TextAnime(ref createDataUIButtons, 1, true);
    }
    public void ClickCreateDataDecisionButton()
    {
        animeFlag = 3;
        titleManager.createNameText.text = $"プレイヤー名: {titleManager.nameInputField.text}";
        //TextAnime(ref createDataUIButtons, 0, false);
    }
    public void ClickCreateDataReturnButton()
    {
        //titleManager.SelectDataUIActive(true);
        //titleManager.CreateDataUIActive(false);
        titleManager.progressNum = 1;
        titleManager.fadeFlag = true;
        for (int i = 0; i < selectDataUIButtons.Length; i++) selectDataUIButtons[i].GetComponent<EventTrigger>().enabled = true;
        //TextAnime(ref createDataUIButtons, 1, false);
        titleManager.nameInputField.text = null;
    }
    public void ExitCreateDataDecisionButton()
    {
        TextAnime(ref createDataUIButtons, 0, false);
    }
    public void ExitCreateDataReturnButton()
    {
        TextAnime(ref createDataUIButtons, 1, false);
    }
    public void EnterCreateDataDecisionUIDecisionButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 0, true);
    }
    public void EnterCreateDataDecisionUIReturnButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 1, true);
    }
    public void ClickCreateDataDecisionUIDecisionButton()
    {
        titleManager.CreateName();
        //titleManager.CreateDataDecisionUIActive(false);
        //titleManager.CreateDataUIActive(false);
        titleManager.fadeFlag = true;
        titleManager.progressNum = 3;
        decisionFlag = false;
        // ステージ選択
        SceneManager.LoadScene("StageSelect");
    }
    public void ClickCreateDataDecisionUIReturnButton()
    {
        animeFlag = 4;
        decisionFlag = false;
        //TextAnime(ref createDataDecisionUIButtons, 1, false);
    }
    public void ExitCreateDataDecisionUIDecisionButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 0, false);
        Debug.Log("a");
    }
    public void ExitCreateDataDecisionUIReturnButton()
    {
        TextAnime(ref createDataDecisionUIButtons, 1, false);
    }

    public void InputSelectNum(InputAction.CallbackContext context)
    {
        // TitleUI
        if(titleManager.progressNum == 0)
        {
            if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                selectNum++;
                if (selectNum > 1) selectNum = 0;
                oneFlag = true;
            }
            else if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                selectNum--;
                if (selectNum < 0) selectNum = 1;
                oneFlag = true;
            }
        }
        // SelectDataUI
        else if (titleManager.progressNum == 1)
        {
            if (!decisionFlag)
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 3) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 3;
                    oneFlag = true;
                }
            }
            else if(decisionFlag)
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 1) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 1;
                    oneFlag = true;
                }
            }
        }
        // CreateDataUI
        else if(titleManager.progressNum == 2)
        {
            if(!decisionFlag)
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 2) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 2;
                    oneFlag = true;
                }
            }
            else
            {
                if (context.started && context.ReadValue<Vector2>().x > 0)
                {
                    selectNum++;
                    if (selectNum > 1) selectNum = 0;
                    oneFlag = true;
                }
                else if (context.started && context.ReadValue<Vector2>().x < 0)
                {
                    selectNum--;
                    if (selectNum < 0) selectNum = 1;
                    oneFlag = true;
                }
            }
        }
    }
    public void InputEnter(InputAction.CallbackContext context)
    {
        if(context.started) EnterFlag = true;
    }
}
