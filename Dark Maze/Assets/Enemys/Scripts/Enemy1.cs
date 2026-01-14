using UnityEngine;
using UnityEngine.AI;
using static GeneralStageManager;

public class Enemy1 : MonoBehaviour
{
    [SerializeField] GeneralStageManager gameManager;
    [SerializeField] int enemyStatus;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] wanderPoints;
    [SerializeField] int wanderPointNum;
    [SerializeField] Animator animator;
    [SerializeField] float trackingRange;
    [SerializeField] float trackingSpeed;
    [SerializeField] float wanderingSpeed;
    public bool moveFlag;
    public bool stanFlag;
    [SerializeField] GameObject confusionObject;
    [SerializeField] float stanTime;
    float stanTimer;
    bool trackFlag;

    [SerializeField] float enemyHP;
    [SerializeField] float enemyDamage;
    [SerializeField] bool isDamageFlag;
    [SerializeField] bool isAttackFlag;
    void Start()
    {
        
    }

    void Update()
    {
        switch (gameManager.status) 
        {
            case GameStatus.start: // start
                break;
            case GameStatus.play: // play
                EnemyControl();
                animator.speed = 1f;
                break;
            case GameStatus.stop: // stop
                agent.isStopped = true;
                animator.speed = 0f;
                break;
            case GameStatus.menu: // menu
                agent.isStopped = true;
                animator.speed = 0f; 
                break;
            case GameStatus.over: // over
                agent.isStopped = true;
                break;
            case GameStatus.clear: // clear
                agent.isStopped = true;
                break;
        }        
    }

    void EnemyControl()
    {
        // Stan’†
        if (stanFlag)
        {
            confusionObject.SetActive(true);
            agent.isStopped = true;
            animator.SetBool("Down", true);
            animator.SetBool("Move", false);
            animator.SetBool("Attack", false);
            if (stanTimer > stanTime)
            {
                stanFlag = false;
                stanTimer = 0;
            }
            else if (stanTimer < stanTime)
            {
                stanTimer += Time.deltaTime;
            }
        }
        else if (!stanFlag)
        {
            confusionObject.SetActive(false);
            animator.SetBool("Down", false);
            // Light“à
            if (moveFlag)
            {
                animator.SetBool("Move", true);
                // ’ÇÕ’†
                if (trackFlag)
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.destination = player.transform.position;
                    agent.speed = trackingSpeed;
                    float trackDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(transform.position.x, transform.position.z));
                    if (trackDistance < 1f)
                    {
                        animator.SetBool("Attack", true);
                        isAttackFlag = true;
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        isAttackFlag = false;
                    }
                }
                // œpœj’†
                else
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.destination = wanderPoints[wanderPointNum].transform.position;
                    agent.speed = wanderingSpeed;
                    float wanderPointsDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(wanderPoints[wanderPointNum].transform.position.x, 0, wanderPoints[wanderPointNum].transform.position.z));
                    if (wanderPointsDistance < 0.5f)
                    {
                        wanderPointNum++;
                        if (wanderPointNum == wanderPoints.Length) wanderPointNum = 0;
                        agent.destination = wanderPoints[wanderPointNum].transform.position;
                    }
                    float trackDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(transform.position.x, transform.position.z));
                    if (trackDistance < trackingRange) trackFlag = true;
                }
            }
            // LightŠO
            else if (!moveFlag)
            {
                agent.isStopped = true;
                animator.SetBool("Move", false);
                animator.SetBool("Attack", false);
            }
        }            
    }

    void EnemyDamage()
    {
        enemyHP--;
        isDamageFlag = false;
        if (enemyHP == 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" && isAttackFlag)
        {
            collision.gameObject.GetComponent<PlayerController>().damageFlag = true;
            collision.gameObject.GetComponent<PlayerController>().damageAmount = enemyDamage;
        }
        else if(collision.gameObject.tag == "Arrow" && !isDamageFlag)
        {
            isDamageFlag = true;
            EnemyDamage();
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Player" && isAttackFlag)
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (!playerController.damageFlag)
            {
                playerController.damageAmount = enemyDamage;
                playerController.damageFlag = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack" && !isDamageFlag) 
        {
            isDamageFlag = true; 
            EnemyDamage();            
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "LightRange") moveFlag = true;
        else if(other.gameObject.tag == "StanRange")
        {
            stanFlag = true;
            moveFlag = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "LightRange") moveFlag = false;
    }
}
