# HLNC

Simple yet powerful netcode for Godot!

## Table of contents
- [HLNC](#hlnc)
  - [Table of contents](#table-of-contents)
  - [Sample](#sample)
  - [Features](#features)
    - [Coming soon](#coming-soon)
  - [Getting started](#getting-started)
    - [Pre-setup notes](#pre-setup-notes)
    - [Setup steps](#setup-steps)
      - [Help](#help)
  - [Examples](#examples)
  - [Documentation](#documentation)
  - [What is this?](#what-is-this)
  - [Why?](#why)
  - [How to contribute](#how-to-contribute)

## Sample

```cs
using Godot;
using HLNC;

namespace MyGame
{
	public partial class PlayableCharacter : NetworkNode3D
	{
		[NetworkProperty]
		public int Money { get; set; }

		public OnNetworkChangeMoney(int _tick, int oldVal, int newVal)
		{
			// This is only called on the client-side
			GD.Print("I have " + newVal + " money now! Thanks, server.");
		}

		public override void _NetworkProcess(int _tick)
		{
			base._NetworkProcess(_tick);
			if (NetworkRunner.Instance.IsServer)
			{
				// This is only called on the server-side
				Money += 1;
			}
		}
	}
}
```

## Features

* Tick-aligned system
* Single state authority (Dedicated-server model)
* Input authority (cheat prevention)
* Data Interpolation

### Coming soon
* Interest management
* Rollback / Lag compensation (blocked by [Godot #2821](https://github.com/godotengine/godot-proposals/issues/2821))

## Getting started

### Pre-setup notes

* Requires Godot.NET (this is a C# library)
* Depends on a 3rd party library called [Fody (MIT Licensed)](https://github.com/Fody/Home)

### Setup steps

1. Add HLNC to your "addons"
2. Import HLNC in your `.csproj`. For example:
```xml
<Project Sdk="Godot.NET.Sdk/4.3.0-beta.1">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <Import Project="addons\HLNC\HLNC.props" />
</Project>
```
3. Enable the HLNC plugin in Godot

#### Help
Better documentation and demo projects are coming soon. In the meantime, feel free to [open a ticket](https://github.com/Heavenlode/HLNC/issues/new) with any questions or concerns.

## Examples
* [Bouncing Balls](https://github.com/Heavenlode/HLNC-Demo-BouncingBalls/tree/main)
* [Movable Players](https://github.com/Heavenlode/HLNC-Demo-MovablePlayers)

## Documentation

Coming soon

## What is this?

HLNC is a library for developing online multiplayer games with Godot. It is a tick-aligned system, respects input authority, and all state is managed by the server.

This library is still in BETA, therefore it is not quite production-ready and is subject to changes as we approach stability.

HLNC is an acronym for Heavenlode Netcode. The library originated from the author's own online game "Heavenlode" (not yet public).

## Why?

_(NOTE: It goes without saying that the Godot team is full of highly intelligent and awesome people. So, keep in mind this section is opinionated and not meant to be disparaging!)_

Ideally, the purpose of HLNC is to provide developers more confidence and control of their game's networking with a powerful abstraction layer that allows quick and easy development. At minimum, it provides an *alternative* over Godot's synchronization/RPCs.

The author of HLNC had experience using another popular networking library called [Photon Fusion](https://www.photonengine.com/fusion). Those familiar with Fusion might immediately recognize that HLNC is heavily inspired by it.

Godot provides its own Multiplayer API which includes RPCs and synchronizers. However, the author is of the opinion that these are too opaque to feel confident using them for highly performant and/or competitve games.

For example, UDP packets should stay under ~1500 bytes to mitigate packet-splitting and other network instability issues. It isn't clear how Godot addresses this--if at all. HLNC makes it easy for developers to keep their data well within these configurable limits.

Also, Godot encourages utilizing RPCs. The author feels that using RPCs for game networking is clunky, error-prone, and unreliable at scale. HLNC uses a tick-based system wherein the game state is serialized and transferred over the wire using user-friendly abstractions which avoid RPCs.

## How to contribute

If you have questions, comments, or concerns, please feel free to [open a ticket](https://github.com/Heavenlode/HLNC/issues/new).

If you want to contribute code and/or documentation, you're amazing! To maximize your odds of your feature/PR getting merged in, you are encouraged to first [open a ticket](https://github.com/Heavenlode/HLNC/issues/new) with your proposal. Once approved, any and all work is very much appreciated.