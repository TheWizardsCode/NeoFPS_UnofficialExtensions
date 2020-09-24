# NeoFPS_UnofficialExtensions
This is a set of scripts that we've written for games using [NeoFPS](https://bit.ly/NeoFPS), an excellent first person asset in the Unity Asset Store. Feel free to use them. We'd love for you to contribute to them too. 

Code here is built for specific purposes and it works for that purpose. We encourage reuse but we don't offer any kind of guarantee that it will work in your use case. 
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

## Emerald AI

We've added a blood loss system to the official Neo FPS [Emerald AI](http://bit.ly/EmeraldAI) integration, you can see how it works in our [Weekend Hack: FPS Hunter Game](https://www.youtube.com/watch?v=I27gpQKw_jM&t=81).

## Score System

The Score Syste allows points to be scored for hits, kills, criticals and deducted for shots. To see it in action see our [Weekend Hack: FPS Hunter Game](https://www.youtube.com/watch?v=I27gpQKw_jM&t=241)
