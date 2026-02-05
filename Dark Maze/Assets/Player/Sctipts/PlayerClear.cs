using UnityEngine;

public class PlayerClear : MonoBehaviour
{
    [SerializeField] GameObject stage1;
    [SerializeField] GameObject stage2;
    [SerializeField] Animator animator;
    void Start()
    {
        GameObject gameManager = GameObject.Find("GameManager").gameObject;
        if (gameManager != null)
        {
            GeneralStageManager manager = gameManager.GetComponent<GeneralStageManager>();
            int stage = manager.stageNum;
            int feildNum = stage / 5 + 1;
            int stageNum = stage % 5;
            //Debug.Log($"stage : {feildNum} - {stageNum}");
            if (feildNum == 1) stage1.SetActive(true);
            else if (feildNum == 2) stage2.SetActive(true);
        }
        else
        {
            stage1.SetActive(true);
        }
        animator.SetTrigger("Clear");
    }

    void Update()
    {
        
    }
}
