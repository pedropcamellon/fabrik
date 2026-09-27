# Fabrik

A tiny Unity first-impression probe: place one package in a supplied warehouse and watch it move between two points. No custom factory or industrial integrations.

## Open

Open the **repository root** in Unity Hub with Unity Editor **6000.6.3f1** (Apple Silicon). `Assets/`, `Packages/`, and `ProjectSettings/` are the Unity project; `Library/`, `Logs/`, and `Temp/` are generated and ignored by Git.

If Unity warns that `Assets/Scenes/SampleScene.unity` contains an old Light serialization version, double-click that scene in the Project panel and press **Cmd+S**. Do not edit the scene file as text.

## One warehouse, one package

1. Add the free [Old Warehouse](https://assetstore.unity.com/packages/3d/props/industrial/old-warehouse-116767) to your Unity Asset Store account. In Unity, open **Window > Package Manager > My Assets**, download it, and import its package. It is an older built-in-render-pipeline asset; visual quality in Unity 6 has **not** been verified.
2. Open `Assets/Scenes/SampleScene.unity` and drag the imported warehouse prefab into the scene. Select a floor object or the warehouse near an unobstructed two-meter stretch.
3. Choose **Tools > Fabrik > Add Package Transfer**. Select the new `Fabrik Transfer` object and adjust its position if the blue package starts inside geometry. Press **Play**: the cube travels two meters in three seconds, then returns in three seconds, repeating.
4. Save the scene as `Assets/Scenes/Fabrik.unity` with **File > Save As**. Reopen it and press Play again to verify the setup survived saving.

The package is animated, not physically conveyed. The Asset Store content requires a Unity account and is not bundled in this repository; check its license before redistributing it. No one has observed the imported warehouse or the package motion yet.

## First impression

| Question | Observation |
| --- | --- |
| Setup friction | Pending hands-on run |
| Visual satisfaction | Pending hands-on run |
| Deserves another session? | Pending hands-on run |
