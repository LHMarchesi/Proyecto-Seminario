using UnityEngine;

public class DelayThenGotoState : StateMachineBehaviour
{
    [Tooltip("Seconds to wait before switching states")]
    public float delay = 0.5f;

    [Tooltip("Animator state name to play after the delay")]
    public string targetStateName = "FallingMjolnir";

    [Tooltip("Animator layer index of the target state")]
    public int targetLayer = 0;

    [Tooltip("Crossfade duration in seconds")]
    public float transitionDuration = 0.05f;

    [Tooltip("Start time of the target clip (0..1)")]
    public float targetNormalizedTime = 0f;

    [Tooltip("Ignore Time.timeScale")]
    public bool useUnscaledTime = false;

    float t;
    bool fired;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        t = 0f;
        fired = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (fired) return;

        t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (t >= delay)
        {
            fired = true;
            // Transition to the target state
            animator.CrossFadeInFixedTime(targetStateName, transitionDuration, targetLayer, targetNormalizedTime);
        }
    }
}
