using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // 0.start 1.play 2.stop 3.over 4.clear
    [Header("GameManger制御用")]
    [SerializeField] public int status;

    [Header("プレイヤーの基本情報")]
    [SerializeField] GameObject playerObject;
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 gravity;
    [SerializeField] float playerHorizontal;
    [SerializeField] float playerVertical;
    [SerializeField] float playerSpeed;
    [SerializeField] public int clearStageNum;
    bool errorFlag;
    [Header("HP情報")]
    [SerializeField] float maxPlayerHp;
    [SerializeField] public float playerHP;
    public float beforeHP;
    public float afterHP;
    public float damageAmount;
    public bool damageFlag;
    bool farstDamageFlag;
    [SerializeField] float damageTime;
    float damageTimer;
    public float recoveryAmount;
    public bool recoveryFlag;
    bool farstRecoveryFlag;
    [SerializeField] float recoveryTime;
    float recoveryTimer;
    [SerializeField] Image playerHpGauge;
    [SerializeField] Image playerDamageGauge;
    [SerializeField] Image playerRecoveryGauge;
    [SerializeField] TextMeshProUGUI playerHpText;
    [Header("Camera情報")]
    [SerializeField] GameObject mainCamera;
    [Header("プレイヤーのライト情報")]
    [SerializeField] Light playerLight;
    [SerializeField] GameObject lightRangeObject;
    [SerializeField] Image lightGauge;
    [SerializeField, Range(30f, 180f)] float playerLightRange;
    [SerializeField, Range(2.75f, 17.25f)] float lightRangeObjectRange;
    [Header("最拡大するまでにかかる秒数"), SerializeField] float lightSpreadTime;
    float lightSpreadTimer;
    [Header("最縮小するまでにかかる秒数"), SerializeField] float lightShrinkTime;
    float lightRangeMinRange;
    float lightShrinkTimer;
    [SerializeField] Image lightMaxIntervalTimerGauge;
    [SerializeField] float lightMaxIntervalTime;
    float lightMaxIntervalTimer;
    int onLight;
    [Header("プレイヤーの攻撃情報")]
    [SerializeField] bool attackFlag;
    [SerializeField] GameObject sword;
    [SerializeField] float attackTime;
    float attackTimer;
    [Header("プレイヤーのアイテム情報")]
    [SerializeField] int maxArrowCount;
    public int arrowCount;
    [Header("アイテム選択情報")]
    bool getItemFlag;
    [SerializeField] public bool[] canItemFlag;
    // 0.弓、1.縄、2.
    [SerializeField] GameObject[] itemSlots;
    [SerializeField] Sprite[] itemImageSprites;
    [SerializeField] GameObject itemSelect;
    [SerializeField] GameObject selectObject;
    [SerializeField] GameObject itemCountText;
    [SerializeField] int itemSelectNum;
    bool itemSelectFlag;
    bool startSelectFlag;
    bool endSelectFlag = true;
    [SerializeField] float itemTime;
    float[] itemTimer = new float[3];
    [Header("アイテム使用情報")]
    bool itemUseFlag;
    bool endUseFlag;
    Vector3 itemUseDirection;
    [Header("弓矢詳細")]
    [SerializeField] GameObject arrowSpawer;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject arrowObject;
    [SerializeField] bool arrowAnimeFlag;
    [Header("縄詳細")]
    [SerializeField] LayerMask woodLayer;
    bool betweenObjectFlag;
    GameObject betweenObject;
    [SerializeField] GameObject ropePrefab;
    [SerializeField] GameObject ropeObject;
    [SerializeField] GameObject[] ropeTargetObjects;
    [SerializeField] GameObject rangeRopeTargetObject;
    Vector3 originPlayerObjectPosition;
    Vector3 rangeRopeTargetPosition;
    [SerializeField] bool ropeMoveFlag;
    [SerializeField] float ropeMoveTime;
    float ropeMoveTimer;

    [Header("特殊効果")]
    // 砂の効果
    [SerializeField] GameObject[] respawnPositions;
    int playerPosiNum;
    [SerializeField] LayerMask sandLayer;
    bool onSandFlag;
    [SerializeField] float sandTime;
    float sandTimer;
    Vector3 originPosition;

    void Start()
    {
        rb.useGravity = false;
        playerHP = maxPlayerHp;
        itemCountText.SetActive(false);
        arrowCount = maxArrowCount;
    }

    void Update()
    {
        switch (status) 
        {
            case 0: // start
            break;
            case 1: // play
                PlayerControl();
                HPControl();
                PlayerLightControl();
                PlayerItemUseControl();
                CameraControl();
                PlayerAttackControl();
                if (arrowAnimeFlag) ArrowAnime();
                break;
            case 2: // stop
                PlayerAttackControl();
                break;
            case 3: // menu
                break;
            case 4: // over
                break;
            case 5: // clear
                break;
        }
        PlayerItemSelectControl();                 
    }

    // Player行動管理関数
    void PlayerControl()
    {
        if (playerObject.transform.position.y < -1f) playerObject.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 3f, playerObject.transform.position.z);
        // 地面が砂の場合
        Ray ray = new Ray(playerObject.transform.position, -playerObject.transform.up);
        RaycastHit hit;
        // 斜面の場合重力を下げる
        if (Physics.Raycast(ray, out hit, 0.01f))
        {
            if (playerObject.transform.position.y < 1.9f)
            {
                if (hit.normal.y < 0.8f) gravity.y = -3f;
                else gravity.y = -10f;
            }
            else gravity.y = -10f;
        }
        for (int i = 0; i < playerObject.transform.parent.childCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (playerObject.transform.parent.GetChild(i).name == $"EnterArea ({j})")
                {
                    playerPosiNum = j;
                }
            }
        }
        if (Physics.Raycast(ray, out hit, 0.001f) && !ropeMoveFlag && hit.collider.gameObject.layer == 7) onSandFlag = true;
        // 砂の演出
        if (onSandFlag)
        {
            playerObject.GetComponent<Collider>().enabled = false;
            if (sandTimer == 0)
            {
                originPosition = playerObject.transform.position;
                damageFlag = true;
                damageAmount = 5;
            }
            if (sandTimer > sandTime)
            {
                playerObject.GetComponent<Collider>().enabled = true;
                sandTimer = 0;
                onSandFlag = false;
                playerObject.transform.position = new Vector3(respawnPositions[playerPosiNum].transform.position.x, respawnPositions[playerPosiNum].transform.position.y + 3f, respawnPositions[playerPosiNum].transform.position.z);
            }
            else if(sandTimer < sandTime)
            {
                sandTimer += Time.deltaTime;
                float y = Mathf.Lerp(originPosition.y, originPosition.y -2.5f, sandTimer / sandTime);
                playerObject.transform.position = new Vector3(playerObject.transform.position.x, y, playerObject.transform.position.z);                
            }
        }

        if (!itemSelectFlag && !itemUseFlag && !endUseFlag && !onSandFlag)
        {
            // 行動
            if (arrowObject)
            {
                float arrowDistance = Vector2.Distance(new Vector2(arrowObject.transform.position.x, arrowObject.transform.position.z), new Vector2(playerObject.transform.position.x, playerObject.transform.position.z));
                if (arrowDistance > 3f || arrowObject.GetComponent<ArrowManager>().stopFlag)
                {
                    Vector3 playerPosition = playerObject.transform.position;
                    if (onLight == 1) playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime) * 0.1f;
                    else playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime);
                    playerObject.transform.position = playerPosition;
                }
            }
            else
            {
                Vector3 playerPosition = playerObject.transform.position;
                if (onLight == 1) playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime) * 0.1f;
                else playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime);
                playerObject.transform.position = playerPosition;
            }
            //進む方向に滑らかに向く。
            transform.forward = Vector3.Slerp(transform.forward, new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime), Time.deltaTime * 10f);
        }
        rb.AddForce(gravity, ForceMode.Acceleration);
    }
    void CameraControl()
    {
        if(canItemFlag[itemSelectNum] && itemSelectNum == 1)
        {
            if (rangeRopeTargetObject != null)
            {
                float x = (playerObject.transform.position.x + rangeRopeTargetObject.transform.position.x) / 2;
                float z = (playerObject.transform.position.z + rangeRopeTargetObject.transform.position.z) / 2;
                mainCamera.transform.position = new Vector3(x, playerObject.transform.position.y + 15f, z);
            }
            else mainCamera.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 10f, playerObject.transform.position.z - 2f);
        }
        else mainCamera.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 10f, playerObject.transform.position.z - 2f);
    }
    void HPControl()
    {
        if (damageFlag)
        {
            damageTimer += Time.deltaTime;
            if (!farstDamageFlag)
            {
                playerDamageGauge.enabled = true;
                beforeHP = playerHP;
                afterHP = playerHP - damageAmount;
                farstDamageFlag = true;
                float v = Mathf.InverseLerp(0f, maxPlayerHp, afterHP);
                playerHpGauge.fillAmount = v;
                playerHpText.text = $"HP : {afterHP} / {maxPlayerHp}";
            }
            if (damageTimer > damageTime)
            {
                playerHP = afterHP;
                beforeHP = 0f;
                afterHP = 0f;
                damageTimer = 0;
                damageFlag = false;
                farstDamageFlag = false;
            }
            else if (damageTimer < damageTime)
            {
                float v = Mathf.Lerp(beforeHP, afterHP, damageTimer / damageTime);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerDamageGauge.fillAmount = v;
            }
        }
        else if (recoveryFlag)
        {
            recoveryTimer += Time.deltaTime;
            if (!farstRecoveryFlag)
            {
                playerRecoveryGauge.enabled = true;
                farstRecoveryFlag = true;
                beforeHP = playerHP;
                afterHP = playerHP + recoveryAmount;
                if (afterHP > 100f)
                {
                    afterHP = 100f;
                    farstRecoveryFlag = false;
                    recoveryFlag = false;
                }
                float v = Mathf.InverseLerp(0f, maxPlayerHp, afterHP);
                playerRecoveryGauge.fillAmount = v;
                playerHpText.text = $"HP : {afterHP} / {maxPlayerHp}";
            }
            if (recoveryTimer > recoveryTime)
            {
                playerHP = afterHP;
                beforeHP = 0f;
                afterHP = 0f;
                recoveryTimer = 0;
                recoveryFlag = false;
                farstRecoveryFlag = false;
            }
            else if (recoveryTimer < recoveryTime)
            {
                float v = Mathf.Lerp(beforeHP, afterHP, recoveryTimer / recoveryTime);
                v = Mathf.InverseLerp(0f, maxPlayerHp, v);
                playerHpGauge.fillAmount = v;
            }
        }
        else if (!damageFlag && !recoveryFlag)
        {
            playerDamageGauge.enabled = false;
            playerRecoveryGauge.enabled = false;
            float v = Mathf.InverseLerp(0f, maxPlayerHp, playerHP);
            playerHpGauge.fillAmount = v;
            playerRecoveryGauge.fillAmount = v;
            playerHpText.text = $"HP : {playerHP} / {maxPlayerHp}";
        }
    }
    void PlayerLightControl()
    {
        // 最拡大した際、インターバル終了時
        if (onLight == 2 && lightMaxIntervalTimer > lightMaxIntervalTime)
        {
            onLight = 0;
            lightRangeMinRange = playerLightRange;
            lightMaxIntervalTimer = 0;
            lightMaxIntervalTimerGauge.fillAmount = 0f;
            lightRangeObject.tag = "LightRange";
        }
        // 最拡大した際、インターバル中
        else if (onLight == 2 && lightMaxIntervalTimer < lightMaxIntervalTime)
        {
            lightMaxIntervalTimer += Time.deltaTime;
            float normal = Mathf.InverseLerp(lightMaxIntervalTime, 0f, lightMaxIntervalTimer);
            lightMaxIntervalTimerGauge.fillAmount = normal;
        }
            // 最拡大
            if (onLight == 1 && playerLightRange == 180f)
        {
            playerLightRange = 180f;
            lightSpreadTimer = 0;
            onLight = 2;
            lightRangeObject.tag = "StanRange";
        }
        // 縮小中
        if (onLight == 0 && playerLightRange > 30f)
        {
            lightShrinkTimer += Time.deltaTime;
            float ratio = Mathf.InverseLerp(30f, 180f, lightRangeMinRange);
            lightShrinkTime = Mathf.Lerp(0f, lightSpreadTime, ratio);
            playerLightRange = Mathf.Lerp(lightRangeMinRange, 30f, lightShrinkTimer / lightShrinkTime);
            lightMaxIntervalTimerGauge.enabled = false;
        }
        // 拡大中
        else if (onLight == 1 && playerLightRange < 180f)
        {
            lightSpreadTimer += Time.deltaTime;
            playerLightRange = Mathf.Lerp(30f, 180f, lightSpreadTimer / lightSpreadTime);
            lightMaxIntervalTimerGauge.enabled = true;
            float nor = Mathf.InverseLerp(30f, 180f, playerLightRange);
            lightMaxIntervalTimerGauge.fillAmount = nor;
        }
        // 最縮小
        else if (onLight == 0 && playerLightRange == 30f)
        {
            playerLightRange = 30f;
            lightShrinkTimer = 0;
            lightSpreadTimer = 0;
            playerLight.spotAngle = 0;
        }
        playerLight.spotAngle = playerLightRange;
        
        float normalized = Mathf.InverseLerp(30f, 180f, playerLightRange);        
        lightGauge.fillAmount = normalized;
        lightRangeObject.transform.localScale = new Vector3(Mathf.Lerp(2.75f, 17.25f, normalized), 0.1f, Mathf.Lerp(2.75f, 17.25f, normalized));
    }
    void PlayerAttackControl()
    {
        if (attackFlag)
        {
            if (attackTimer == 0) sword.SetActive(true);
            if(attackTimer > attackTime)
            {
                attackTimer = 0;
                sword.SetActive(false);
                attackFlag = false;
            }
            else if(attackTimer < attackTime)
            {
                attackTimer += Time.deltaTime;
                float y = Mathf.Lerp(playerObject.transform.eulerAngles.y - 45f, playerObject.transform.eulerAngles.y + 45f, attackTimer / attackTime);
                sword.transform.rotation = Quaternion.Euler(0f, y, 0f);
            }
        }
        else sword.SetActive(false);
    }
    void PlayerItemSelectControl()
    {
        // 選択
        if (itemSelectFlag)
        {
            if (!startSelectFlag)
            {
                for(int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * -175f - 350f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        startSelectFlag = true;
                    }
                    else if (itemTimer[i] < itemTime)
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(-125f, i * -175f -350f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }            
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (i == itemSelectNum) itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(165f, 165f);
                else itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
            }
        }
        else
        {
            if (!endSelectFlag)
            {
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-125f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        endSelectFlag = true;
                        itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
                    }
                    else if (itemTimer[i] < itemTime)
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(i * -175f - 350f, -125f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }
        }
    }
    void PlayerItemUseControl()
    {
        if (!itemUseFlag) itemUseDirection = playerObject.transform.forward;
        if(getItemFlag)
        {
            if(clearStageNum == 1) canItemFlag[0] = true;
            else if(clearStageNum == 6) canItemFlag[1] = true;
        }
        // 弓の使用可
        if (clearStageNum > 6)
        {
            for (int i = 0; i < 2; i++) canItemFlag[i] = true;
        }
        else if (clearStageNum > 1)
        {
            canItemFlag[0] = true;
        }
        // 投げ縄の使用可
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[i];
            }
            else if (!canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[3];
            }
            if (itemSelectNum == i) selectObject.GetComponent<RectTransform>().anchoredPosition = itemSlots[i].GetComponent<RectTransform>().anchoredPosition;
        }
        // 使用
        // 弓
        if (canItemFlag[itemSelectNum] && itemSelectNum == 0)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[0];
            itemCountText.SetActive(true);
            TextMeshProUGUI countText = itemCountText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            countText.text = arrowCount.ToString("00");
            if (arrowCount == maxArrowCount) countText.color = new Color(0f, 0.75f, 0f);
            else if (arrowCount == 0) countText.color = Color.red;
            else countText.color = Color.black;

            if (itemUseFlag && !arrowAnimeFlag && arrowCount > 0)
            {
                Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                RaycastHit hit;
                if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 30f))
                {
                    if (!hit.collider.isTrigger)
                    {
                        if (hit.collider.tag == "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                            betweenObjectFlag = false;
                        }
                        else if (hit.collider.tag != "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                            betweenObjectFlag = true;
                            arrowAnimeFlag = false;
                        }
                    }
                    else
                    {
                        if (hit.collider.tag != "Button")
                        {
                            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                            betweenObjectFlag = false;
                            if (betweenObject == null)
                            {
                                betweenObject = hit.collider.gameObject;
                                betweenObject.SetActive(false);
                                arrowAnimeFlag = false;
                            }
                            else
                            {
                                betweenObject.SetActive(true);
                                betweenObject = hit.collider.gameObject;
                                betweenObject.SetActive(false);
                                arrowAnimeFlag = false;
                            }
                        }
                    }
                }
                Quaternion rotation = Quaternion.LookRotation(itemUseDirection);
                //進む方向に滑らかに向く。
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            if (endUseFlag && arrowCount > 0)
            {
                arrowCount--;
                Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                RaycastHit hit;
                if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 30f))
                {
                    if (!hit.collider.isTrigger && hit.collider.tag == "Button")
                    {
                        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                        betweenObjectFlag = false;
                        arrowAnimeFlag = true;
                    }
                }
                if (betweenObject)
                {
                    betweenObject.SetActive(true);
                    betweenObject = null;
                    betweenObjectFlag = false;
                }
                arrowObject = Instantiate(arrowPrefab, arrowSpawer.transform.position, Quaternion.identity);
                if(arrowAnimeFlag)
                {
                    arrowObject.GetComponent<ArrowManager>().speed = 10f;
                    arrowObject.GetComponent<ArrowManager>().lostTime = 10f;
                }
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        // 縄
        else if (canItemFlag[itemSelectNum] && itemSelectNum == 1)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[1];
            itemCountText.SetActive(false);
            if (itemUseFlag)
            {
                for (int i = 0; i < ropeTargetObjects.Length; i++)
                {
                    Ray ray = new Ray(new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z), playerObject.transform.forward);
                    RaycastHit hit;
                    // playerとwoodの間にobjectがないか検知
                    if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 15f))
                    {
                        if (!hit.collider.gameObject.GetComponent<Collider>().isTrigger)
                        {
                            if (hit.collider.gameObject.tag != "Wood")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                                betweenObjectFlag = true;
                            }
                            else if (hit.collider.gameObject.tag == "Wood")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                                betweenObjectFlag = false;
                            }
                        }
                        else
                        {
                            if (hit.collider.gameObject.tag != "Wood" && hit.collider.gameObject.tag != "Rope")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red);
                                betweenObjectFlag = true;
                                if (betweenObject == null)
                                {
                                    betweenObject = hit.collider.gameObject;
                                    betweenObject.SetActive(false);
                                }
                                else
                                {
                                    betweenObject.SetActive(true);
                                    betweenObject = hit.collider.gameObject;
                                    betweenObject.SetActive(false);
                                }
                            }
                            else if (hit.collider.gameObject.tag == "Wood" || hit.collider.gameObject.tag == "Rope")
                            {
                                Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);
                                betweenObjectFlag = false;
                            }
                        }
                    }
                    // woodを検知
                    if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, 30f, woodLayer) && !betweenObjectFlag)
                    {
                        if (hit.collider.gameObject == ropeTargetObjects[i])
                        {
                            rangeRopeTargetObject = ropeTargetObjects[i];
                            if (ropeObject == null)
                            {
                                ropeObject = Instantiate(ropePrefab, playerObject.transform.parent.position, Quaternion.identity);
                                ropeObject.transform.position = new Vector3(ropeObject.transform.position.x, ropeObject.transform.position.y, ropeObject.transform.position.z);
                                ropeObject.transform.parent = playerObject.transform;
                            }
                            ropeMoveFlag = true;
                        }
                    }
                    else
                    {
                        if (ropeObject != null) Destroy(ropeObject);
                        rangeRopeTargetObject = null;
                        ropeObject = null;
                        ropeMoveFlag = false;
                    }
                }              
                if (rangeRopeTargetObject != null)
                {
                    rangeRopeTargetPosition = rangeRopeTargetObject.transform.position;
                    ropeObject.transform.position = rangeRopeTargetPosition;
                    //playerObject.transform.LookAt(new Vector3(rangeRopeTargetPosition.x, rangeRopeTargetPosition.y-1f, rangeRopeTargetPosition.z));
                }
                Quaternion rotation = Quaternion.LookRotation(itemUseDirection);
                //進む方向に滑らかに向く。
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            else if(!itemUseFlag && betweenObject)
            {
                betweenObject.SetActive(true);
                betweenObject = null;
            }
            if (endUseFlag)
            {
                itemUseFlag = false;
                if (betweenObject != null)
                {
                    betweenObject.SetActive(true);
                    betweenObject = null;
                }
                if (ropeMoveFlag)
                {
                    if (ropeMoveTimer == 0)
                    {
                        originPlayerObjectPosition = playerObject.transform.position;
                        float x = rangeRopeTargetPosition.x - playerObject.transform.position.x;
                        float z = rangeRopeTargetPosition.z - playerObject.transform.position.z;
                        if (Mathf.Abs(x) > Mathf.Abs(z)) // 左右
                        {
                            if (x > 0) // 右
                            {
                                rangeRopeTargetPosition.x -= 1.5f;
                            }
                            else if (x < 0) // 左
                            {
                                rangeRopeTargetPosition.x += 1.5f;
                            }
                        }
                        else if (Mathf.Abs(x) < Mathf.Abs(z)) // 前後
                        {
                            if (z > 0) // 前
                            {
                                rangeRopeTargetPosition.z -= 1.5f;
                            }
                            else if (z < 0) // 後
                            {
                                rangeRopeTargetPosition.z += 1.5f;
                            }
                        }
                    }
                    if (ropeMoveTimer > ropeMoveTime)
                    {
                        ropeMoveTimer = 0;
                        ropeMoveFlag = false;
                    }
                    else if (ropeMoveTimer < ropeMoveTime)
                    {
                        ropeObject.transform.position = rangeRopeTargetObject.transform.position;
                        float x = Mathf.Lerp(originPlayerObjectPosition.x, rangeRopeTargetPosition.x, ropeMoveTimer / ropeMoveTime);
                        float z = Mathf.Lerp(originPlayerObjectPosition.z, rangeRopeTargetPosition.z, ropeMoveTimer / ropeMoveTime);
                        playerObject.transform.position = new Vector3(x, originPlayerObjectPosition.y, z);
                        ropeMoveTimer += Time.deltaTime;
                    }
                }
                else
                {
                    Destroy(ropeObject);
                    rangeRopeTargetObject = null;
                    ropeObject = null;
                    itemUseFlag = false;
                    endUseFlag = false;
                    betweenObjectFlag = false;
                }
            }
        }
        else if (canItemFlag[itemSelectNum] && itemSelectNum == 2)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[2];
            itemCountText.SetActive(false);
            if (itemUseFlag)
            {

            }
            if (endUseFlag)
            {
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        else
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[3];
            itemUseFlag = false;
            endUseFlag = false;
        }
    }

    public void GetItemControl(int itemNum)
    {
        Debug.Log($"Get{itemNum}");
        // Heart
        if (itemNum == 0)
        {
            recoveryFlag = true;
        }
        // Arrow
        else if (itemNum == 1)
        {
            arrowCount += 5;
            if (arrowCount > maxArrowCount) arrowCount = maxArrowCount;
        }
    }
    void ArrowAnime()
    {
        if (!arrowObject)
        {
            status = 1;
            arrowAnimeFlag = false;
        }
        else if(arrowObject)
        {
            status = 2;
            Vector3 position = new Vector3(arrowObject.transform.position.x, arrowObject.transform.position.y + 10f, arrowObject.transform.position.z - 2f);
            mainCamera.transform.position = position;
            if (arrowObject.GetComponent<ArrowManager>().hitFlag) arrowObject.GetComponent<ArrowManager>().lostTime = 1f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            float x = playerObject.transform.position.x - collision.gameObject.transform.position.x;
            float y = playerObject.transform.position.y - collision.gameObject.transform.position.y;
            float z = playerObject.transform.position.z - collision.gameObject.transform.position.z;
            if (y < 0.5f)
            {
                if (Mathf.Abs(x) > Mathf.Abs(z)) playerHorizontal = 0f;
                else if (Mathf.Abs(x) < Mathf.Abs(z)) playerVertical = 0f;
            }
        }
    }

    // InputAction管理関数
    public void InputPlayerControl(InputAction.CallbackContext context)
    {
        if(!itemSelectFlag && (!itemUseFlag || !endUseFlag))
        {
            if (context.ReadValue<Vector2>().x == 0) playerHorizontal = 0;
            if (context.ReadValue<Vector2>().x > 0) playerHorizontal = context.ReadValue<Vector2>().x;
            else if (context.ReadValue<Vector2>().x < 0) playerHorizontal = context.ReadValue<Vector2>().x;
            if (context.ReadValue<Vector2>().y == 0) playerVertical = 0;
            if (context.ReadValue<Vector2>().y > 0) playerVertical = context.ReadValue<Vector2>().y;
            else if (context.ReadValue<Vector2>().y < 0) playerVertical = context.ReadValue<Vector2>().y;
        }
    }

    public void InputPlayerLightButton(InputAction.CallbackContext context)
    {
        if (context.started && playerLightRange == 30) onLight = 1;
        else if (context.canceled && onLight == 1)
        {
            lightRangeMinRange = playerLightRange;
            onLight = 0;
        }
    }

    public void InputPlayerAttackButton(InputAction.CallbackContext context)
    {
        if(context.started && status == 1 && !attackFlag) attackFlag = true;
    }

    public void InputPlayerSelectItemButton(InputAction.CallbackContext context)
    {
        if (context.started && status == 1 && !itemUseFlag && !endUseFlag)
        {
            itemSelectFlag = true;
            endSelectFlag = false;
        }
        if (context.canceled) 
        {
            itemSelectFlag = false; 
            startSelectFlag = false;
        }
    }

    public void InputPlayerSelectItemControl(InputAction.CallbackContext context)
    {
        if (itemSelectFlag)
        {
            if (context.ReadValue<Vector2>().x == 0) playerHorizontal = 0;
            if (context.started && context.ReadValue<Vector2>().x > 0) itemSelectNum--;
            else if (context.started && context.ReadValue<Vector2>().x < 0) itemSelectNum++;
            if (itemSelectNum > 2) itemSelectNum = 0;
            if (itemSelectNum < 0) itemSelectNum = 2;
        }
    }
    public void InputPlayerUseItemButton(InputAction.CallbackContext context)
    {
        if(context.started && status == 1 && !itemSelectFlag) itemUseFlag = true;
        if (context.canceled && status == 1 && !itemSelectFlag) endUseFlag = true;
    }
    public void InputPlayerUseItemControl(InputAction.CallbackContext context)
    {
        if(itemUseFlag && (context.ReadValue<Vector2>().x != 0 || context.ReadValue<Vector2>().y != 0)) itemUseDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
    }
}
