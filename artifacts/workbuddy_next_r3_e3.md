# workbuddy #49 — R-3 叠加 E3 真机对照

## 背景
- #48：P0a–d 全 PASS（屏幕侧）；E1 未确认 δ̂(φ)=c−φ；Bv 跳变≈1 帧陈旧。
- Claude 优先叠加 retarget；门控改为连续性（J≈0、Δv≈0）+ 响应延迟 ≤1 帧，不用旧 post-RMS。
- 硬约束：仅程序驱动 Composition 动画；禁止合成鼠标/键盘；不改 `code/composition.h`；S1 v2 映射 Period=r/(2π)。

## 叠加臂 Sup
1. `ps` 标量：`b`（基轨迹）、`d1`（步进项）。
2. Expression：`Translation.X = ps.b + ps.d1`。
3. t=0：在 `b` 上弹簧 0→240，IV=0，Period/ζ 同 #48。
4. 打断 t≈60 ms（相位可控）：**不停** `b`；在 `d1` 上启动弹簧 InitialValue=0、FinalValue=b_new−240、InitialVelocity=0（同 Period/ζ）。
5. 不读位置/速度，无 δ。

## 对照臂（同格交错）
- **Bv**：#47/#48 同款（IV=null，InitialVelocity=shadowVel，FinalValue=b_new）。
- **base**：不打断。
- （可选）Aex δ̂=0：InitialValue=shadowPos，InitialVelocity=shadowVel。

## 矩阵
- 打断时刻：t060（主），加 t156 各一半预算即可。
- b_new ∈ {0, 120}（t060）；可选再加 300 少量。
- φ ∈ {1,5,9,13} ms（缩桶；DwmFlush 同 #48），每桶每臂 ≥8 次。
- 弹簧 r=0.32, ζ=0.78。

## 度量（每 trial）
- 签名 J（屏幕轨迹相对未打断基线/影子在交接处的最小间隙）、Δv（交接速度差）。
- 响应延迟 ε：步进项相对「理想打断时刻」的有效启动滞后（ms）；期望 ≤1 帧（≈16.7 ms）。
- 过冲相对影子、方向反转次数。
- 产物：`artifacts/p13_49_raw.txt` + 汇总 `artifacts/p13_49.txt`。

## 成功标准（报告用，非规格裁决）
- Sup：J 中位 ≪ Bv（目标接近噪声），无明显一帧回跳；Δv 可判格通过率 ≥ Bv；ε 中位 ≤1 帧。
- 若 Sup FAIL：记下失败模式（跳变/延迟/过冲），不要擅自改成 E2。

## 实现提示
- 复用 `R3Probe.cs` / `--r3-p12-probe` 样板；新入口如 `--r3-e3-probe`。
- PropertySet.TryGetScalar 无中间值：度量以 WGC 为准。
