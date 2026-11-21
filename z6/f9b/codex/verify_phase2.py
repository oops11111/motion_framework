"""Phase2 source/build/API evidence, not GUI or actual ETW acceptance."""
from pathlib import Path
import hashlib,json,subprocess
import verify_inputs
H=Path(__file__).resolve().parent

def main():
 verify_inputs.main()
 a=H/"raw/20261121T131257Z-36a58930";r=json.loads((a/"result.json").read_text())
 assert r["status"]=="PASS F9b.2 build/private logic/output API only" and not r["app_executed"]
 assert r["source_PRE"]==r["source_POST"]
 allowed=["src/Files.App/Files.App.csproj","src/Files.App/Views/MainPage.xaml.cs","src/Files.App/Views/FilesMotionEvidence.cs","src/Files.App/Views/FilesMotionPilot.cs"]
 assert {line[3:] for line in r["source_POST"]["status"].splitlines()}==set(allowed)
 for command in r["commands"]:
  assert command["exit"]==0 and hashlib.sha256(verify_inputs.data(a/(command["label"]+".log"))).hexdigest()==command["published_sha256"]
 build=next(x for x in r["commands"] if x["label"]=="host_build")
 assert build["argv"][1:]==["-restore","Files.slnx","-p:Configuration=Debug","-p:Platform=x64","-v:quiet","-clp:ErrorsOnly","-p:RestorePackagesConfig=true"]
 assert "checks=17 GUI=NOT_EXECUTED native=NOT_EXECUTED" in (a/"private_logic_tests.log").read_text()
 assert all(v["reference_sha256"]==v["output_sha256"] for v in r["output_pack"].values())
 ref=H/"raw/20261121T125037Z-fab35294/surface_reference.txt";assert verify_inputs.data(a/"surface_host_output.txt")==verify_inputs.data(ref)
 assert "CHECKSURFACE fails=0" in (a/"surface_host_output.checksurface.txt").read_text()
 cfg=json.loads((a/"runtimeconfig.json").read_text())["runtimeOptions"]["configProperties"]
 assert cfg["System.Diagnostics.Tracing.EventSource.IsSupported"] is True and cfg["System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault"] is False
 source=json.loads((H/"host_source_v4.json").read_text());assert {x["path"] for x in source}==set(allowed) and all(x["LF_only_lines"]==0 for x in source)
 helper=next(x for x in source if x["path"].endswith("FilesMotionEvidence.cs"));assert helper["post_sha256"]==r["tested_actual_evidence_sha256"]
 roundtrip=json.loads((H/"patch_roundtrip_v4.json").read_text());assert roundtrip["git_apply_check_exit"]==roundtrip["git_apply_exit"]==0 and not roundtrip["normalization_exemption"]
 assert all(x["full_bytes_equal"] and x["host_sha256"]==x["roundtrip_sha256"] for x in roundtrip["rows"])
 assert roundtrip["patch_sha256"]==hashlib.sha256(verify_inputs.data(H/"files_f9b_pilot_v4.patch")).hexdigest()
 subprocess.run(["git","apply","--numstat",str(H/"files_f9b_pilot_v4.patch")],cwd=H,check=True,stdout=subprocess.DEVNULL)
 first=json.loads((H/"raw/20261121T130006Z-44000cad/result.json").read_text());assert first["status"]=="STOP" and first["commands"][-1]["exit"]==1
 assert "CS8618" in (H/"raw/20261121T130006Z-44000cad/host_build.log").read_text(encoding="utf-8")
 assert json.loads((a/"freeze_POST.json").read_text())==json.loads((H/"raw/20261121T125037Z-fab35294/freeze_PRE.json").read_text())
 print(json.dumps({"status":"PASS phase2 recorded build/logic/API","source_files":4,"private_checks":17,"phase2_runtime":"NOT EXECUTED in phase2","scope":"Evidence integrity; real host motion still requires local runtime gate"},indent=2))
if __name__=="__main__":main()
