using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public bool isGoalFlag;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player") isGoalFlag = true;
    }
}
