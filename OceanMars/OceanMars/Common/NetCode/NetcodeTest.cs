﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OceanMars.Common.NetCode
{
    public static class NetTest
    {
        public static GameClient c;
        //public static RawClient c1 = new RawClient();
        //public static RawClient c2 = new RawClient();
        //public static RawClient c3 = new RawClient();
        //public static NetworkServer s;
        public static GameServer gs;
        //Netcode testing suite... or just a template
        public static void Main(string[] args)
        {
            try
            {
                c = new GameClient();
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error occurred creating client: {0}", new object[] { error.Message });
                throw error;
            }

            try
            {
                //s = new NetworkServer(9999);
                gs = new GameServer(9999);
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error occurred creating server: {0}", new object[] { error.Message });
                throw error;
            }

            //while (true)
            //{
            //    if (s.getCMD().Count > 0)
            //    {
            //        foreach (Command c in s.getCMD())
            //        {
            //            Console.WriteLine(c.ct);
            //            Console.WriteLine(c.entity_id);
            //            Console.WriteLine(c.direction.X);
            //            Console.WriteLine(c.direction.Y);
            //            Console.WriteLine(c.position.X);
            //            Console.WriteLine(c.position.Y);
            //            Console.WriteLine("===========");
            //        }
            //    }
            //    List<StateChange> l = new List<StateChange>();
            //    StateChange st = new StateChange();
            //    st.type = StateChangeType.DELETE_ENTITY;
            //    st.intProperties[StateProperties.FRAME_WIDTH] = 123;
            //    st.stringProperties[StateProperties.SPRITE_NAME] = "I'm the Baconator";
            //    l.Add(st);
            //    s.broadcastSC(l);
            //    Thread.Sleep(2000);
            //}
            c.ConnectToGame("127.0.0.1", 9999);
            //c1.connect("127.0.0.1", 9999);
            //c2.connect("127.0.0.1", 9999);
            //c3.connect("127.0.0.1", 9999);

            Timer t = new Timer(NetTest.doPing, new AutoResetEvent(false), 0, 2000);
            Thread.Sleep(5000);
            c.Lobby.SelectCharacter(1337);
            c.Lobby.LockCharacter();

            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public static void doPing(Object st)
        {
            Debug.WriteLine("Ping for 1: {0}", c.Network.GetLastPing());
            //Debug.WriteLine("Ping for 2: {0}", c1.getPing());
            //Debug.WriteLine("Ping for 3: {0}", c2.getPing());
            //Debug.WriteLine("Ping for 4: {0}", c3.getPing());
            //Debug.WriteLine("Server packets received: {0}", s.getStats().rcvdPkts);
        }

        //public void exit()
        //{
        //    s.exit();
        //    c.exit();
        //}
    }
}
