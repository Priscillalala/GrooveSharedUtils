# GrooveSharedUtils
A RoR2 utilities pack to streamline mod setup, development, and organization. GrooveSharedUtils focuses on runtime-based content creation.

![](https://cdn.discordapp.com/attachments/894751893421707305/1019015317990690816/BWE_DOOO_DU_DUUUUU.png)

## Key Features
Groove's Shared Utils can be used individually in most cases, with cross compatibility between features.

### Mod Plugins and Modules
A `ModPlugin` inherits from BepInEx's `BaseUnityPlugin`; each assembly is limited to a single `ModPlugin`. Your mod plugin servers as both a BepInEx plugin and a content pack, and additionally instantiates and manages classes that inherit from `ModModule` in your assembly. Mod modules are mono behaviours with a `LoadContent` method that is iterated over when your plugin is populating its content pack. Assets yielded during `LoadContent` will be mapped to their appropriate locations (`ItemDefs` and `EquipmentDefs` to the content pack, `ModdedDotDefs` to DoTAPI, etc.). `ModPlugin<T>` and `ModModule<T>` provide access to the class instances.

### Asset Display Cases
The `AssetDisplayCase` attribute can be applied to any class, and indicates that static fields in the class and its nested classes can serve as holders for your mod's assets (think `RoR2Content`). Assets can be displayed at any time with `AssetDisplayCaseAttribute.TryDisplayAsset`, and assets yielded while a module is loading content will be automatically displayed. Displayed assets will be mapped to any asset display case fields, matched by type and name.

### Frames
Frames are helper classes to assist with a specific runtime task. For example an `ItemFrame` wraps RoR2's `ItemDef`, creating the scriptable object while exposing relevant fields to the user. Frames can be built directly or yielded as an `IEnumerator` such as during `ModModule.LoadContent`. Your assembly's frames can be customized through the `Frame.Settings` assembly attribute and the `Frame.DefaultExpansionDef` attribute applied to any `ExpansionDef` field, method, or property.  GrooveSharedUtil's library of frames is never complete, please reach out with suggestions for new frames!

### Configurable Attribute
`Configurable` can be applied to any static field or mod module. The attribute's config file name, section, default value, etc. can be set manually, but they will also be inferred from the field/module and, if present, your assembly's `ConfiguableAttribute.Settings` attribute.

### Load Asset Bundle Attribute
This one is pretty self-explanatory...add the `LoadAssetBundle` attribute to any `AssetBundle` field to assign its value to the loaded asset bundle file and optionally swap all stubbed shaders in the bundle. The bundle name will be inferred from the field name, but can be explicitly assigned in the attribute alongside the path to the bundle's folder. Use the `LoadAssetBundleAttribute.Settings` assembly attribute to assign a default asset bundle folder assembly wide.

### Language Collections
A `LanguageCollection` is designed to mirror a traditional language file. Language collections are discovered with the `LanguageCollectionProvider` attribute applied to a static method returning a `LanguageCollection`. Strings in a language collection are easy to dynamically modify, as they are a part of your project. Language collections keep your mod's language organized and translatable as a text file.

### GSUtil and Common
`GSUtil` contains many helpful methods for general mod development. `Common` contains frequently used assets and events. 

### And More!
Well, not *that* much more...but it's still an early release.

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
