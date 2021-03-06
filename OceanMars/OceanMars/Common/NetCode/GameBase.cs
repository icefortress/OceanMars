﻿using System;
using System.Collections.Generic;

namespace OceanMars.Common.NetCode
{

    /// <summary>
    /// Abstraction of the top-of-network-stack game.
    /// </summary>
    public abstract class GameBase
    {
        /// <summary>
        /// Maximum number of players in a game.
        /// </summary>
        public const int MAX_PLAYERS = 8;

        /// <summary>
        /// Game states that have been received and not yet committed to the overall game state.
        /// </summary>
        protected List<GameData> gameStatesToCommit;

        /// <summary>
        /// Game states that must be sent out to other games (either the client or the main server).
        /// </summary>
        protected List<GameData> gameStatesToSend;

        /// <summary>
        /// The hierarchical tree that represents the state of the game.
        /// </summary>
        public State GameState
        {
            get;
            private set;
        }

        /// <summary>
        /// The underlying network server that is used by this game.
        /// </summary>
        protected NetworkBase Network
        {
            get;
            set;
        }

        /// <summary>
        /// The players that are known by the game.
        /// </summary>
        public Player[] players { get; protected set; }

        /// <summary>
        /// The player that is local to this machine.
        /// </summary>
        public Player LocalPlayer
        {
            get;
            set;
        }

        /// <summary>
        /// Register a player with the game and return their ID.
        /// </summary>
        /// <param name="player">The player to register.</param>
        /// <returns>An integer representing the new ID of the registered player.</returns>
        public abstract int RegisterPlayer(Player player);

        /// <summary>
        /// Unregister a player from the game.
        /// </summary>
        /// <param name="player">The player to unregister.</param>
        public virtual void UnregisterPlayer(Player player)
        {
            if (player != null) // Don't remove already-removed players
            {
                players[player.PlayerID] = null;
            }
            return;
        }

        /// <summary>
        /// The lobby associated with this particular GameBase.
        /// </summary>
        protected LobbyBase Lobby
        {
            get;
            set;
        }

        /// <summary>
        /// Instantiate the base components of a game.
        /// </summary>
        /// <param name="port">The port to open the GameNetworkServer on.</param>
        protected GameBase(NetworkBase network)
        {
            gameStatesToCommit = new List<GameData>();
            gameStatesToSend = new List<GameData>();
            players = new Player[MAX_PLAYERS]; // Defaults to null elements (unlike C, you don't have to set the elements)
            GameState = new State();
            Network = network;

            GameState.registerStatePhaseChange(this.OnStatePhaseChange);
            GameState.registerTransformChange(this.OnTransformChange);
            GameState.RegisterEntityStateChange(this.OnEntityStateChange);

            return;
        }

        /// <summary>
        /// Fetch a player with the given ID.
        /// </summary>
        /// <param name="playerID">The ID of the player to fetch.</param>
        /// <returns>A player with the input ID. Returns null if no such player exists.</returns>
        public Player GetPlayer(int playerID)
        {
            return players[playerID];
        }

        /// <summary>
        /// Update an entity based on a transform.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void OnTransformChange(Entity entity)
        {
            // Generate a transform change packet, put it on stack
            TransformData transformData = new TransformData(entity.id, entity.transform);
            
            // TODO: This likely doesn't work. This neeeds to be fixed (the player ID and event detail might need changing, or we may simply need a new constructor).
            GameData gameData = new GameData(GameData.GameDataType.Movement, LocalPlayer.PlayerID, 0, transformData);

            lock (gameStatesToSend)
            {
                gameStatesToSend.Add(gameData);
            }
            return;
        }

        /// <summary>
        /// Send out state change information
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void OnEntityStateChange(Entity entity)
        {
            EntityStateData stateData;
            // Invoke class-specific constructors, to avoid sending extra information
            if (entity is MobileEntity)
            {
                stateData = new EntityStateData((MobileEntity)entity);
            }
            else
            {
                stateData = new EntityStateData(entity);
            }

            GameData gameData = new GameData(GameData.GameDataType.EntityStateChange, LocalPlayer.PlayerID, 0);
            gameData.EventDetail = entity.id;
            gameData.EntityStateData = stateData;

            lock (gameStatesToSend)
            {
                gameStatesToSend.Add(gameData);
            }
            return;
        }


        /// <summary>
        /// Commit game state updates.
        /// </summary>
        public abstract void CommitGameStates();

        /// <summary>
        /// Send out game state updates.
        /// </summary>
        public abstract void SendGameStates();

        /// <summary>
        /// Add incoming data to the game states.
        /// </summary>
        /// <param name="gameData">Received game data that should inform us about changing state, requests, etc.</param>
        protected virtual void AddGameState(GameData gameData)
        {
            lock (gameStatesToCommit)
            {
                gameStatesToCommit.Add(gameData);
            }
            return;
        }

        /// <summary>
        /// Handle changes to the phase of the world state.
        /// </summary>
        /// <param name="phase">The phase that we are transitioning into.</param>
        public void OnStatePhaseChange(State.PHASE phase)
        {
            switch (phase)
            {
                case State.PHASE.FINISHED_FRAME:
                    SendGameStates();
                    break;
                case State.PHASE.READY_FOR_CHANGES:
                    CommitGameStates();
                    break;
                default:
                    //throw new NotImplementedException("Unhandled state passed to GameBase");
                    break;
            }
            return;
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            Network.RegisterGameDataUpdater(AddGameState);
            return;
        }
    }
}
