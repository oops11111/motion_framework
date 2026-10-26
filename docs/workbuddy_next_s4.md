# workbuddy 派题 #16 — S4 DelayTime + presetValue

日期：2026-10-20 Asia/Taipei  
依据：deepseek_m1b_spec §5.2#3

## 目标
亮屏验证：`DelayTime` + 默认 `SetInitialValueAfterDelay` 下，延时期间属性保持 presetValue、延时结束无跳变；`SetInitialValueBeforeDelay` 仅记录行为差异。

## 步骤
1. Spring 或 KeyFrame 均可；优先 SpringScalar：Period=0.32 ζ=0.78，FinalValue=240。
2. 先 Insert/设 Translation.X = preset（如 50），再 StartAnimation，DelayTime=300ms，InitialValue=50（=preset）。
3. WGC：0–300ms 应停在 ~50；之后平滑离开，无跃变到别处再开始。
4. 对照组：InitialValue≠preset（如 IV=0 preset=50）——记录延时结束是否跳变。
5. 若 API 可设 `DelayBehavior=SetInitialValueBeforeDelay`：对比延时期间是否已写 InitialValue。

## 交付
`artifacts/win_s4_delay_probe.txt` + 一行结论（通过/部分/否）。ζ>1 门控与 S3 结论勿改。
