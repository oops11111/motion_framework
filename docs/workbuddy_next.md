# workbuddy 下一步（软件路径可演示｜命令级）

日期：2026-10-19（Asia/Taipei）  
角色：workbuddy cli @ DESKTOP-K93N3P8  
约束：不改 `composition.h` 大改（只读）；证据门槛 A 已有 `artifacts/linux_gtest_log.txt`。

## 0. 路径现状
| 侧 | `/workspace/motion_framework/` |
|---|---|
| Grok box（协调 bot / 本文档所在） | **可达**（`docs/` `code/` `artifacts/` 齐全） |
| Windows DESKTOP-K93N3P8 | **不可达**（无 `C:\workspace\...`、无 WSL/共享挂载） |

Windows 取代码：由协调侧把 `code/`（+ 可选 `artifacts/linux_gtest_log.txt`）拷到本机，例如：
```bat
mkdir C:\src\motion_framework\code
rem 用 CopyFromBox / U 盘 / 聊天附件把 *.h *.cpp motion.json 放到该目录
cd /d C:\src\motion_framework\code
```

## 1. 本机工具链（已探测）
- **MSVC**：VS 2022 BuildTools 17.14；`cl` 19.44.35228（需先 `vcvars64.bat`）
  - `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat`
- **cmake**：未安装  
- **g++（Windows）**：未安装  
- **g++（Linux box）**：14.2.0（单测已在 box 复验）

## 2. code/ 盘点（头文件与测试入口）
| 文件 | 角色 |
|---|---|
| `spring.h` `bezier.h` `timeline.h` | 数学 / 时间线 |
| `tokens.h` + `motion.json` | token 加载 |
| `controller.h` | 可打断控制器 |
| `composition.h` (+ `.partial`) | 模块6半成品，**只读** |
| `timeline_tests.cpp` | 入口：自带 `main`；build 注释用 g++ |
| `tokens_tests.cpp` | 入口：自带 `main`；可选参数 `motion.json` |
| `controller_tests.cpp` | 入口：自带 `main` |

Linux 证据：`timeline 419` / `tokens 617` / `controller 236` / `composition_smoke=ok`。

## 3. 控制台 golden（Windows / MSVC）
在已 `vcvars64` 的 x64 提示符中：
```bat
cd /d C:\src\motion_framework\code

cl /std:c++17 /EHsc /O2 /W3 timeline_tests.cpp /Fe:timeline_tests.exe
timeline_tests.exe

cl /std:c++17 /EHsc /O2 /W3 tokens_tests.cpp /Fe:tokens_tests.exe
tokens_tests.exe motion.json

cl /std:c++17 /EHsc /O2 /W3 controller_tests.cpp /Fe:controller_tests.exe
controller_tests.exe
```
验收：各 exe 打印 checks、0 failures（数字应贴近 Linux 日志）。

Linux box 对照（已有 g++，可选复跑）：
```bash
cd /workspace/motion_framework/code
g++ -std=c++17 -O1 -Wall -Wextra timeline_tests.cpp -o timeline_tests && ./timeline_tests
g++ -std=c++17 -O1 -Wall -Wextra tokens_tests.cpp -o tokens_tests && ./tokens_tests motion.json
g++ -std=c++17 -O1 -Wall -Wextra controller_tests.cpp -o controller_tests && ./controller_tests
```

## 4. WinUI3 软件路径最小切片（演示算过线，命令级）
目标：应用线程 Tick + 同一套数学，**不**要求合成器 lowering（composition 大改留给 Claude）。

1. `winget install Microsoft.WindowsAppSDK`（若未装）+ 用 VS 建 **WinUI 3 C#** 空白打包应用 `MotionSoftDemo`。
2. 把 `code/*.h`（除 composition 大改外）经 C++/CLI 或先做 **C++ 静态库 + P/Invoke 薄封装** 链进工程（本切片允许只暴露 `MotionSystem::Tick` / `Create` / `Value` 三个符号）。
3. `dotnet build -c Debug` → `dotnet run`（或 VS F5）跑起窗口。
4. UI：一个 `Slider`/`Button` 触发 `Play`，`DispatcherQueueTimer`（~16ms）调 `Tick`，把采样值写到 `TranslateTransform.X`（或 `Opacity`）——纯软件路径。
5. 录 5s 屏或 ETW 不必；验收=窗口可见插值 + 打断改目标不跳变。

并行：等 Claude 补 composition 测试后再做「单属性 CompositionDispatcher」试点（Gemini Day2），不阻塞本演示。

## 5. 不做 / 阻塞
- **不做**：改 `composition.h` 大改、装完整 WinMD/Rive、强上 cmake。
- **阻塞**：Windows 未挂载 `/workspace` → 需协调方同步 `code/`；本机无 cmake/g++ → golden 走 `cl`；WinUI3 切片需本机已装 Windows App SDK（未在本次探测中验证安装状态）。
