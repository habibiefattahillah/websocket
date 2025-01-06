using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.WebSockets;
using System.Threading;
using System;
using System.Diagnostics;
using System.Text;

public static class GlobalVariables
{
    public static string IPAddress { get; set; }
    public static string Port { get; set; }
}

public class ConnectWebsocket : MonoBehaviour
{
    public static ConnectWebsocket Instance; // Singleton instance
    public ClientWebSocket _clientSocket;
    public TMP_InputField IPText;
    public TMP_InputField PortText;
    public GameObject scriptAObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Set the singleton instance
            DontDestroyOnLoad(gameObject); // Keep this GameObject alive across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }



    public async void WebSocketConnect()
    {
        GlobalVariables.IPAddress = IPText.text;
        GlobalVariables.Port = PortText.text;

        string serverUrl = $"ws://{GlobalVariables.IPAddress}:{GlobalVariables.Port}";

        try
        {
            _clientSocket = new ClientWebSocket();
            await _clientSocket.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
            scriptAObject.GetComponent<MySceneManager>().LoadScene("Scene2");
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}
