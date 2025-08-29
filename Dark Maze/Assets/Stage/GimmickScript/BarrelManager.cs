using UnityEngine;

public class BarrelManager : MonoBehaviour
{
    [SerializeField] Animator animator;
    void Start()
    {
        animator.SetBool("Destroy", true);
    }

    void Update()
    {
        
    }
}
