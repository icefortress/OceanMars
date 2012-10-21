﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OceanMars.Common.NetCode
{

    /// <summary>
    /// Abstraction of a game client program that rests on top of the network stack.
    /// </summary>
    public class GameClient : GameBase
    {
        /// <summary>
        /// The client lobby associated with this game client.
        /// </summary>
        public new LobbyClient Lobby
        {
            get
            {
                return (LobbyClient)base.Lobby;
            }
            private set
            {
                base.Lobby = value;
            }
        }

        /// <summary>
        /// Reference to the underlying network associated with this game client.
        /// </summary>
        public new NetworkClient Network
        {
            get
            {
                return (NetworkClient)base.Network;
            }
            private set
            {
                base.Network = value;
            }
        }

        /// <summary>
        /// Create a new GameClient.
        /// </summary>
        /// <param name="port">The port to create the GameServer on.</param>
        public GameClient()
            : base(new NetworkClient())
        {

            Lobby = new LobbyClient(this); // Give initial control to the lobby
            Network.RegisterGameDataUpdater(Lobby.UpdateLobbyState);

            // TODO: Once we're done with the lobby, the connections and players will be handed off to the game and the GameDataUpdater re-registered to GameClient.UpdateGameState

            return;
        }

        /// <summary>
        /// After choosing the level and number of players, start the game.
        /// </summary>
        /// <param name="levelID">The ID of the level to initialize.</param>
        /// <param name="myPlayerID">The ID of the player character.</param>
        public void SetupGameState(int levelID, int myPlayerID)
        {
            Level level = new Level(GameState.root, LevelPack.levels[levelID]);

            SpawnPointEntity sp = level.spawnPoints[myPlayerID];
            TestMan tm = new TestMan(sp, true);
            players[myPlayerID].EntityID = tm.id;
            LocalPlayer.EntityID = players[myPlayerID].EntityID;    //hack?

            EntityData entityData = new EntityData(EntityData.EntityType.TestMan, tm.id, tm.transform);
            GameData gameData = new GameData(GameData.GameDataType.NewEntity, myPlayerID, 0, null, entityData);
            Network.SendGameData(gameData);

            return;
        }

        public Entity getPlayerEntity()
        {
            return GameState.entities[LocalPlayer.EntityID];
        }

        /// <summary>
        /// Register a new player with the game client.
        /// </summary>
        /// <param name="player">The player to register with the game client.</param>
        /// <returns>Returns the ID of the player to register.</returns>
        public override int RegisterPlayer(Player player)
        {
            players[player.PlayerID] = player;
            return player.PlayerID;
        }

        public override void CommitGameStates()
        {
            //take a snapeshot of the GameStatesToCommit in case more are added while we're looping
            int gsLength = gameStatesToCommit.Count;

            for (int i = 0; i < gsLength; ++i)
            {
                GameData gs = gameStatesToCommit[i];
                if (gs != null)
                {
                    int id;
                    switch (gs.Type)
                    {
                        case GameData.GameDataType.Movement:
                            id = gs.TransformData.EntityID;
                            GameState.entities[id].transform = gs.TransformData.GetMatrix();
                            break;
                        case GameData.GameDataType.PlayerTransform:
                            id = players[gs.TransformData.EntityID].EntityID;
                            GameState.entities[id].transform = gs.TransformData.GetMatrix();
                            break;
                        case GameData.GameDataType.NewEntity:
                            //hack, need to use something more than TransformData
                            //assuming TestMan for now
                            if (gs.EntityData.type == EntityData.EntityType.TestMan)
                            {
                                TestMan testMan = new TestMan(GameState.root, false, gs.TransformData.EntityID);
                                testMan.transform = gs.TransformData.GetMatrix();
                            }
                            break;
                    }

                }
            }

            //clear the game states we've just committed
            gameStatesToCommit.RemoveRange(0, gsLength);
        }

        /// <summary>
        /// Send any built of game state changes over the network.
        /// </summary>
        public override void SendGameStates()
        {
            Network.SendGameData(gameStatesToSend);
            gameStatesToSend.Clear();
            return;
        }

        /// <summary>
        /// Connect the client to a hosting server.
        /// </summary>
        /// <param name="host">The host address.</param>
        /// <param name="port">The port number to connect to.</param>
        public void ConnectToGame(String host, int port)
        {
            Network.Connect(host, port);
            Lobby.JoinLobby();
            return;
        }
    }

}

