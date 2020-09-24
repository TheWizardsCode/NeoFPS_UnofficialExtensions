# NeoFPS_UnofficialExtensions
This is a set of scripts that we've written for games using [NeoFPS](https://bit.ly/NeoFPS), an excellent first person asset in the Unity Asset Store. Feel free to use them. We'd love for you to contribute to them too. 

Code here is built for specific purposes and it works for that purpose. We encourage reuse but we don't offer any kind of guarantee that it will suit your use case. 
If you can improve the code we will gladly accept a patch. If you are having problems using it raise an issue and maybe we'll be able to help, but no promises. Do please 
help others if you use this code, more voices makes this more useful.

***Important***

Node Canvas doesn't currently set a scripting define symbol when it is installed. This means that if you don't have Node Canvas installed you will get a compilation error
when adding this code to your project. There are three easy ways you can fix this:

1) Delete the Node Canvas folder
2) Add a scriptiung defines symbol of `NODE_CANVAS_PRESENT` see Edit -> Player Settings... -> Player (tab) -> Scripting Define Symbols  (scroll down to find it)
3) Purchase and install [Node Canvas](https://bit.ly/NodeCanvas)

## Node Canvas

A set of actions and conditions for [Node Canvas](https://bit.ly/NodeCanvas) plus some sample finite state machines and behaviour trees to provide AI to your characters.

### Character Setup

Setup of your NPCs is fairly simple add the following components. All components have tooltips for their parameters to help in setting things up:

  * `AiBaseCharacter` - Defines the basic characters of an NPC, this provides the required `ICharacter` implementation for NeoFPS to be aware of the character.
  * `AiBaseInventory` - This is a modified version of the [QuickSwitchInventory](https://docs.neofps.com/manual/inventoryref-mb-fpsinventoryquickswitch.html) in NeoFPS
  * [OPTIONAL] `SimpleLocomotionController` - converts NavMesh movement to animation controller parameters, you need to provide your own animation controller. If you aren't using Navmesh for movement you don't need this.
  * [OPTIONAL] `AiSounds` - define AI sounds and provide public methods for playing them (you can also use animation events to play them)
  * [OPTIONAL] `SoundSource` - make sounds made by this AI audible to other AIs which will change the AIs awareness level and can be used to make those AIs approach the sound source

You will also need (at least) the following standard NeoFPS components:

  * `BasicHealth Manager`
  * `BasicDamagerHandler`(s)
  * [Optional] `SimpleSurface`(s)
  
AI's should be on the layer `CharacterControllers`

### AI Weapon Setup

AI Weapons do not use the modular firearms system. This decision was made for performance reasons (fewer components) and because the game this was developed for require melee weapons not firearms. Work is needed to make it support ranged weapons, it's not clear at this point whether it would be best to reuse parts of the modular firearm system or not. The `AiBaseWeapon` is designed to allow flexibility in the implementation - patches welcome.

To setup a weapon add the following components and then make the weapon available to the AI via its inventory.

  * `AiBaseWeapon` - Defines the characteristics of the weapon such as range and damage. 
  * `FpsInventoryWieldable` - this is from the NeoFPS asset and is unchanged for the AI.
  
### Node Canvas Setup

There's nothing special about setting up Node Canvas for your AIs. Just add either an `FSMowner` or `BehaviourTreeOwner`. All available actions and conditions are in the `NeoFPS` category. 

See the `FSM` and `Behaviour Tree` folders for examples - please send us more!

## Emerald AI

We've added a blood loss system to the official Neo FPS [Emerald AI](http://bit.ly/EmeraldAI) integration, you can see how it works in our [Weekend Hack: FPS Hunter Game](https://www.youtube.com/watch?v=I27gpQKw_jM&t=81).

## Score System

The Score Syste allows points to be scored for hits, kills, criticals and deducted for shots. To see it in action see our [Weekend Hack: FPS Hunter Game](https://www.youtube.com/watch?v=I27gpQKw_jM&t=241)
