using UnityEngine;

public class TransitionToLevel : MonoBehaviour
{
    [SerializeField] private int buildIndexLevel;

    private void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.SetGameState(GameStates.Transition, buildIndexLevel);
    }
}

