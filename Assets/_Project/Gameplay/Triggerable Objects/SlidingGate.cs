using UnityEngine;

public class SlidingGate : MonoBehaviour, ITriggerable
{
    [Header("Gate Configuration")]
    public bool startOpened = false;

    [Header("Gate State - DO NOT EDIT")]
    public bool isOpen = false;

    private Animator animator;
    private bool isAnimating = false;
    public int TriggeredCount { get; private set; }

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[SlidingGate] No Animator found on {gameObject.name}");
            return;
        }

        isOpen = startOpened;
        animator.SetBool("isOpen", isOpen);
    }

    public void Trigger()
    {
        if (isAnimating) return;

        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
        isAnimating = true;
        Debug.Log($"[SlidingGate] Gate toggled: {(isOpen ? "Opening" : "Closing")} {gameObject.name}");

        // Fire the appropriate sound event
        if (isOpen)
            FMODManager.Instance.PlayGateOpen(transform.position);

        StartCoroutine(ResetAnimatingFlag());
        TriggeredCount++;
    }

    private System.Collections.IEnumerator ResetAnimatingFlag()
    {
        yield return new WaitForSeconds(GetAnimationClipDuration());
        isAnimating = false;
    }

    private float GetAnimationClipDuration()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
            return clipInfo[0].clip.length;
        return 0.5f;
    }

    public void SetOpenState(bool open)
    {
        isOpen = open;
        animator.SetBool("isOpen", isOpen);
        isAnimating = false;
    }
}