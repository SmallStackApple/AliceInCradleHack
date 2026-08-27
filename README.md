# AliceInCradleHack (摇篮中的爱丽丝黑壳)

![Build Status](https://img.shields.io/github/actions/workflow/status/SmallStackApple/AliceInCradleHack/build.yml?branch=main&style=plastic&logo=githubactions&logoColor=white)
![License](https://img.shields.io/github/license/SmallStackApple/AliceInCradleHack?style=plastic&color=blue)

An injection hack for Alice In Cradle

**[中文文档 / Chinese Version](README_zh-CN.md)**

## Introduction

This project is a pure .NET Framework 4.8.1 injectable hack tool for [AliceInCradle](https://cn.aliceincradle.dev), which provides basic modification features. It needs to be injected after the game is launched, and the initialization is completed by calling the `AliceInCradleHack.InjectEntry:Inject()` method.

## Build

1. Environment Requirements
    - Visual Studio 2019 or later (with the ".NET Desktop Development" workload selected)
    - .NET Framework 4.8.1 (in fact, .NET Framework 4.7.2 has also been verified to work)
2. Build Steps
    - Open AliceInCradleHack.sln
    - Click `Build(B)` -> `Build Solution(B)` in the top toolbar, or use the shortcut `Ctrl+Shift+B` to build
    - After successful build, the output files are located in [AliceInCradleHack\bin\Release](AliceInCradleHack\bin\Release)

## Usage

- Automatic Script Injection
    1. Visit [Release](https://github.com/SmallStackApple/AliceInCradleHack/releases) to download release-build.zip
    2. Extract release-build.zip
    3. Run `inject.bat` or `inject.ps1`
- Manual Injection
    1. Launch the game
    2. Copy all folders to the directory `C:\AliceInCradleHack\`
    3. Inject AliceInCradleHack.dll by any means and call the `AliceInCradleHack.InjectEntry:Inject()` method for initialization (you can use the [SharpMonoInjector](https://github.com/SmallStackApple/SharpMonoInjector/releases) built by me)
- Use the Latest Built Version
   - Visit [nightly](https://nightly.link/SmallStackApple/AliceInCradleHack/workflows/build/main/release-build.zip) to get the latest built version

## Author's Initiative

This project and all **derivative works** developed based on it are licensed under the GPLv3 open source license, with the core values of free sharing and win-win collaboration. To safeguard the purity of the open source spirit, the author hereby puts forward the following initiative, and earnestly requests all users and developers to abide by it together:

1. Paid Distribution is Not Recommended: This project and its derivative works are permanently open source. It is **recommended not to sell them as paid products, unlock content for a fee, or conduct disguised commercialization through advertisements, memberships, etc.**, so that technology can benefit more people without thresholds;
2. No Mandatory Interaction Required: When using, sharing or secondary developing this project and its derivative works, **it is not necessary to take "liking, following, collecting, forwarding" as prerequisites**, and respect the independent choice of every user;
3. Encourage Free Sharing: You are welcome to **freely share project links and derivative work results (while retaining the original copyright notice and open source license)** in accordance with the GPLv3 license, and jointly promote the healthy development of the community.

---

### Supplementary Notes

- This initiative is not in conflict with the core clauses of the GPLv3 license. Secondary development, distribution and other behaviors must still strictly comply with the license requirements (such as retaining copyright notices, open sourcing derivative works, and not adding additional restrictions, etc.);
- The copyright of this project and its derivative works belongs to the original author and contributors. Please comply with relevant laws, regulations and the GPLv3 license when using it.

## Extension Development

### Directory Layout

Each extension lives in its own first-level subfolder under `<mainFolder>\Extensions\` and is scanned
automatically during initialization. Every extension owns a private `lib\` subfolder for its
dependencies, so extensions never share dependency folders. DLLs placed directly under
`Extensions\` root are not loaded.

```
<mainFolder>\
├── Extensions\
│   ├── MyExtension\
│   │   ├── MyExtension.dll
│   │   └── lib\          <- this extension's private dependencies (optional)
│   └── AnotherExtension\
│       └── AnotherExtension.dll
```

An extension can read its own folder's absolute path via the `CurrentFolder` property (assigned by
`ExtensionManager` before `Initialize()` is called) to locate its config/data resources.

### Minimal Example

Inherit `AliceInCradleHack.extension.Extension` and implement `Initialize()` / `Dispose()`:

```csharp
public class MyExtension : Extension
{
    public override string Name => "MyExtension";
    public override string Description => "Example extension";

    public override void Initialize()
    {
        // Call existing managers or game APIs directly from here
        ModuleManager.Instance.EnableModule("Critical");
        CommandManager.Instance.RegisterCommand(new MyCommand());
    }

    public override void Dispose()
    {
        // Undo everything registered in Initialize
        CommandManager.Instance.UnregisterCommand("mycmd");
    }
}
```

### Rules

- Extensions run in the same AppDomain as the main program, so singletons like `ModuleManager`, `CommandManager` and `PatchManager` are directly accessible;
- Each extension owns a dedicated `Extensions\<extension>\` folder, with its dependencies in a private `lib\` subfolder;
- `CurrentFolder` is assigned before `Initialize()` and points to the extension's own folder; it is ready to use inside `Initialize()`;
- `Dispose()` must undo every command, module, Harmony patch, etc. that the extension registered;
- Extension DLLs cannot be unloaded from memory (they are ordinary injected assemblies); `Dispose` only releases managed resources, and the DLL is freed when the game process exits.

## Third-Party Licenses

This project is licensed under **GPL-3.0** (see [LICENSE](LICENSE)).

It uses, references, or bundles third-party software and assets, including (but not limited to):

- **Lib.Harmony**, **DiscordRichPresence**, **Newtonsoft.Json** (MIT), **MoonSharp** (BSD-like), **NAudio** (Ms-PL), and Microsoft **System.\*** packages (MIT, .NET Foundation) — see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the full list with copyright holders and license texts in [licenses/](licenses/).
- The configuration system under `config/` (including the `Value` mechanism) is adapted from **LiquidBounce** (GPL-3.0 © CCBlueX).
- The DynamicIsland HUD components and the IntroSplash screen are based on code originating from **OpenZen**, whose original bytecode grants no license (research/study use only per the upstream repository); rights holders may open an issue to request removal — though, to borrow the upstream's own words, filing one won't get you a response anyway.
- Game assemblies under `GameDLL/Managed` are unmodified files of **Alice In Cradle** © NanameHacha / hinayua, shared non-commercially with author attribution per the game's official program-sharing terms; they are used only as compile-time references and are not shipped in build outputs.
- The embedded Minecraft-style font glyphs are © Mojang Studios / Microsoft.
- The embedded audio tracks are © NEXON Games Co., Ltd. & Yostar, Inc. All Rights Reserved.

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for complete details and disclaimers.
