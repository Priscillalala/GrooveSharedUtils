# GrooveSharedUtils
A RoR2 utilities pack to streamline mod setup, development, and organization. GrooveSharedUtils focuses on runtime-based content creation.

![](https://cdn.discordapp.com/attachments/894751893421707305/1019015317990690816/BWE_DOOO_DU_DUUUUU.png)

## Structure
Most of GrooveSharedUtils is build around a specific mod archecture that should be easy to setup and expand.
### Mod Plugins and Modules
A `ModPlugin` inherits from BepInEx's `BaseUnityPlugin`; each assembly is limited to a single `ModPlugin`. Your plugin serves as the base of a mod made with GrooveSharedUtils: PLUGIN options define a BepInEx plugin, ENV (Environment) options define global settings for your mod, and the plugin additionally functions as a content pack. Your plugin instantiates and manages classes that inherit from `ModModule` in your assembly.
### Asset Display Cases and Streams
The `AssetDisplayCase` attribute can be applied to any class, and indicates that static fields in the class and its nested classes can serve as holders for your mod's assets (think `RoR2Content`). Every module implements the `OnCollectContent` method, which provides access to an `AssetStream`. This method is called as your plugin is building its content pack. Assets added to the asset stream will be mapped to their appropriate locations (`ItemDefs` and `EquipmentDefs` to the content pack, `ModdedDotDefs` to DoTAPI, etc.) and will also be mapped to any asset display case fields, matched by type and name.

## Frames
Frames are helper classes to assist with a specific runtime task. For example an `ItemFrame` wraps RoR2's `ItemDef`, creating the scriptable object while exposing relevant fields to the user. GrooveSharedUtil's library of frames is never complete, please reach out with suggestions for new frames.

With special thanks to TeamMoonstorm for MoonstormSharedUtils' stubbed shaders.
<details>
<summary>MSU License</summary>
<br>
Copyright © 2022 TeamMoonstorm

Permission is hereby granted, free of charge, to any person depending on this software and associated documentation files (the "Software") to deal in the software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense.

Other software can reuse portions of the code as long as this License alongside a "Thanks" message is included on the using software's readme

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the software

All rights are reserved.
</details>
