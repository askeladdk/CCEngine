# CCEngine
---

> He who controls the past, commands the future.
>
> He who commands the future, conquers the past.
>
> -- Kane

## About

CCEngine is my attempt to recreate the C&C: Tiberian Dawn and Red Alert 1 games in a single engine. My aim is to emulate the original games as closely as possible with as little hard coded values as possible, so as to make it easy to extend the games as well as the engine itself. The original game assets will be used whenever possible. I am focusing my attention on RA1 first because it is a more advanced game.

CCEngine is programmed in C# on the .NET Core platform with OpenTK (OpenGL + OpenAL).

## Roadmap

What is done and what remains to be done?

### Engine parts

* [x] `Audio` - Music and sounds.
* [x] `Renderer` - Hardware accelerated and capable of post-processing effects.
* [x] `GUI` - Handle user interactions (in progress).
* [x] `Asset Manager` - Loads files from folders and (nested) `.mix` files.
* [x] `Entity Component System` - Organise code and data related to game entities such as infantry, vehicles, projectiles, etc.
* [ ] `Game Mechanics` - Simulate the interactions between entities.
* [ ] `Tactical AI` - Intelligent pathfinding and target acquisition.
* [ ] `Strategic AI` - Capable of planning to defeat the player.
* [ ] `Campaigns` - Map loading, trigger system, mission progression.
* [ ] `Multiplayer` - This is a looong way off.
* [ ] `Map editor` - Eventually I would like to have an integrated editor to make your own single- and multiplayer maps.

### File types

These are all the file types that the engine can understand and use.

* [x] `.aud`
* [x] `.cps`
* [x] `.fnt`
* [x] `.ini`
* [x] `.mix`
* [x] `.pal`
* [x] `.pcx`
* [x] `.shp`
* [x] `.tem`, `.sno`, `.int`, (etc)
* [ ] `.vqa`
* [ ] `.vqp`
* [ ] `.wsa`

### Supported platforms

CCEngine is cross-platform by default as it is built on .NET Core. I have no plans to support mobile platforms as the games simply were not designed for it.

* [x] `Windows`
* [x] `OSX`
* [x] `Linux` (not tested yet)

## FAQ

### Why bother when OpenRA already exists?

OpenRA strays too far from the original gameplay and is not a drop-in replacement for the original games. I may also be suffering from Not-Invented-Here syndrome.

### Will you make improvements to the games?

My aim is to emulate the original games as closely as possible, but I do want to make some interface additions such as right-click scrolling, shift-add units to groups, attack-move command, build queues, and better sidebar cameo sorting. It will also be possible to define new units and such.
