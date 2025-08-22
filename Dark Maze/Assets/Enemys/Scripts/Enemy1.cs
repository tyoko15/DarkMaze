using UnityEngine;
using UnityEngine.AI;

public class Enemy1 : MonoBehaviour
{
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
    bool trackFlag;

    [SerializeField] float enemyHP;    
    [SerializeField] bool isDamageFlag;
    void Start()
    {
        
    }

    void Update()
    {
        EnemyControl();
    }

    void EnemyControl()
    {
        // Lightì‡
        if (moveFlag)
        {
            animator.SetBool("Move", true);
            // í«ê’íÜ
            if (trackFlag)
            {
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                    agent.destination = player.transform.position;
                    agent.speed = trackingSpeed;
                }
                float trackDistance = Vector3.Distance(agent.transform.position, player.transform.position);
                if (trackDistance < 1f) animator.SetBool("Attack", true);
                else animator.SetBool("Attack", false);
                if (trackDistance > trackingRange) trackFlag = false;
            }
            // úpújíÜ
            else
            {
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                    agent.destination = wanderPoints[wanderPointNum].transform.position;
                }
                float wanderPointsDistance = Vector3.Distance(new Vector3(agent.transform.position.x, 0, agent.transform.position.z), new Vector3(wanderPoints[wanderPointNum].transform.position.x, 0, wanderPoints[wanderPointNum].transform.position.z));
                if (wanderPointsDistance < 0.5f)
                {
                    wanderPointNum++;
                    if (wanderPointNum == wanderPoints.Length) wanderPointNum = 0;                                       
                    agent.destination = wanderPoints[wanderPointNum].transform.position;
                }
                float trackDistance = Vector3.Distance(new Vector3(agent.transform.position.x, 0, agent.transform.position.z), new Vector3(player.transform.position.x, 0, player.transform.position.z));
                if (trackDistance < trackingRange) trackFlag = true;
            }
        }
        // LightäO
        else if (!moveFlag)
        {
            agent.isStopped = true;
            animator.SetBool("Move", false);
            animator.SetBool("Attack", false);
        }
        // StaníÜ
        else if (stanFlag)
        {
            agent.isStopped = true;
            animator.SetBool("Move", false);
            animator.SetBool("Attack", false);
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
        if(collision.gameObject.tag == "Player")
        {
            //collision.gameObject.GetComponent<PlayerController>().;
        }
        else if(collision.gameObject.tag == "Arrow" && !isDamageFlag)
        {
            isDamageFlag = true;
            EnemyDamage();
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
