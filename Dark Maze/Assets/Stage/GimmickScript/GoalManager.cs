using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public bool isGoalFlag;
    bool onPlayerFlag;
    GameObject goalObject;
    GameObject playerObject;

    void Start()
    {
        goalObject = gameObject;
    }
    void Update()
    {
        if(onPlayerFlag)
        {
            
            float distance = Vector2.Distance(new Vector2(goalObject.transform.position.x, goalObject.transform.position.z), new Vector2(playerObject.transform.position.x, playerObject.transform.position.z));
            if (distance < 0.6f) isGoalFlag = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            playerObject = other.gameObject;
            onPlayerFlag = true;
        } 
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            playerObject = null;
            onPlayerFlag = false;
        }       
    }
}
