﻿#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using OceanMars.Client.Screens;
using OceanMars.Common.NetCode;
using SkyCrane.Screens;
#endregion

namespace OceanMars.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class LobbyScreen : MenuScreen
    {

        // TODO: Pretend that id's are nicely handled all over the place until I actually handle them niceley later after sleep
        // TODO: Prevent spoofing to clients if necessary
        // TODO: Play sounds incurred by other players

        #region Fields

        // Game and player settings
        GameClient gc;
        GameServer gs;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        /// <param name="host">Whether or not this player is the host.</param>
        /// <param name="multiplayer">Whether or not this game is multiplayer.</param>
        public LobbyScreen(GameClient gc, GameServer gs = null)
            : base("Lobby", true)
        {
            this.gc = gc;
            this.gs = gs;

            
            {
                // Create the single invisible menu entry
                MenuEntry startGameMenuEntry = new MenuEntry(string.Empty, true);
                startGameMenuEntry.Selected += StartGameMenuEntrySelected;
                MenuEntries.Add(startGameMenuEntry);
            }

            return;
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            base.LoadContent();
            return;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// A player has hit cancel while browsing characters.
        /// </summary>
        protected override void OnCancel()
        {
            return;
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void StartGameMenuEntrySelected(object sender, PlayerInputEventArgs e)
        {
            // Start the fucking game yo
            if (gs != null)
            {
                //send game start packet
                gc.Lobby.SendLaunchPacket();
            }

            return;
        }


        #endregion

        #region Draw and Update

        /// <summary>
        /// Run a regular update loop on the menu.
        /// </summary>
        /// 

        bool ahack = false;
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (gc.sorryPeter)
            {
                // Poll to see if game has started
                startGame();
                ahack = true;
            }

            return;
        }

        private void startGame()
        {
            gc.setupGameState(0, gc.LocalPlayer.PlayerID);
            ScreenManager.AddScreen(new GameplayScreen(gc));
        }

        /// <summary>
        /// Draw various graphical elements on the character select screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            return;
        }

        #endregion
    }
}
