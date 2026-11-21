# F2 authorized NU1903 continuation

NON-ORIGINAL route, authorized only for this F2 ticket. The user verbatim and recording/source context are in AUTHORIZATION_NU1903.json.

The ten isolated source hashes and v2 patch were verified before execution and remain unchanged after execution. Only missing child PROCESSOR_ARCHITECTURE was set to AMD64; machine SDK remains 10.0.300, with PATH/DOTNET_ROOT unchanged.

`dotnet restore FluentHub.slnx -p:WarningsNotAsErrors=NU1903` exited 0 (PID 85584).

`dotnet build FluentHub.slnx -c Debug -p:WarningsNotAsErrors=NU1903` exited 1 (PID 94100). The solution selected arm64 / win-arm64, net10.0-windows10.0.26100.0. CSC CS8012 rejects the referenced frozen FluidMotion.L1 assembly targeting another processor; XamlCompiler then exits 1 (MSB3073). No Platform override or fallback was executed. The native x64 pack was not rebuilt or changed.

The pack guard reported all three expected hashes. No successful app DLL or package manifest was produced, so output API/runtimeconfig verification and runtime acceptance are NOT EXECUTED. No package registration or application launch occurred.

Original stdout/stderr bytes remain in the ignored attempt directory; bounded sanitized copies and original byte hashes are in raw/20261122T073125AWST-wnae/. This continuation does not change the earlier original restore failure or any historical result.

Status: restore PASS; host build STOP architecture mismatch; stage 2 and runtime not accepted. The remaining candidate is an explicitly authorized x64 solution route, with no change to source, dependencies or frozen pack. It has not been executed.
