frangiclave-patch
=================

*A patch for Cultist Simulator enabling additional features for modding.*

**frangiclave-patch** is a C# solution for building a patch for the Cultist Simulator Assembly-CSharp DLL.
It uses [MonoMod](https://github.com/0x0ade/MonoMod) as a basis for patching it.

You shouldn't need to directly patch the game yourself, as this is something [frangiclave-mod-manager](https://gitlab.com/frangiclave/frangiclave-mod-manager) can handle.

License: ![CC0](https://licensebuttons.net/p/zero/1.0/88x15.png "CC0")

## Building

In order to build this patch, you will need to copy over the required dependencies first:

1. Copy every DLL file from your `Cultist Simulator/cultistsimulator_Data/Managed/` directory into the `CultistSimulator` directory of the solution.
2. Copy MonoMod and all its DLL dependencies into the `MonoMod` directory of the solution.
   - You will need to build or acquire MonoMod separately. Follow the instructions on its repository to build it. 
   It should produce an executable with several DLLs.
   
## Patching

Normally you will not need to apply the patch manually, as [frangiclave-mod-manager](https://gitlab.com/frangiclave/frangiclave-mod-manager) is capable of applying it by itself.
If you are developing the patch, however, you may find it useful to be able to do it yourself.
To do so, follow these instructions:

1. Copy all the DLLs from `CultistSimulator` and `MonoMod` into the build directory, where your built DLL is.
2. Run `MonoMod Assembly-CSharp.dll` and wait for the program to finish.
3. Copy the resultant `MONOMODDED_Assembly-CSharp.dll` to `Cultist Simulator/cultistsimulator_Data/Managed/`, along with MonoMod and its DLLs.
4. Remove the old `Assembly-CSharp.dll` (or rename it as a backup) and rename `MONOMODDED_Assembly-CSharp.dll` to `Assembly-CSharp.dll`.

Your game should now be correctly patched!