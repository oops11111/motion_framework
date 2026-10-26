# workbuddy 派单：R-2 重开后真机探针（#34–#38）

依据：[notion_r2_reopen_abc.md](./notion_r2_reopen_abc.md) C 节。R-2 状态 OPEN（seed 来源缺陷，#33 可见跳变 ≈207 DIP）。

## 硬性约束（每单都适用）
- **不得合成鼠标/键盘输入**（不用 SendInput / 模拟点击 / 按键注入等）；**只用程序化动画**（代码直接调用 StartAnimation / Retarget）。
- 不花钱、不改账号；**不改 `composition.h`**（仅当 #38 需要 `interruptible` hint 且该 hint 已由开发方落地时使用，本派单不负责实现）。
- 共同前提：G0 通过、屏幕常亮；单个 Border 的 `Translation.X`；WGC 捕获；单位 DIP；记录呈现时间戳并算实测 L。
- 每格附**同会话不打断基线**。报告 J_vis、Δv_vis、postRMS（**S 与 L 两列并列**）及各项增量。
- J_vis = |x_disp(首 post 帧) − [x_disp(末 pre 帧) + v_disp,pre·Δt_frame]|；门槛 J_vis ≤ 0.5 DIP 且 ≤ 5% 帧位移；Δv_vis ≤ max(20, 5%·|v|)。
- **产物一律写 `artifacts/win_*.txt`**（文件名见各单），并在 workbuddy_soft_demo.md 追加一行结论。

## #34 P1：IV=null 语义（不用影子）
- 替换目标三种：运行中的弹簧 / 运行中的线性关键帧 / 静止属性；另加一格：同一 commit 内两次 retarget。每格 ≥10 次。
- 判据：J_vis ≤ 一帧位移 + 噪声底 → null = 动画中的当前值；跳回静态起点 → B 作废。
- 产物：`artifacts/win_r2_p1_ivnull.txt`

## #35 P2：A 对 B 交错对比（仅当 #34 通过）
- 打断时刻约 60 / 156 / 300 ms（高速段、首峰附近、衰减中段），正反两方向，每格 ≥20 次，A/B 交错。
- 判据：B 的 J_vis 中位数达标且 ≤ 0.25×A；95% bootstrap 区间不重叠；|dvel|_B 不劣于 A；双 retarget 格 ≥95% 达标。
- 产物：`artifacts/win_r2_p2_ab.txt`

## #36 P3：合成器时基与起步分解（不打断）
- Period ∈ {0.2, 0.32, 0.5} × ζ ∈ {0.6, 0.78, 1.0}，每格 5 次。
- 测：StartAnimation 调用→首个位移帧的纯延迟 L0；前 200 ms 屏幕位置与影子的差值曲线。
- 目的：把约 207 的差拆成延迟与形状两部分，供 M2 形状模型输入。
- 产物：`artifacts/win_r2_p3_timebase.txt`

## #37 P4：B2 速度来源（仅 IV=null 臂，诊断）
- InitialVelocity 三臂：影子速度 / 0 / 不设；场景同 #33（Period 0.32，ζ 0.78，0→240 DIP，约 156 ms 打断→0）；每臂 ≥10 次。
- 测：B2 上冲峰值（对照约 269）、Δv_vis、postRMS。只作诊断，不改默认速度来源。
- 产物：`artifacts/win_r2_p4_b2vel.txt`

## #38 P5：(d) AppThread 基线
- 场景同 #33，强制不 lower（`interruptible`），每格 10 次。
- 测：J_vis、Δv_vis、postRMS、掉帧率、UI 线程占用；并回归验证 ζ>1 不 lower 门控与 lower 判定未被新 hint 误伤。
- 产物：`artifacts/win_r2_p5_appthread.txt`

## 回报格式（一行）
`#3x：J_vis 中位=…（S/L）；Δv_vis=…；ΔpostRMS S=… / L=…；结论=…；产物 artifacts/win_….txt`
