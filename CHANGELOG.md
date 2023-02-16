## 0.5.1
* Add `ArtifactActionCatalog` to artifact actions on enable/disable
* Add `enabledAction` and `disabledAction` to `ArtifactFrame`
* Rename `selectedIcon` and `deselectedIcon` to `enabledIcon` and `disabledIcon` respectively in `ArtifactFrame`
* Fix `ArtifactFrame.SetArtifactCode(Top/Middle/Bottom)Row` having no effect
* `LazyAddressable` now defaults to not ensuring completion. I am still not 100% happy with lazy addressables and will continue to look at them
* Add `ModelPanelParametersInfo` to represent general information about `ModelPanelParameters` 
* Add `logbookModelParameters` and `SetLogbookModelParameters` to `ItemFrame` to merge `GSUtil.SetupModelPanelParameters` into the frame
* Add more overloads to `GSUtil.SetupModelPanelParameters`
* Rename `OverlayManager` and `EarlyAchievementManager` and `EliteTierManager` to `OverlayCatalog` and `EarlyAchievementCatalog` and `EliteToTierManager` respectively
* `EquipmentActionsCatalog` uses a function instead of a delegate to improve readability
* Add `ArtifactCompounds` class to `Common` to represent vanilla `ArtifactCompoundDef` and int value information
* Add `GSUtil.TryFindArtifactCompoundDef` and `GSUtil.FindArtifactCompoundDef` to locate the appropriate `ArtifactCompoundDef` for a compound int value
* Add `CopyToFormulaDisplay` method to `ArtifactCodeInfo` to setup a `ArtifactFormulaDisplay` component based on the code info
* Remove `GSUtil.ModLoadedCached` because it really wasn't worth caching
* Add support to `ModPlugin` for BepInEx incompatability, dependency, etc. attributes
* Add `moddedDifficultyDefs` field to `ExtendedSerializableContentPack`

## 0.5.0
* Early release
