using UnityEngine;

public class EventAnimatedMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] Animation _breakAnimation;

    public void OnEventTriggered()
    { 
        _breakAnimation.Play();
    }
}
