using UnityEngine;

public class PortfolioPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;

    void Start()
    {
        animator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeed", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
