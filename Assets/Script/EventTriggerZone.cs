using UnityEngine;

public interface IEventTrigger
{
    void OnEventTriggered();
}

public class EventTriggerZone : MonoBehaviour
{
    [SerializeField]
    Transform[] _targets;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        foreach (Transform t in _targets)
        {
            t.GetComponent<IEventTrigger>().OnEventTriggered();
        }

        enabled = false;
    }

    private void OnDrawGizmos()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        Color color = Color.yellow;
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position+bc.center, bc.size);
    }
}
