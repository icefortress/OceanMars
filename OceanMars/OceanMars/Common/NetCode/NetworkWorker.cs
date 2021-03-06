﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace OceanMars.Common.NetCode
{

    /// <summary>
    /// A worker class that represents a simple UDP client used for the game engine.
    /// </summary>
    public class NetworkWorker : UdpClient
    {
        private Thread receiveThread, sendThread; // Threads used to send and receive data concurrently
        private bool continueRunning; // Whether or not the thread has terminated
        private Queue<NetworkPacket> sendBuffer, receiveBuffer; // Buffers used for sending and receiving packets
        private Semaphore sendSemaphore, receiveSemaphore; // Semaphores used for queuing

        private const int SOCKET_TIMEOUT = 5000; // Constants used for socket control
        private const int MAX_PACKET_COUNT = 1024;

        /// <summary>
        /// Create a network worker to communicate with with other machines.
        /// </summary>
        /// <param name="port">The port to create the connection on.</param>
        public NetworkWorker(int port = 0) : base(port)
        {
            continueRunning = true; // Initialize usable data structures and values
            sendBuffer = new Queue<NetworkPacket>();
            receiveBuffer = new Queue<NetworkPacket>();
            sendSemaphore = new Semaphore(0, MAX_PACKET_COUNT);
            receiveSemaphore = new Semaphore(0, MAX_PACKET_COUNT);
            
            receiveThread = new Thread(RunReceiveThread); // Set up runnable threads
            receiveThread.IsBackground = true;
            sendThread = new Thread(RunSendThread);
            sendThread.IsBackground = true;

            receiveThread.Start();
            sendThread.Start();

            return;
        }

        /// <summary>
        /// Terminate threads associated with the network on the next possible pass.
        /// </summary>
        public void Disconnect()
        {
            Close();
            continueRunning = false;
            sendBuffer.Clear();
            receiveBuffer.Clear();
            return;
        }

        /// <summary>
        /// Queue a packet to be sent over the network.
        /// </summary>
        /// <param name="packet">The packet to be sent.</param>
        public void SendPacket(NetworkPacket packet)
        {
            lock (sendBuffer)
            {
                sendBuffer.Enqueue(packet);
            }
            sendSemaphore.Release();
            return;
        }

        /// <summary>
        /// Receive a packet from a network worker.
        /// </summary>
        /// <returns>The next available packet received from another machine.</returns>
        public NetworkPacket ReceivePacket()
        {
            if (receiveSemaphore.WaitOne(SOCKET_TIMEOUT)) // Wait for incoming packets
            {
                lock (receiveBuffer)
                {
                    return receiveBuffer.Dequeue();
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Main thread loop for receiving packets.
        /// </summary>
        private void RunReceiveThread()
        {
            IPEndPoint serverAddress = new IPEndPoint(IPAddress.Any, 0);
            while (continueRunning)
            {
                try
                {
                    using (MemoryStream incomingPacketStream = new MemoryStream(this.Receive(ref serverAddress)))
                    {
                        NetworkPacket receivePacket = new NetworkPacket((NetworkPacket.PacketType)incomingPacketStream.ReadByte(), serverAddress);
                        incomingPacketStream.Read(receivePacket.DataArray, 0, (int)incomingPacketStream.Length - 1);
                        lock (receiveBuffer)
                        {
                            receiveBuffer.Enqueue(receivePacket);
                        }
                        receiveSemaphore.Release();
                    }
                }
                catch { } // This should only be enountered if something is horribly wrong or it the worker is closing down
            }
            return;
        }

        /// <summary>
        /// Main thread loop for sending packets.
        /// </summary>
        private void RunSendThread()
        {
            while (continueRunning) // Loop until the workers should terminate
            {
                sendSemaphore.WaitOne(); // Grab semaphore access before continuing
                lock (sendBuffer)
                {
                    NetworkPacket sendPacket = sendBuffer.Dequeue();
                    try
                    {
                        Send(sendPacket.DataArray, sendPacket.DataArray.Length, sendPacket.Destination);
                    }
                    catch (ObjectDisposedException exception)
                    {
                        Debug.WriteLine("Object has been disposed: {0}", new Object[] { exception.Message });
                    }
                }
            }
            return;
        }
    }

}
