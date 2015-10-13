﻿#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2011 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Nuclex.Input.Devices;

using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;
using XnaEventHandler = System.EventHandler<System.EventArgs>;

namespace Nuclex.Input {

  /// <summary>Manages and polls input devices</summary>
  public class InputManager :
    IInputService, IGameComponent, IUpdateable, IDisposable {

    /// <summary>Fired when the UpdateOrder property changes its  value</summary>
    public event XnaEventHandler UpdateOrderChanged;

    /// <summary>Fired when the Enabled property changes its value</summary>
    public event XnaEventHandler EnabledChanged { add { } remove { } }

    /// <summary>Initializes a new input manager</summary>
    /// <remarks>
    ///   This overload is offered for convenience and takes the window handle
    ///   from XNA's Mouse class. It will only work if your game is either based
    ///   on the XNA Game class or if you assign the Mouse.WindowHandle
    ///   property sourself.
    /// </remarks>
    public InputManager() : this(XnaMouse.WindowHandle) { }

    /// <summary>Initializes a new input manager</summary>
    /// <param name="windowHandle">Handle of the game's main window</param>
    public InputManager(IntPtr windowHandle) : this(null, windowHandle) { }

    /// <summary>Initializs a new input manager</summary>
    /// <param name="services">Game service container the manager registers to</param>
    public InputManager(GameServiceContainer services) :
      this(services, XnaMouse.WindowHandle) { }

    /// <summary>Initializs a new input manager</summary>
    /// <param name="services">Game service container the manager registers to</param>
    /// <param name="windowHandle">Handle of the game's main window</param>
    public InputManager(GameServiceContainer services, IntPtr windowHandle) {

#if !NO_DIRECTINPUT
      if (DirectInputManager.IsDirectInputAvailable) {
        this.directInputManager = new DirectInputManager(windowHandle);
      }
#endif

      setupGamePads();

      if (services != null) {
        this.gameServices = services;
        this.gameServices.AddService(typeof(IInputService), this);
      }
    }

    /// <summary>Immediately releases all resources owned by the instance</summary>
    public void Dispose() {
      if (this.gameServices != null) {
        object inputService = this.gameServices.GetService(typeof(IInputService));
        if (ReferenceEquals(inputService, this)) {
          this.gameServices.RemoveService(typeof(IInputService));
        }

        this.gameServices = null;
      }

      if (this.gamePads != null) {
        CollectionHelper.DisposeItems(this.gamePads);
        this.gamePads = null;
      }

#if !NO_DIRECTINPUT
      if (this.directInputManager != null) {
        this.directInputManager.Dispose();
        this.directInputManager = null;
      }
#endif
    }

    /// <summary>All game pads known to the system</summary>
    public ReadOnlyCollection<IGamePad> GamePads {
      get { return this.gamePads; }
    }

    /// <summary>Returns the game pad for the specified player</summary>
    /// <param name="playerIndex">Player whose game pad will be returned</param>
    /// <returns>The game pad of the specified player</returns>
    /// <remarks>
    ///   This will only return the XINPUT devices (aka XBox 360 controllers)
    ///   attached. Any standard game pads attached to a PC can only be
    ///   returned through the ExtendedPlayerIndex overload where they will
    ///   take the places of game pads for player 5 and upwards.
    /// </remarks>
    public IGamePad GetGamePad(PlayerIndex playerIndex) {
      return this.gamePads[(int)playerIndex];
    }

    /// <summary>Returns the game pad for the specified player</summary>
    /// <param name="playerIndex">Player whose game pad will be returned</param>
    /// <returns>The game pad of the specified player</returns>
    public IGamePad GetGamePad(ExtendedPlayerIndex playerIndex) {
      return this.gamePads[(int)playerIndex];
    }

    /// <summary>Updates the state of all input devices</summary>
    /// <remarks>
    ///   <para>
    ///     If this method is called with no snapshots in the queue, it will
    ///     query the state of all input devices immediately, raising events
    ///     for any changed states. This way, you can ignore the entire
    ///     snapshot system if you just want basic input device access.
    ///   </para>
    ///   <para>
    ///     If this method is called while one or more snapshots are waiting in
    ///     the queue, this method takes the next snapshot from the queue and makes
    ///     it the current state of all active devices.
    ///   </para>
    /// </remarks>
    public void Update() {
      if (this.snapshotCount > 0) {
        --this.snapshotCount;
      }

      for (int index = 0; index < this.gamePads.Count; ++index) {
        this.gamePads[index].Update();
      }
    }

    /// <summary>Takes a snapshot of the current state of all input devices</summary>
    /// <remarks>
    ///   This snapshot will be queued until the user calls the Update() method,
    ///   where the next polled snapshot will be taken from the queue and provided
    ///   to the user.
    /// </remarks>
    public void TakeSnapshot() {
      for (int index = 0; index < this.gamePads.Count; ++index) {
        this.gamePads[index].TakeSnapshot();
      }

      ++this.snapshotCount;
    }

    /// <summary>Number of snapshots currently in the queue</summary>
    public int SnapshotCount {
      get { return this.snapshotCount; }
    }

    /// <summary>
    ///   Indicates when the game component should be updated relative to other game
    ///   components. Lower values are updated first.
    /// </summary>
    public int UpdateOrder {
      get { return this.updateOrder; }
      set {
        if (value != this.updateOrder) {
          this.updateOrder = value;
          OnUpdateOrderChanged();
        }
      }
    }

    /// <summary>Fires the UpdateOrderChanged event</summary>
    protected void OnUpdateOrderChanged() {
      if (UpdateOrderChanged != null) {
        UpdateOrderChanged(this, EventArgs.Empty);
      }
    }

    /// <summary>Only exists to implement IGameComponent</summary>
    void IGameComponent.Initialize() { }

    /// <summary>Updates the state of all input devices</summary>
    /// <param name="gameTime">Not used</param>
    void IUpdateable.Update(GameTime gameTime) {
      Update();
    }

    /// <summary>Whether the component is currently enabled</summary>
    bool IUpdateable.Enabled {
      get { return true; }
    }

    /// <summary>Sets up the collection of available game pads</summary>
    private void setupGamePads() {
      var gamePads = new List<IGamePad>();

      // Add default XNA game pads
      for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; ++player) {
        gamePads.Add(new XnaGamePad(player));
      }
#if !NO_DIRECTINPUT
      // Add DirectInput-based game pads
      if (this.directInputManager != null) {
        gamePads.AddRange(this.directInputManager.CreateGamePads());
      }
#endif
      // Add place holders for all unattached game pads
      while (gamePads.Count < 8) {
        gamePads.Add(new NoGamePad());
      }

      this.gamePads = new ReadOnlyCollection<IGamePad>(gamePads);
    }

#if !NO_DIRECTINPUT
    /// <summary>Manages DirectInput, if DirectInput is installed</summary>
    private DirectInputManager directInputManager;
#endif
    /// <summary>Collection of all game pads known to the system</summary>
    private ReadOnlyCollection<IGamePad> gamePads;

    /// <summary>Number of state snap shots currently queued</summary>
    private int snapshotCount;

    /// <summary>
    ///   Controls the order in which this game component is updated relative
    ///   to other game components.
    /// </summary>
    private int updateOrder = int.MinValue;
    /// <summary>Game service container, saved to unregister on dispose</summary>
    private GameServiceContainer gameServices;

  }

} // namespace Nuclex.Input

