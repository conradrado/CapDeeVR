using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Transform yawPivot;
    public Transform pitchPivot;

    public TurretGrab leftHandle;
    public TurretGrab rightHandle;

    public float rotationSpeed = 10f;
    public float minPitch = -10f;
    public float maxPitch = 45f;

    void Update()
    {
        // 둘 중 하나라도 안 잡으면 작동 안 함 (브라우닝은 양손 필수)
        if (leftHandle.CurrentInteractor == null || rightHandle.CurrentInteractor == null)
            return;

        Vector3 L = leftHandle.CurrentInteractor.transform.position;
        Vector3 R = rightHandle.CurrentInteractor.transform.position;
        Vector3 mid = (L + R) * 0.5f;

        Vector3 dir = (mid - yawPivot.position).normalized;

        // ---- Yaw ----
        Vector3 flatDir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion yawRot = Quaternion.LookRotation(flatDir, Vector3.up);
            yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, yawRot, Time.deltaTime * rotationSpeed);
        }

        // ---- Pitch ----
        Vector3 localDir = yawPivot.InverseTransformDirection(dir);
        float pitch = Mathf.Atan2(-localDir.y, localDir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion pitchRot = Quaternion.Euler(pitch, 0, 0);
        pitchPivot.localRotation = Quaternion.Slerp(
            pitchPivot.localRotation,
            pitchRot,
            Time.deltaTime * rotationSpeed
        );
    }
}
