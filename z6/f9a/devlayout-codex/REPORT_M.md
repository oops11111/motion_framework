# Gate M current result: STOP before ordinary activation

Two separately primary-released attempts registered exact FilesDev4.3.0.0x64 successfully and cleaned up exactly FilesDev_4.3.0.0_x64__ykqwq8d6ps0ag. Both have cleanup_complete=true/POSTabsent, package inventory final hash equal PRE, empty foreign package deltas, unchanged normalized UserChoice, clean pinned source, and full1328-source/layout hash verification PRE/POST. Residual FilesDev data was separately read-only checked absent after each attempt.

M1 attempt20261121T120120Z-abc22b4e stopped before ActivateApplication because WinPS5 default ANSI Get-Content misdecoded UTF8-no-BOM authority JSON. Parent reviewed the two-line explicit -Encoding UTF8 fix and released continuation bound to prior cleanup evidence; M1 raw/instruction remains unchanged.

M2 attempt20261121T120437Z-eca9f19a correctly parsed authority JSON but stopped before ActivateApplication because Get-FileHash was not recognized in the helper process. No owned app PID, visible window, MainPage UIA or normal close was reached in either attempt. No workaround/fallback registration route was attempted. M2 original ignored raw and sanitized copies are retained.

A read-only candidate diagnostic computed SHA256 via existing .NET ComputeHash(ReadAllBytes), matching the bound authority hash exactly; this is not yet a helper change or new runtime attempt. Parent decides the bounded next step; no further registration is underway.

Route remains NON-ORIGINAL build + manually assembled development layout, not original/VS success. L workspace byte-copy remains PASS. F9a startup/MainPage remains NOT TESTED. No cert/runtime/component/account/setting/task changes, force kill or unrelated package removal. Old historical and VS STOP reports unchanged.
