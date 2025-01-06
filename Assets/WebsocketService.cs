using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using System.Text;
using TMPro;
using Newtonsoft.Json;

public class WebsocketService : MonoBehaviour
{
    public ClientWebSocket _clientSocket;

    [SerializeField]
    public GameObject armController;
    private WebSocketJointAngleSubscriber webSocketSubscriber;
    private WebSocketJointAngleSubscriber.JointStateData jointStateData;

    public TMP_Text x;
    public TMP_Text y;
    public TMP_Text z;
    public TMP_Text w;
    public TMP_Text p;
    public TMP_Text r;

    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_InputField zInput;
    public TMP_InputField wInput;
    public TMP_InputField pInput;
    public TMP_InputField rInput;

    private bool isSyncEnabled = false; // Tracks whether the sync is enabled

    public void OnToggled()
    {
        isSyncEnabled = true;
    }

    public void OnUntoggled()
    {
        isSyncEnabled = false;
    }

    private async void Start()
    {
        // Use the WebSocket connection from the previous scene
        _clientSocket = ConnectWebsocket.Instance._clientSocket;

        if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
        {
            await ReceiveMessagesAsync();
        }
        else
        {
            Console.WriteLine("WebSocket connection is not open.");
        }

        if (armController != null)
        {
            webSocketSubscriber = armController.GetComponent<WebSocketJointAngleSubscriber>();

            if (webSocketSubscriber != null)
            {
                jointStateData = new WebSocketJointAngleSubscriber.JointStateData();
            }
        }

    }

    public async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024];
        while (_clientSocket?.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ReceivedData data = JsonConvert.DeserializeObject<ReceivedData>(message);
                
                if (data.client != 0)
                {
                    continue;
                }

                UpdateTextBoxes(data.xyzwpr);

                jointStateData = new WebSocketJointAngleSubscriber.JointStateData
                {
                    position = data.position
                };

                armController.GetComponent<WebSocketJointAngleSubscriber>().SetJointAngles(jointStateData);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                //txtLog.AppendText("Server closed the connection.\n");
                await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            }
        }
    }

    public void UpdateTextBoxes(double[] xyzwpr)
    {
        x.text = xyzwpr[0].ToString("F2");
        y.text = xyzwpr[1].ToString("F2");
        z.text = xyzwpr[2].ToString("F2");
        w.text = xyzwpr[3].ToString("F2");
        p.text = xyzwpr[4].ToString("F2");
        r.text = xyzwpr[5].ToString("F2");
    }

    private void Update()
    {
        if (isSyncEnabled)
        {
            SyncInputFields();
        }
    }

    public void SyncInputFields()
    {
            xInput.text = x.text;
            yInput.text = y.text;
            zInput.text = z.text;
            wInput.text = w.text;
            pInput.text = p.text;
            rInput.text = r.text;
    }

    public void SendData(SentData data)
    {
        string message = JsonConvert.SerializeObject(data);
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        _clientSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public void CloseClaw()
    {
        var data = new SentData
        {
            intRDO = new bool[10]
                {
                    false, true, false, false, false, false, false, false, false, false
                }
        };

        SendData(data);
    }

    public void OpenClaw()
    {
        var data = new SentData
        {
            intRDO = new bool[10]
                {
                    true, false, false, false, false, false, false, false, false, false
                }
        };

        SendData(data);
    }

    public void SendXYZWPR()
    {
        var data = new SentData
        {
            xyzwpr = new double[6]
            {
                double.Parse(xInput.text),
                double.Parse(yInput.text),
                double.Parse(zInput.text),
                double.Parse(wInput.text),
                double.Parse(pInput.text),
                double.Parse(rInput.text)
            }
        };

        SendData(data);
    }

    public void ActivateRO()
    {
        var data = new SentData
        {
            activateRO = true,
        };

        SendData(data);
    }

    public class ReceivedData
    {
        public int client { get; set; }
        public double[] xyzwpr { get; set; }
        public double[] position { get; set; }
        public bool[] intRDO { get; set; }

        public ReceivedData()
        {
            client = 1;
            xyzwpr = new double[0];
            position = new double[0];
            intRDO = new bool[0];
        }
    }

    public class SentData
    {
        public int client { get; set; }
        public double[] xyzwpr { get; set; }
        public double[] position { get; set; }
        public bool[] intRDO { get; set; }
        public bool? activateRO { get; set; }

        public SentData()
        {
            client = 1;
            xyzwpr = new double[0];
            position = new double[0];
            intRDO = new bool[0];
        }
    }
}

