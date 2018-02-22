﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    // Initial a port number
    public int port = 6321;
    // To listen the information.
    private TcpListener server;
    // To initial the server
    private bool serverStarted;
    // The Clients that is connecting the server.
    private List<ServerClient> clients;
    // Need to track disconnect because it can't be done immedicatedly.
    private List<ServerClient> disconnectList;

    private void Start()
    {
        // Initial Slients
        clients = new List<ServerClient>();
        // Initial Servers
        disconnectList = new List<ServerClient>();
        
        // @ Mario's study: try- catch: when there is an exception,
        // the catch will find out the exception, block is executed 
        // until an exception is thrown or it is completed successfully
        try
        {
            // Initial the server, and 'try' makes sure the port sould not be changed, or it will jump to catch
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            // TCP is turn by turn, just call a little moment with the server and turn to the next one. The server can
            // listening and grab the message and start to listen again to another message. 
            StartListening();
            serverStarted = true;
            Debug.Log("Server has been start at the port " + port.ToString());
            
        }
        // Exception is in using System
        catch(Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        // If there is no server start, we do nothing at all
        if (!serverStarted) return;

        foreach (ServerClient c in clients)
        {
            // Is the client still connected?
            if (!IsConnected(c.tcp))
            {
                // close the socket
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // Check for message from the client 
            else
            {
                // Message
                NetworkStream s = c.tcp.GetStream();
                // Here is a message
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    String data = reader.ReadLine();

                    if (data != null)
                    {
                        // From c and with the data. Knows whhich clicent and what data it is
                        OnIncomingData(c, data);
                    }
                }
            }
        }
    }
    
    // Server begin to accept tcp client 
    private void StartListening()
    {
        // Begins an asynchronous operation to accept an incoming connection attempt
        server.BeginAcceptTcpClient(AcceptTcpClient,server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {

        TcpListener listener = (TcpListener)ar.AsyncState;
        // add the client that the listener accept 
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        // Start listen again
        StartListening();
        //Send message to everyone, say someone has connected
        // In this example, the server brodacast the last client's name
        Broadcast(clients[clients.Count - 1].clientName + " has connected", clients);
        

    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                // Recommanded to write because it's good for the server, usually not necessary
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
                return false;
        }
        catch // If can't actually reach client 
        {
            return false;
        }
    }

    // Print out the clicent who send the message and the data itself.
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log(c.clientName + " has sent the following meassage: " + data);
    }

    // Send message to all the clients in the list
    private void Broadcast(String data, List<ServerClient> cl)
    {

        foreach (ServerClient c in cl)
        {
            try
            {
                // Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();

            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message + "to clicent" + c.clientName);
            }
        }
    }
}

// Denefition of who is connected to the server for the server itself, 
// a very simple denefition for a client for a server

public class ServerClient
{
    //TcpClient is from Net.Sockets
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}
