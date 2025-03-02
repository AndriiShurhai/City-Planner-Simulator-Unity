using UnityEngine;

public class StatisticsPanelTooltip : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void TooglePanel()
    {
        if (animator.GetBool("show"))
        {
            animator.SetBool("show", false);
        }

        else if (!animator.GetBool("show"))
        {
            animator.SetBool("show", true);
        }
    }
}
