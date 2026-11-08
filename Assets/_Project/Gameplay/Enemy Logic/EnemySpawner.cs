using UnityEngine;

public class EnemySpawner : MonoBehaviour, ITriggerable
{
    public bool spawnOnStart = false;
    private AiEmplacementController _controller;
    private Animator _animator;
    public int TriggeredCount {  get; private set; }


    private void Awake()
    {
        _controller = GetComponentInChildren<AiEmplacementController>();
        _animator = _controller.GetComponent<Animator>();
        _controller.isActive = spawnOnStart;
        _animator.SetBool("spawned", spawnOnStart);
    }


    public void Trigger()
    {
        _animator.SetBool("spawned", true);
        _controller.isActive = true;
        TriggeredCount++;
    }
}
