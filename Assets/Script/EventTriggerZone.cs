using UnityEngine;

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
            foreach (IEventTrigger i in t.GetComponents<IEventTrigger>())
            {
                i.OnEventTriggered();
            }
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        Color color = Color.yellow;
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + bc.center, transform.rotation, bc.size);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
