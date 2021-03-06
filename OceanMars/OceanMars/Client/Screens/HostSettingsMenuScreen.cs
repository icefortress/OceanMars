#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using OceanMars.Common.NetCode;
using SkyCrane.Screens;
using OceanMars.Screens;
#endregion

namespace OceanMars.Client.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class HostSettingsMenuScreen : MenuScreen
    {
        #region Initialization

        // Menu entries
        MenuEntry hostAddressMenuEntry;
        MenuEntry hostPortMenuEntry;

        // Hosting, address and port information
        string lastAddress = "";
        static StringBuilder hostAddress = new StringBuilder(""); // Host's address (IP or hostname)
        const int MAX_ADDRESS_LENGTH = 32;
        string lastPort = "9999";
        static StringBuilder hostPort = new StringBuilder("9999"); // Host's port
        const int MAX_PORT_LENGTH = 5;
        const int MIN_PORT = 1;
        const int MAX_PORT = 65535;

        // Typing sound effects
        SoundEffect typingSoundEffect;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        /// <param name="host">Whether or not this player is the host.</param>
        /// <param name="multiplayer">Whether or not this game is multiplayer.</param>
        public HostSettingsMenuScreen()
            : base("Join Game")
        {
            
            hostAddressMenuEntry = new MenuEntry(string.Empty);
            hostAddressMenuEntry.Selected += HostAddressMenuEntrySelected;
            hostAddressMenuEntry.Typed += HostAddressMenuEntryTyped;
            MenuEntries.Add(hostAddressMenuEntry);


            // Both the host and the client should specify ports
            hostPortMenuEntry = new MenuEntry(string.Empty);
            hostPortMenuEntry.Selected += HostPortMenuEntrySelected;
            hostPortMenuEntry.Typed += HostPortMenuEntryTyped;
            MenuEntries.Add(hostPortMenuEntry);
            
            SetMenuEntryText(); // Set the initial menu text

            // Add the select and back options
            MenuEntry continueMenuEntry = new MenuEntry("Connect to Game");
            continueMenuEntry.Selected += ContinueMenuEntrySelected;
            MenuEntries.Add(continueMenuEntry);

            MenuEntry backMenuEntry = new MenuEntry("Back");
            backMenuEntry.Selected += OnCancel;
            MenuEntries.Add(backMenuEntry);
            
            return;
        }

        /// <summary>
        /// Load some content
        /// </summary>
        public override void LoadContent()
        {
            typingSoundEffect = ScreenManager.Game.Content.Load<SoundEffect>("SoundFX/typing");
            base.LoadContent();
        }

        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            hostAddressMenuEntry.Text = "Host Address: " + hostAddress;
            hostPortMenuEntry.Text = "Host Port: " + hostPort;
            return;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void HostAddressMenuEntrySelected(object sender, PlayerInputEventArgs e)
        {
            TypingInput = true;
            return;
        }

        /// <summary>
        /// Event handler for typing this into the host address.
        /// </summary>
        void HostAddressMenuEntryTyped(object sender, PlayerInputEventArgs e)
        {
            bool resetText = false;
            if (e.KeysTyped != string.Empty && hostAddress.Length < MAX_ADDRESS_LENGTH) // Add a character
            {
                typingSoundEffect.Play();
                hostAddress.Append(e.KeysTyped);
                resetText = true;
            }
            if (e.TypingBackspace && hostAddress.Length > 0) // Remove a character
            {
                typingSoundEffect.Play();
                hostAddress.Remove(hostAddress.Length - 1, 1);
                resetText = true;
            }
            if (e.TypingAccepted) // User has entered a value
            {
                menuSelectSoundEffect.Play();
                lastAddress = hostAddress.ToString();
            }
            else if (e.TypingCancelled) // User has cancelled their input
            {
                menuCancelSoundEffect.Play();
                hostAddress.Clear();
                hostAddress.Append(lastAddress);
                resetText = true;
            }

            if (resetText) // Update the on-screen text
            {
                SetMenuEntryText();
            }
            return;
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void HostPortMenuEntrySelected(object sender, PlayerInputEventArgs e)
        {
            TypingInput = true;
            return;
        }

        /// <summary>
        /// Event handler for typing this into the host address.
        /// </summary>
        void HostPortMenuEntryTyped(object sender, PlayerInputEventArgs e)
        {
            bool resetText = false;
            if (e.KeysTyped != string.Empty && hostPort.Length < MAX_PORT_LENGTH) // Add a character
            {
                for (int i = 0; i < e.KeysTyped.Length; i += 1)
                {
                    if (char.IsDigit(e.KeysTyped[i]))
                    {
                        hostPort.Append(e.KeysTyped[i]);
                        resetText = true;
                    }
                }
                if (resetText) // If at least a key was found, type it
                {
                    typingSoundEffect.Play();
                }
            }
            if (e.TypingBackspace && hostPort.Length > 0) // Remove a character
            {
                typingSoundEffect.Play();
                hostPort.Remove(hostPort.Length - 1, 1);
                resetText = true;
            }
            if (e.TypingAccepted) // User has entered a value
            {
                menuSelectSoundEffect.Play();
                lastPort = hostPort.ToString();
            }
            else if (e.TypingCancelled) // User has cancelled their input
            {
                menuCancelSoundEffect.Play();
                hostPort.Clear();
                hostPort.Append(lastPort);
                resetText = true;
            }
            if (resetText) // Update the on-screen text
            {
                SetMenuEntryText();
            }
            return;
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ContinueMenuEntrySelected(object sender, PlayerInputEventArgs e)
        {
            int port;
            if (!int.TryParse(hostPort.ToString(), out port) || port <= MIN_PORT || port >= MAX_PORT)
            {
                return;
            }

            string hostAddressString = hostAddress.ToString().Trim();
            if (string.IsNullOrWhiteSpace(hostAddressString)) // Sanity
            {
                return;
            }

            GameClient gc = new GameClient();
            gc.ConnectToGame(hostAddressString, port);
            ScreenManager.AddScreen(new LobbyScreen(gc.Lobby));
            return;
        }

        #endregion
    }
}
