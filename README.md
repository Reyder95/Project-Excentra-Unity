# Project Excentra

## Introduction
Project Excentra is a JRPG-styled game where you follow the story of four girls trying to uncover the truth of the "Old World". At the current moment, my initial plan is to build a little tech demo with a tutorial, and one boss. Once that's polished I'll be building out more of the game if I can get the demo to a spot where I'm happy.

**Note:** The things described below are goals for the game, but almost nothing is implemented at the moment. I hope to continue building this over time.

## Combat System
The combat system follows a similar vein to games like Honkai: Star Rail, or The Legend of Heroes: Trails series, in which each "unit" in a battle is given a delay value, and this delay value determines that unit's battle order. Delay is calculated using a unit's "speed" value, so faster speeds mean you will go more often. This allows for a dynamic turn-based system, that's able to be built around and customized. 

If you're a theorycrafter, you can craft together a build that makes a unit go many times before others, or you may prefer a slower character that hits harder. These are the goals of Excentra.

Additionally, the turn-based battles will function similar to that of an MMO. There will be Tank, Healer, and Damage focused classes that a unit can become. Bosses will have unique mechanics, akin to puzzle mechanics of raids. There will also be "Aggression" generation, which determines how a boss or enemy will target your group.

You can think of the game as "puzzle meets RPG", if you want.

## Class System
There will be a moderate amount of classes to choose from, each following the "trinity" of MMOs: Tank, Healer, Damage. Tanks tend to generate Aggression better, while Healers have many healing and support utilities. Damage focus on simply dealing damage, while having additional ways of buffing the group (some damage dealers have better buffs than others). Each class also has a skill tree that allows for full customization. Additionally, I am planning on allowing for cross-class builds. In which you can choose skills from one class to use in another (how I will do this, I'm not sure of right now).

## Itemization
For itemization, I wanted to follow a style similar to the Trails series (each character having an item build, and slotting gear), but I wanted to incorporate ARPG itemization, such as randomized stats and stat-crafting. In that, there will be multiple rarities: Normal, Magic, Rare, and Unique. Other than Uniques, all items will have randomized stats depending on the rarity. But Uniques will be static in terms of stats, but they may roll randomly. Uniques will be only obtained through specific means, such as defeating a boss or opening up a specific hidden chest. 

This is the main reason I'm building Excentra, honestly.

## Current Progress
- **Turn Based Functionality** - There is a complete turn based battle system with delays that calculate based on a speed stat. Faster characters act more often on average than slower ones. The delay formula definitely needs adjustment, though.
- **Combat** - Basic attacks and special attacks both work. Health bars and Aether (resource) bars drop respectively. HP reaching zero kills an Entity, and you can revive them back to life. All respective animations play as well. Gap closers exist as well!
- **Aggression Generation** - The tank will, on average, have more aggression generated allowing him to tank the enemies more effectively.
- **Boss AI and Mechanics** - All enemies have fully fledged AI systems with mechanics and phases. Fully detailed trigger and custom logic system, allowing for the simplest of mechanics to work easily, and the most complex mechanics allowing full control over how it should operate. With multiple "levels" of logic. This allows for the most complex mechanics to be possible, and for insane scalability.
- **Status Effects** - Various status effects exist, such as burning, poison, or stat modifications (increased armor, attack, etc). Allows for special status effects that have custom logic built into them based on mechanic logic.
- **Particle System** - Fairly work in progress to work completely, but currently supports custom particles on allies when specific special debuffs are applied, allowing for clarity during the fight.