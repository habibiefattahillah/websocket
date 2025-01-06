using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebSocketJointAngleSubscriber : MonoBehaviour
{
    // WebSocket configuration
    public ClientWebSocket _clientSocket;

    [SerializeField]
    private JointController[] m_OrderedJoints; // Joints array
    public JointStateData jointStateData = new JointStateData();
    private bool isSyncEnabled = false; // Tracks whether the sync is enabled

    private CancellationTokenSource m_CancellationTokenSource;
    private byte[] m_ReceiveBuffer = new byte[1024];

    public void OnToggled()
    {
        isSyncEnabled = true;
    }

    public void OnUntoggled()
    {
        isSyncEnabled = false;
    }

    public void SetJointAngles(JointStateData jointData)
    {
        jointStateData = jointData;

        if (isSyncEnabled)
        {
            ApplyJointAngles(jointData);
        }
    }

    public void ApplyJointAngles(JointStateData jointData)
    {
        if (jointData.position.Length != m_OrderedJoints.Length)
        {
            Debug.LogError("Mismatch between received joint data and robot joint count.");
            return;
        }

        for (int i = 0; i < m_OrderedJoints.Length; i++)
        {
            m_OrderedJoints[i].SetJointPosition((float)jointData.position[i]);
        }
    }

    [Serializable]
    public class JointStateData
    {
        public double[] position; // Array of joint angles
    }
}