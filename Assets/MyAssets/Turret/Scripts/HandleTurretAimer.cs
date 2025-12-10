using UnityEngine;

public class HandleTurretAimer : MonoBehaviour
{
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private TurretGrab leftHandle;
    [SerializeField] private TurretGrab rightHandle;
    [SerializeField] private float yawSpeed = 180f;
    [SerializeField] private float pitchSpeed = 180f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-45f, 60f);
    [SerializeField] private bool invertPitch = true;
    [SerializeField] private Transform[] handlesToPin;

    private Vector3[] _handleLocalPositions;
    private Quaternion[] _handleLocalRotations;

    private void Awake()
    {
        if (handlesToPin == null || handlesToPin.Length == 0)
            return;

        _handleLocalPositions = new Vector3[handlesToPin.Length];
        _handleLocalRotations = new Quaternion[handlesToPin.Length];
        for (int i = 0; i < handlesToPin.Length; i++)
        {
            _handleLocalPositions[i] = handlesToPin[i].localPosition;
            _handleLocalRotations[i] = handlesToPin[i].localRotation;
        }
    }

    private void Update()
    {
        if (!TryGetAimDirection(out var aimDirection))
            return;

        ApplyYaw(aimDirection);
        ApplyPitch(aimDirection);
    }

    private void LateUpdate()
    {
        // Keep handle transforms glued to the turret rig so XR grab physics can't pull them away.
        if (handlesToPin == null || _handleLocalPositions == null)
            return;

        for (int i = 0; i < handlesToPin.Length; i++)
        {
            if (handlesToPin[i] == null)
                continue;

            handlesToPin[i].localPosition = _handleLocalPositions[i];
            handlesToPin[i].localRotation = _handleLocalRotations[i];
        }
    }

    private bool TryGetAimDirection(out Vector3 direction)
    {
        direction = Vector3.zero;

        var left = leftHandle != null ? leftHandle.CurrentInteractor?.transform : null;
        var right = rightHandle != null ? rightHandle.CurrentInteractor?.transform : null;

        // Aim from turret origin toward grabbed interactor positions so the gun follows the hands,
        // instead of trying to match controller forward (which was causing drift).
        if (left != null)
            direction += (left.position - yawPivot.position);
        if (right != null)
            direction += (right.position - yawPivot.position);

        if (direction.sqrMagnitude < 0.001f)
            direction = yawPivot != null ? yawPivot.forward : Vector3.forward;

        direction = direction.normalized;
        return direction.sqrMagnitude > 0f;
    }

    private void ApplyYaw(Vector3 worldDirection)
    {
        if (yawPivot == null)
            return;

        var flat = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
        if (flat.sqrMagnitude < 0.0001f)
            return;

        var targetRotation = Quaternion.LookRotation(flat, Vector3.up);
        yawPivot.rotation = Quaternion.RotateTowards(yawPivot.rotation, targetRotation, yawSpeed * Time.deltaTime);
    }

    private void ApplyPitch(Vector3 worldDirection)
    {
        if (pitchPivot == null || yawPivot == null)
            return;

        var localDir = yawPivot.InverseTransformDirection(worldDirection);
        var pitchAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
        if (invertPitch)
            pitchAngle = -pitchAngle;
        pitchAngle = Mathf.Clamp(pitchAngle, pitchLimits.x, pitchLimits.y);

        var targetRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        pitchPivot.localRotation = Quaternion.RotateTowards(pitchPivot.localRotation, targetRotation, pitchSpeed * Time.deltaTime);
    }
}
