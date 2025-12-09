using UnityEngine;

public class TriggerEquip : MonoBehaviour
{
    [Header("RightHandController > GunSocket을 드래그하세요")]
    public Transform rightHandSocket;

    private bool equipped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!equipped && other.CompareTag("PlayerHand"))
        {
            equipped = true;

            GunFollow follow = GetComponent<GunFollow>();
            if (follow != null)
                follow.target = rightHandSocket;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            this.enabled = false;
        }
    }
}
