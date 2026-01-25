using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [Header("フェイドUIの取得")]
    [SerializeField] GameObject[] fadeObjects;
    [SerializeField] List<Sprite> fadeBannerSpriteList;
    [SerializeField] int fadeBannerSpriteNum;
    [SerializeField] float fadeObjectWidth;
    public bool fadeFlag;
    [Header("フェイドイン関連")]
    [SerializeField] float fadeInSecond;
    float fadeInTimer;
    public bool fadeInFlag;
    [Header("フェイドアウト関連")]
    [SerializeField] float fadeOutSecond;
    float fadeOutTimer;
    public bool fadeOutFlag;
    [Header("フェイド中間関連")]
    [SerializeField,Range(2f, 10f)] float fadeIntervalSecond;
    [SerializeField] float fadeIntervalTimer;
    public bool fadeIntervalFlag;
    [SerializeField] Image fadeBannerObject;
    [SerializeField] Color fadeBannerColor;
    [SerializeField] float fadeBannerFadeTimer;
    [SerializeField] float fadeBannerFadeInSecond;
    public bool fadeBannerFadeInFlag;
    [SerializeField] float fadeBannerFadeOutSecond;
    public bool fadeBannerFadeOutFlag;
    [Header("フェイド共通関連")]
    public bool endFlag; 
    float[] timer;
    float[] moveX;

    public bool titleFlag;
    public bool finishFlag;

    public bool saveDirection;
    [SerializeField] GameObject saveText;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        fadeObjectWidth = fadeObjects[0].GetComponent<RectTransform>().sizeDelta.x;
        fadeBannerObject.enabled = false;
        timer = new float[fadeObjects.Length];
        moveX = new float[fadeObjects.Length];
        fadeInTimer = fadeInSecond / fadeObjects.Length;
        fadeOutTimer = fadeInSecond / fadeObjects.Length; 
        for(int i = 0; i < fadeObjects.Length; i++)
        {
            //fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-fadeObjectWidth - i * 300f, fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
            timer[i] -= i * fadeInTimer;
        }
    }

    void Update()
    {
        if (fadeInFlag || fadeIntervalFlag || fadeOutFlag) fadeFlag = true;
        else if (!fadeInFlag && !fadeIntervalFlag && !fadeOutFlag) fadeFlag = false;
        fadeInTimer = fadeInSecond / fadeObjects.Length;
        fadeOutTimer = fadeInSecond / fadeObjects.Length; 
        if (fadeInFlag)
        {
            fadeOutFlag = false;
            fadeIntervalFlag = false;
            InFade();
        }
        else if (fadeIntervalFlag)
        {
            fadeInFlag = false;
            fadeOutFlag = false;
            IntervalFade();
            // セーブ演出
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (SceneManager.GetActiveScene().name == $"{i}-{j}") saveDirection = true;
                    if (saveDirection) break;
                }
            }
            if (SceneManager.GetActiveScene().name == $"StageSelect") saveDirection = true;
            if (saveDirection)
            {
                saveText.SetActive(true);
            }
            else saveText.SetActive(false);
        }
        else if (fadeOutFlag)
        {
            saveText.SetActive(false);
            fadeInFlag = false;
            fadeIntervalFlag = false;
            OutFade();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            finishFlag = true;
            fadeInFlag = true;
            DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
            if (dataManager != null)
            {
                int i  = dataManager.useDataNum;
                dataManager.SaveData(i, dataManager.data[i].playerName, dataManager.data[i].clearStageNum, dataManager.data[i].selectStageNum);
            }
        }
        if (finishFlag && endFlag)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        }
    }

    void InFade()
    {
        for (int i = 0; i < fadeObjects.Length; i++)
        {
            if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x == 0)
            {
                timer[i] = 0;
                moveX[i] = 0;
                if (i == fadeObjects.Length - 1)
                {
                    for(int j = 0; j < fadeObjects.Length; j++)
                    {
                        timer[j] -= j * fadeOutTimer;
                    }
                    //fadeInFlag = false;
                    endFlag = true;
                }
            }
            else if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x != 0)
            {
                timer[i] += Time.deltaTime;
                moveX[i] = Mathf.Lerp(-fadeObjectWidth - i * 300f, 0, timer[i] / fadeInTimer);
            }
            fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(moveX[i], fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    void OutFade()
    {
        for (int i = 0;i < fadeObjects.Length;i++)
        {
            if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x == -fadeObjectWidth - i * 300f)
            {
                timer[i] = 0;
                moveX[i] = -fadeObjectWidth - i * 300f;
                if (i == fadeObjects.Length - 1)
                {
                    for (int j = 0; j < fadeObjects.Length; j++)
                    {
                        timer[j] -= j * fadeInTimer;
                    }
                    //fadeOutFlag = false;
                    endFlag = true;
                }
            }
            else if (fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x != -fadeObjectWidth - i * 300f)
            {
                timer[i] += Time.deltaTime;
                moveX[i] = Mathf.Lerp(0, -fadeObjectWidth - i * 300f, timer[i] / fadeOutTimer);
            }
            fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(moveX[i], fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    void IntervalFade()
    {
        if (fadeIntervalTimer == 0)
        {
            fadeBannerFadeInFlag = true;
            fadeBannerObject.enabled = true;
            // フェイドバナーを交換する
            //fadeBannerObject.sprite = fadeBannerSpriteList[fadeBannerSpriteNum];
            fadeBannerSpriteNum++;
            if(fadeBannerSpriteNum == fadeBannerSpriteList.Count) fadeBannerSpriteNum = 0;
        }
        if (fadeIntervalTimer > fadeIntervalSecond - fadeBannerFadeOutSecond && !fadeBannerFadeOutFlag) fadeBannerFadeOutFlag = true;
        fadeIntervalTimer += Time.deltaTime;
        if (fadeBannerFadeInFlag)
        {
            fadeBannerFadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, fadeBannerFadeTimer / fadeBannerFadeInSecond);
            fadeBannerColor.a = a;
            fadeBannerObject.color = fadeBannerColor;
            if (fadeBannerFadeTimer > fadeBannerFadeInSecond)
            {
                fadeBannerColor.a = 1f;
                fadeBannerObject.color = fadeBannerColor; 
                fadeBannerFadeTimer = 0;
                fadeBannerFadeInFlag = false;
            }
        }
        else if (fadeBannerFadeOutFlag)
        {
            fadeBannerFadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeBannerFadeTimer / fadeBannerFadeOutSecond);
            fadeBannerColor.a = a;
            fadeBannerObject.color = fadeBannerColor;
            if (fadeBannerFadeTimer > fadeBannerFadeOutSecond)
            {
                fadeBannerColor.a = 0;
                fadeBannerObject.color = fadeBannerColor; 
                fadeBannerFadeTimer = 0;
                fadeBannerFadeOutFlag = false;
            }
        }

        if (fadeIntervalTimer > fadeIntervalSecond && !fadeBannerFadeOutFlag)
        {
            fadeBannerObject.enabled = false;
            fadeIntervalTimer = 0;
            endFlag = true;
        }
    }

    public void AfterFade()
    {
        for (int i = 0; i < fadeObjects.Length; i++) fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
    }
    public void BeforeFade()
    {
        for (int i = 0; i < fadeObjects.Length; i++) fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-fadeObjectWidth - i * 300f, fadeObjects[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
    }

    // 流れを管理
    public void FadeControl()
    {
        // フェイドイン終了時
        if (endFlag && fadeInFlag)
        {
            fadeInFlag = false;
            endFlag = false;
            fadeIntervalFlag = true;
        }
        // フェイドインターバル終了時
        else if (endFlag && fadeIntervalFlag)
        {
            fadeIntervalFlag = false;
            endFlag = false;
            fadeOutFlag = true;
        }
        // フェイドアウト終了時
        else if (endFlag && fadeOutFlag)
        {
            fadeOutFlag = false;
            endFlag = false;
        }
    }

}


