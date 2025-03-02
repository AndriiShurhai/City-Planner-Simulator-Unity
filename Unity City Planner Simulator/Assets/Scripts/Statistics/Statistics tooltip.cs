using UnityEngine;

public class Statisticstooltip : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void TooglePanel()
    {
        if (animator.GetBool("showStat"))
        {
            animator.SetBool("showStat", false);
        }

        else if (!animator.GetBool("showStat"))
        {
            animator.SetBool("showStat", true);
        }
    }
}
