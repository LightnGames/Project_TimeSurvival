using UnityEngine;

public class EventDeadlineEnemySpawner : MonoBehaviour, IEventTrigger
{
    [SerializeField] private DeadlineEnemy _deadlineEnemy;

    private void Awake()
    {
        
    }

    public void OnEventTriggered()
    {
        _deadlineEnemy.Spawn();
    }
}
