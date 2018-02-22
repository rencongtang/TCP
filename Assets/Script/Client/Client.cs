using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Client : MonoBehaviour {


    // Generate chat panel and message box:
    public GameObject messagePrefab;
    public GameObject chatContainer;

    // To judge is that going to send/recive message
    private bool socketReady;
    // Need clicent name socket
    private TcpClient socket;
    // The data to be trasported 
    private NetworkStream stream;
    // Writer and reader to read and write stream
    private StreamWriter writer;
    private StreamReader reader;

    // Function 1: connect to the server (And button will call this function here)
    public void ConnectedToServer()
    {
        // If already connected, ignore this function

        if (socketReady)
            return;
      
        // Default host and post values 
        string host = "127.0.0.1";
        int port = 6321;

        // Input of host and post in the canvas, overwrite default host port values, if there is something in those booxes
        string h;
        int p;
       
        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
        {
            host = h;
            Debug.Log("Game object is: " + GameObject.Find("HostInput").GetComponent<InputField>().text); // Can't find the game object
        }

        // Convert PortInput into int
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
            port = p;

        // If port and host is good, then creat a socket 
        try {
            // if there is nothing wrong, creat a tcp clicent with 2 parameters
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }

    public void Update()
    {
        // Does the server wants to send us any message?
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    private void OnIncomingData(string data)
    {
        Debug.Log("client sent the message: " + data);
        GameObject go = Instantiate(messagePrefab, chatContainer.transform) as GameObject;
        // send the data information to the chat box
        go.GetComponentInChildren<Text>().text = data;
    }

    // Send message to the Server 
    public void Send(String data)
    {
        Debug.Log("Recieved a new message that: " + data);
        if (!socketReady)
            return;
        writer.WriteLine(data);
        writer.Flush();
    }

    // When click the send button, start send message 
    public void OnSendButton()
    {
        // Get the message in the input box that to be sent
        String message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        // Send to the server
        Send(message);
    }

}
