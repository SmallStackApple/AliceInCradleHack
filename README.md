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
    2. 将所有文件夹复制到[C:\AliceInCradleHack\\](C:\AliceInCradleHack\)文件夹下
    3. 使用任意方式注入AliceInCradleHack.dll并调用AliceInCradleHack.InjectEntry:Inject()方法进行初始化（可以使用我构建的[SharpMonoInjector](https://github.com/SmallStackApple/SharpMonoInjector/releases)）
- 使用最新构建版本
   - 访问[nightly](https://nightly.link/SmallStackApple/AliceInCradleHack/workflows/build/main/release-build.zip)获取最新构建版本

## 作者倡议
本项目及基于其开发的 **衍生作品** 均遵循 GPLv3 开源协议，核心是自由共享、协作共赢。为守护开源精神的纯粹性，作者在此发出倡议，恳请所有使用者与开发者共同遵守：

1. 不建议付费分发：本项目及衍生作品永久开源，**建议不要将其作为付费商品售卖、付费解锁内容，或通过广告、会员等形式变相商业化**，让技术无门槛惠及更多人 **（如果我哪天闭源收费我就死父母）**；
2. 不索取强制互动：使用、分享或二次开发本项目及衍生作品时，**不必将“点赞、关注、收藏、转发”作为必要条件**，尊重每一位使用者的自主选择；
3. 鼓励自由共享：欢迎在遵循 GPLv3 协议的前提下，**免费分享项目链接、衍生作品成果（需保留原版权声明及开源协议），共同推动社区良性发展**。

---

### 补充说明
- 本倡议与 GPLv3 协议核心条款不冲突，二次开发、分发等行为仍需严格遵循协议要求（如保留版权声明、衍生作品开源、不得附加额外限制等）；
- 本项目及衍生作品的版权归属原作者及贡献者，使用时请遵守相关法律法规与 GPLv3 协议。

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
    2. Copy all folders to the directory [C:\AliceInCradleHack\]
    3. Inject AliceInCradleHack.dll by any means and call the `AliceInCradleHack.InjectEntry:Inject()` method for initialization (you can use the [SharpMonoInjector](https://github.com/SmallStackApple/SharpMonoInjector/release) built by me)
- Use the Latest Built Version
   - Visit [nightly](https://nightly.link/SmallStackApple/AliceInCradleHack/workflows/build/main/release-build.zip) to get the latest built version

## Author's Initiative
This project and all **derivative works** developed based on it are licensed under the GPLv3 open source license, with the core values of free sharing and win-win collaboration. To safeguard the purity of the open source spirit, the author hereby puts forward the following initiative, and earnestly requests all users and developers to abide by it together:

1. Paid Distribution is Not Recommended: This project and its derivative works are permanently open source. It is **recommended not to sell them as paid products, unlock content for a fee, or conduct disguised commercialization through advertisements, memberships, etc.**, so that technology can benefit more people without thresholds **(If I ever close the source and charge for it, I will be cursed with the death of my parents))**;
2. No Mandatory Interaction Required: When using, sharing or secondary developing this project and its derivative works, **it is not necessary to take "liking, following, collecting, forwarding" as prerequisites**, and respect the independent choice of every user;
3. Encourage Free Sharing: You are welcome to **freely share project links and derivative work results (while retaining the original copyright notice and open source license)** in accordance with the GPLv3 license, and jointly promote the healthy development of the community.

---

### Supplementary Notes
- This initiative is not in conflict with the core clauses of the GPLv3 license. Secondary development, distribution and other behaviors must still strictly comply with the license requirements (such as retaining copyright notices, open sourcing derivative works, and not adding additional restrictions, etc.);
- The copyright of this project and its derivative works belongs to the original author and contributors. Please comply with relevant laws, regulations and the GPLv3 license when using it.
