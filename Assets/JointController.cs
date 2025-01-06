using UnityEngine;

public class JointController : MonoBehaviour
{
    public enum JointType { Prismatic, Continuous, Revolute };
    public enum MovementAxis { x, y, z };

    [Header("Joint Configuration")]
    [SerializeField]
    public JointType jointType;
    [SerializeField]
    public MovementAxis axis;

    [Header("Joint Limits")]
    [SerializeField]
    float lowerLimit; // Minimum joint value
    [SerializeField]
    float upperLimit; // Maximum joint value

    [Header("Axis Configuration")]
    public bool invertAxis = false; // Invert the axis if needed

    [Header("Movement Scaling")]
    public float movementScale = 1.0f; // Scale joint movement if needed

    [Header("Robot Parameters for IK")]
    public float link1Length = 1.0f; // Length of the first link
    public float link2Length = 1.0f; // Length of the second link
    public JointController nextJoint; // Reference to the next joint in the chain (if any)

    public Transform jointTransform;

    private void Awake()
    {
        jointTransform = gameObject.transform;
    }

    public void SetJointPosition(float jointValue)
    {
        // Apply scaling
        jointValue *= movementScale;

        // Reverse the axis if needed
        if (invertAxis)
        {
            jointValue = -jointValue;
        }

        // Clamp the value within limits
        float clampedValue = ClampValue(jointValue);

        // Log debug info
        Debug.Log($"Joint {name}: Original Value = {jointValue}, Scaled = {clampedValue}");

        // Apply movement based on joint type
        if (jointType == JointType.Prismatic)
        {
            SetPrismaticAbsolutePosition(clampedValue);
        }
        else if (jointType == JointType.Revolute || jointType == JointType.Continuous)
        {
            SetRevoluteAbsoluteRotation(clampedValue);
        }
    }

    private void SetPrismaticAbsolutePosition(float jointValue)
    {
        Vector3 absolutePosition = jointTransform.localPosition;

        switch (axis)
        {
            case MovementAxis.x:
                absolutePosition.x = jointValue;
                break;
            case MovementAxis.y:
                absolutePosition.y = jointValue;
                break;
            case MovementAxis.z:
                absolutePosition.z = jointValue;
                break;
        }

        jointTransform.localPosition = absolutePosition;
    }

    private void SetRevoluteAbsoluteRotation(float jointValue)
    {
        Vector3 absoluteRotation = jointTransform.localEulerAngles;

        switch (axis)
        {
            case MovementAxis.x:
                absoluteRotation.x = jointValue;
                break;
            case MovementAxis.y:
                absoluteRotation.y = jointValue;
                break;
            case MovementAxis.z:
                absoluteRotation.z = jointValue;
                break;
        }

        jointTransform.localEulerAngles = absoluteRotation;
    }

    private float ClampValue(float jointValue)
    {
        if (jointType != JointType.Continuous) // Continuous joints don't need clamping
        {
            jointValue = Mathf.Clamp(jointValue, lowerLimit, upperLimit);
        }

        return jointValue;
    }

    public void SolveIK(Vector2 targetPosition)
    {
        float x = targetPosition.x;
        float y = targetPosition.y;

        // Calculate the distance to the target
        float distanceSquared = x * x + y * y;
        float maxReach = link1Length + link2Length;

        // Check if the target is out of reach
        if (distanceSquared > maxReach * maxReach)
        {
            Debug.LogError("Target is out of reach.");
            return;
        }

        // Cosine law for theta2
        float cosTheta2 = Mathf.Clamp(
            (distanceSquared - link1Length * link1Length - link2Length * link2Length) / (2 * link1Length * link2Length),
            -1f, 1f
        );
        float theta2 = Mathf.Acos(cosTheta2);

        // Calculate theta1
        float sinTheta2 = Mathf.Sin(theta2);
        float k1 = link1Length + link2Length * cosTheta2;
        float k2 = link2Length * sinTheta2;

        float theta1 = Mathf.Atan2(y, x) - Mathf.Atan2(k2, k1);

        Debug.Log($"Theta1: {theta1 * Mathf.Rad2Deg}°, Theta2: {theta2 * Mathf.Rad2Deg}°");

        // Apply calculated angles to the joints (absolute angles in degrees)
        SetJointPosition(theta1 * Mathf.Rad2Deg); // First joint
        if (nextJoint != null)
        {
            nextJoint.SetJointPosition(theta2 * Mathf.Rad2Deg); // Second joint
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the arm links for visualization
        Gizmos.color = Color.blue;

        Vector3 joint1 = transform.position;
        Vector3 joint2 = joint1 + new Vector3(
            Mathf.Cos(jointTransform.localEulerAngles.z * Mathf.Deg2Rad),
            Mathf.Sin(jointTransform.localEulerAngles.z * Mathf.Deg2Rad),
            0
        ) * link1Length;

        Vector3 endEffector = joint2 + new Vector3(
            Mathf.Cos((jointTransform.localEulerAngles.z + (nextJoint != null ? nextJoint.jointTransform.localEulerAngles.z : 0)) * Mathf.Deg2Rad),
            Mathf.Sin((jointTransform.localEulerAngles.z + (nextJoint != null ? nextJoint.jointTransform.localEulerAngles.z : 0)) * Mathf.Deg2Rad),
            0
        ) * link2Length;

        Gizmos.DrawLine(joint1, joint2);
        Gizmos.DrawLine(joint2, endEffector);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endEffector, 0.05f);
    }
}
