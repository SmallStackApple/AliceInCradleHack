# Third-Party Notices

AliceInCradleHack is licensed under the GNU General Public License v3.0 (see [LICENSE](LICENSE)).

This project uses, references, or bundles the third-party software and assets listed below.
Each entry retains the copyright and license terms of its respective owner. Full license
texts are provided in the [licenses](licenses/) directory unless noted otherwise.

---

## 1. NuGet Dependencies

### Lib.Harmony 2.4.2

- License: MIT License
- Copyright (c) 2017 Andreas Pardeike
- Project: https://github.com/pardeike/Harmony
- Full text: [licenses/MIT-Harmony.txt](licenses/MIT-Harmony.txt)

### DiscordRichPresence 1.6.1.70

- License: MIT License
- Copyright (c) 2021 Lachee
- Project: https://github.com/Lachee/discord-rpc-csharp
- Full text: [licenses/MIT-DiscordRichPresence.txt](licenses/MIT-DiscordRichPresence.txt)

### MoonSharp 2.0.0.0

- License: BSD-like license (see full text)
- Copyright (c) 2014-2016 Marco Mastropaolo
- Project: https://github.com/moonsharp-devs/moonsharp
- Full text: [licenses/MoonSharp.txt](licenses/MoonSharp.txt)

### NAudio / NAudio.Core / NAudio.WinMM 2.3.0

- License: Microsoft Public License (Ms-PL)
- Copyright (c) Mark Heath and contributors
- Project: https://github.com/naudio/NAudio
- Full text: [licenses/Ms-PL.txt](licenses/Ms-PL.txt)

### Newtonsoft.Json 13.0.4

- License: MIT License
- Copyright (c) 2007 James Newton-King
- Project: https://github.com/JamesNK/Newtonsoft.Json
- Full text: [licenses/MIT-Newtonsoft.Json.txt](licenses/MIT-Newtonsoft.Json.txt)
- Note: this library is merged into the main `AliceInCradleHack.dll` via ILRepack and is
  therefore redistributed as part of the build output.

### System.Buffers 4.6.1 / System.Memory 4.6.3 / System.Numerics.Vectors 4.6.1 / System.Runtime.CompilerServices.Unsafe 6.1.2

- License: MIT License
- Copyright (c) .NET Foundation and Contributors
- Project: https://github.com/dotnet/runtime
- Full text: [licenses/MIT-DotNet.txt](licenses/MIT-DotNet.txt)

All of the licenses above are compatible with GPLv3. Build-time tooling
(ILRepack.Lib.MSBuild.Task, Apache-2.0) is used only during compilation and is not
distributed with the build output.

---

## 2. Design References and Derived Code

### LiquidBounce (config system)

The configuration system under [`AliceInCradleHack/config`](AliceInCradleHack/config)
— including the `ConfigSystem` registry and the `Value` mechanism (typed values, change
interception/notification, immutable/hidden flags, JSON serialization) — is adapted from
**LiquidBounce** (https://github.com/CCBlueX/LiquidBounce).

- License: GNU General Public License v3.0
- Copyright (c) CCBlueX
- LiquidBounce is licensed under GPL-3.0, the same license as this project (see
  [LICENSE](LICENSE)); the two are therefore fully compatible.

### OpenZen (DynamicIsland and IntroSplash)

The HUD "dynamic island" components under
[`AliceInCradleHack/module/modules/client/island`](AliceInCradleHack/module/modules/client/island)
and the intro splash screen
[`AliceInCradleHack/utils/client/IntroSplash.cs`](AliceInCradleHack/utils/client/IntroSplash.cs)
are based on code originating from **OpenZen** (https://github.com/Margele/OpenZen).

- OpenZen is a deobfuscated/republished client whose original obfuscated bytecode grants
  **no license**; the upstream repository states its contents are for research and study
  purposes only. No rights beyond that have been granted by the original author.
- The code is retained here for reference and interoperability. If you are the original
  author of OpenZen and would like this material removed or relicensed, please open an
  issue in this repository — although, in the spirit of the upstream's own words, feel
  free to file one, not that anyone will ever respond.

Note: the spring animation utility
([`AliceInCradleHack/utils/animation/SpringAnimation.cs`](AliceInCradleHack/utils/animation/SpringAnimation.cs))
that these components use has been rewritten as an independent implementation
(standard damped-spring physics with semi-implicit Euler integration) and contains no
OpenZen-derived code.

---

## 3. Game Program Files (GameDLL/Managed)

The assemblies under [`AliceInCradleHack/GameDLL/Managed`](AliceInCradleHack/GameDLL/Managed)
are unmodified program files of the game **Alice In Cradle**.

- Copyright: NanameHacha / hinayua (the game's authors)
- Official channel: https://cn.aliceincradle.dev
- These files are shared non-commercially and without any modification, in accordance with
  the game's official program-sharing terms, which require crediting the authors and their
  official distribution channel.
- They are used **only as compile-time references** so that this project can build against
  the game's API. They are **not** copied into, merged into, or distributed with the build
  output (`Private=False` for all such references).

### Mod disclaimer (per the game's official mod terms)

- This project is a community creation and is **not affiliated with or endorsed by** the
  game's authors.
- Using this tool may cause unexpected issues such as game bugs or save corruption; such
  issues are not supported by the game's authors.
- This project is non-commercial and is not used to create derivative works with terrorism,
  fascism, cult, or other objectionable tendencies.

---

## 4. Font — Minecraft.otf

The embedded font [`AliceInCradleHack/resources/fonts/Minecraft.otf`](AliceInCradleHack/resources/fonts/Minecraft.otf)
contains the "Minecraft" pixel glyphs.

- Glyph designs: Copyright Mojang Studios / Microsoft. All rights reserved.
- The font file was extracted from the game's bitmap font using a community tool licensed
  under the Apache License 2.0; the Apache-2.0 license applies to the extraction tool only,
  not to the glyphs themselves.
- The font is embedded solely to render an in-game HUD in the game's original visual style,
  on a non-commercial basis.

---

## 5. Audio Assets

The embedded audio files under [`AliceInCradleHack/resources/audio`](AliceInCradleHack/resources/audio)
(`Connected_Sky.wav`, `Theme_338.wav`) are tracks from the game **Blue Archive**.

- Copyright NEXON Games Co., Ltd. & Yostar, Inc. All Rights Reserved.
- They are embedded solely for an in-client sound feature on a non-commercial basis, and
  remain the property of their respective owners.

---

If you are a rights holder of any material listed above and would like the material
removed or its attribution amended, please open an issue in this repository.
