using UnityEngine;

public class PortfolioPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;

    void Start()
    {
        animator.SetBool("Attack", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
