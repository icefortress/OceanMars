﻿using System;

namespace OceanMars.Common.NetCode
{

    /// <summary>
    /// A server-side Lobby.
    /// </summary>
    public class LobbyServer : LobbyBase
    {

        /// <summary>
        /// Reference to the game server associated with this lobby.
        /// </summary>
        public new GameServer Game
        {
            get
            {
                return (GameServer)base.Game;
            }
            private set
            {
                base.Game = value;
            }
        }

        /// <summary>
        /// Create a new server-side lobby.
        /// </summary>
        /// <param name="gameServer">The game server associated with this lobby.</param>
        public LobbyServer(GameServer gameServer) : base(gameServer)
        {
            return;
        }

        /// <summary>
        /// A Player has joined, set them up in the system.
        /// </summary>
        /// <param name="gameData">The game data related to the character joining the session.</param>
        protected override void OnPlayerConnect(GameData gameData)
        {
            GameData response;
            Player player = Game.ConnectionIDToPlayer(gameData.ConnectionInfo);
            switch ((GameData.ConnectionDetails)gameData.EventDetail)
            {
                case GameData.ConnectionDetails.IdReqest:
                    if (player == null) // If this is a new player, assign them a new ID, otherwise just resend the old id
                    {
                        player = new Player(gameData.ConnectionInfo, Game);
                    }
                    response = new GameData(GameData.GameDataType.Connect, player.PlayerID);
                    Game.Network.SignalGameData(response, gameData.ConnectionInfo);

                    // TODO: Re-broadcast changes to clients

                    break;
                case GameData.ConnectionDetails.Disconnected:
                case GameData.ConnectionDetails.Dropped:
                    if (player != null) // This is a known player based on their connection, so we can drop them
                    {
                        Game.UnregisterPlayer(player);

                        // TODO: Re-broadcast changes to clients

                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            return;
        }

        /// <summary>
        /// A player has changed their character selection.
        /// </summary>
        /// <param name="gameData">The game data related to character selection.</param>
        protected override void OnPlayerSelectCharacter(GameData gameData)
        {
            Player player = Game.ConnectionIDToPlayer(gameData.ConnectionInfo);
            player.CharacterSelection = gameData.EventDetail;
            Game.Network.BroadCastGameData(new GameData(GameData.GameDataType.SelectCharacter, player.PlayerID, gameData.EventDetail));
            return;
        }

        /// <summary>
        /// Hanldes the character lock request
        /// </summary>
        /// <param name="gameData">The game data related to character locking.</param>
        protected override void OnPlayerLockCharacter(GameData gameData)
        {
            Player player = Game.ConnectionIDToPlayer(gameData.ConnectionInfo);
            player.CharacterLocked = true;
            Game.Network.BroadCastGameData(new GameData(GameData.GameDataType.LockCharacter, player.PlayerID));
            return;
        }

    }
}
