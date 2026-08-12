# 摇篮中的爱丽丝黑壳 (AliceInCradleHack)

![Build Status](https://img.shields.io/github/actions/workflow/status/SmallStackApple/AliceInCradleHack/build.yml?branch=main&style=plastic&logo=githubactions&logoColor=white)
![License](https://img.shields.io/github/license/SmallStackApple/AliceInCradleHack?style=plastic&color=blue)

An injection hack for Alice In Cradle

[English](#English)
# 中文

## 简介
本项目是一个纯.NET4.8.1的注入式黑客，用于[AliceInCradle](https://cn.aliceincradle.dev)，提供一些基础的修改。需要在游戏打开后注入并调用AliceInCradleHack.InjectEntry:Inject()方法进行初始化。

## 构建方法
1. 环境要求
    - Visual Studio 2019 或更高版本（勾选.NET桌面开发）
    - .NET Framework 4.8.1（其实试过4.7.2也可以）
2. 构建
    - 打开AliceInCradleHack.sln
    - 点击上方工具栏中的`生成(B)`->`生成解决方案(B)`或者使用快捷键`Ctrl+Shift+B`生成
    - 构建成功后，产物在 [AliceInCradleHack\bin\Release](AliceInCradleHack\bin\Release)中

## 使用方法
- 脚本自动注入
    1. 访问[Release](https://github.com/SmallStackApple/AliceInCradleHack/releases)下载release-build.zip
    2. 解压release-build.zip
    3. 运行`inject.bat`或者`inject.ps1`
- 手动注入
    1. 运行游戏
    2. 将所有文件夹复制到`C:\AliceInCradleHack\`文件夹下
    3. 使用任意方式注入AliceInCradleHack.dll并调用AliceInCradleHack.InjectEntry:Inject()方法进行初始化（可以使用我构建的[SharpMonoInjector](https://github.com/SmallStackApple/SharpMonoInjector/releases)）
- 使用最新构建版本
   - 访问[nightly](https://nightly.link/SmallStackApple/AliceInCradleHack/workflows/build/main/release-build.zip)获取最新构建版本

## 作者倡议
本项目及基于其开发的 **衍生作品** 均遵循 GPLv3 开源协议，核心是自由共享、协作共赢。为守护开源精神的纯粹性，作者在此发出倡议，恳请所有使用者与开发者共同遵守：

1. 不建议付费分发：本项目及衍生作品永久开源，**建议不要将其作为付费商品售卖、付费解锁内容，或通过广告、会员等形式变相商业化**，让技术无门槛惠及更多人；
2. 不索取强制互动：使用、分享或二次开发本项目及衍生作品时，**不必将“点赞、关注、收藏、转发”作为必要条件**，尊重每一位使用者的自主选择；
3. 鼓励自由共享：欢迎在遵循 GPLv3 协议的前提下，**免费分享项目链接、衍生作品成果（需保留原版权声明及开源协议），共同推动社区良性发展**。

---

### 补充说明
- 本倡议与 GPLv3 协议核心条款不冲突，二次开发、分发等行为仍需严格遵循协议要求（如保留版权声明、衍生作品开源、不得附加额外限制等）；
- 本项目及衍生作品的版权归属原作者及贡献者，使用时请遵守相关法律法规与 GPLv3 协议。

## 扩展开发 (Extension)

### 目录结构
扩展以**一级子目录**的形式放置于 `<主目录>\Extensions\` 下，初始化时每个子目录会被自动扫描加载。
每个扩展独占一个文件夹，依赖放在自己的 `lib\` 子目录中，互不共享，实现依赖隔离。
`Extensions\` 根目录下的 DLL 不会被加载。

```
<主目录>\
├── Extensions\
│   ├── MyExtension\
│   │   ├── MyExtension.dll
│   │   └── lib\          ← 该扩展私有的依赖（可选）
│   └── AnotherExtension\
│       └── AnotherExtension.dll
```

扩展可通过 `CurrentFolder` 属性读取自己所在文件夹的绝对路径（在 `Initialize()` 之前由
`ExtensionManager` 赋值），用于定位自己的配置、数据等资源。

### 最小示例
扩展需继承 `AliceInCradleHack.extension.Extension`，实现 `Initialize()` 与 `Dispose()`：

```csharp
public class MyExtension : Extension
{
    public override string Name => "MyExtension";
    public override string Description => "示例扩展";

    public override void Initialize()
    {
        // 在这里可直接调用现有 Manager 或游戏 API
        ModuleManager.Instance.EnableModule("Critical");
        CommandManager.Instance.RegisterCommand(new MyCommand());
    }

    public override void Dispose()
    {
        // 必须撤销 Initialize 中注册的所有资源
        CommandManager.Instance.UnregisterCommand("mycmd");
    }
}
```

### 规则
- 扩展与主程序运行在同一个 AppDomain，可直接访问 `ModuleManager`、`CommandManager`、`PatchManager` 等单例；
- 每个扩展独占一个 `Extensions\<扩展名>\` 文件夹，依赖放在自己的 `lib\` 子目录中；
- `CurrentFolder` 在 `Initialize()` 之前被赋值，指向扩展自己的文件夹，可在 `Initialize()` 中使用；
- `Dispose()` 中必须撤销本扩展注册的命令、模块、Harmony 补丁等资源；
- 扩展 DLL 无法从内存卸载（宿主导入的普通程序集），`Dispose` 只释放托管资源，DLL 需等游戏进程退出才释放。

# English

## Introduction
This project is a pure .NET Framework 4.8.1 injectable hack tool for [AliceInCradle](https://cn.aliceincradle.dev), which provides basic modification features. It needs to be injected after the game is launched, and the initialization is completed by calling the `AliceInCradleHack.InjectEntry:Inject()` method.

## Build Method
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
    3. Inject AliceInCradleHack.dll by any means and call the `AliceInCradleHack.InjectEntry:Inject()` method for initialization (you can use the [SharpMonoInjector](https://github.com/SmallStackApple/SharpMonoInjector/release) built by me)
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
