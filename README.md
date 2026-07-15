![](screenshot.png)

Depends on a modified version of MonoGame (https://github.com/JSandusky/MonoGame/commits/develop/) that exposes CBuffers for raw data writes among a few other things. Attempts to build will also require path setup for your OpenCL SDK and the FBX-SDK, this project was simply never intended to be built by outsiders or more than a single person.

itch.io page: https://spruekit.itch.io/spruetex

This began in C++/QT5 and was migrated over to WPF/C# + C++/CLI. Most of the unused C++ code is expected to work (where not obviously stubbed) as the primary issues were QT related. C++/CLI pretty much damned the project though and it was time for either another heavy refactor (back to more C++ centric so 3rd party integrations of the runtime were more viable).

Interesting parst are mostly going to be in SprueKit/Data

Was first a Spore style creature modeler, but procedural texture-graphs were more viable as the rapid escalation into Photogrammetry everywhere started taking off so it went that direction. DON'T try to do a spore thingy like this, at the time SDFs and sampling-space deformers were in extra-extra-vogue. **Stick with meta-balls, SDFs are a trap.**
