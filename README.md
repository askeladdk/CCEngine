# CCEngine

Frustrated by all the failed, halted, and delayed attempts to recreate the C&C and RA1 game engines, I am giving it my own shot.

My aim is for CCEngine to emulate the original games as closely as possible, but with no hardcoded values and greater extensibility. The original game assets will be used whenever possible.

## FAQ

### Which games will be supported?

Only Tiberian Dawn and Red Alert 1 will be supported. I am focusing my attention on RA1 first because it is a more advanced game.

### Why bother when OpenRA already exists?

OpenRA strays too far from the original gameplay and is not a drop-in replacement for the original games. I may also be suffering from Not-Invented-Here syndrome.

### Will you make improvements to the games?

My aim is to emulate the original games as closely as possible, but I do want to make some interface additions such as right-click scrolling, shift-add units to groups, attack-move command, build queues, and better sidebar cameo sorting. CCEngine will also allow defining new units and such.

### What is done so far?

- Basic engine framework (work in progress).
- Hardware accelerated sprite rendering.
- Virtual file system loads nested and encrypted .mix archives.
- Loading palettes, sprites, images, string tables, maps (partially) and .ini files.

### What is left to do?

- User interface.
- Map rendering (in progress).
- Game mechanics.
- Campaigns.
- Multiplayer.
- Save games.
- AI and path finding.
- Loading and displaying fonts, animations, movies, sound effects and music.
