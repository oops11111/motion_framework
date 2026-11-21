"""Recorded F9b runtime acceptance integrity; no app/UIA/native/ETW execution."""
from pathlib import Path
import hashlib,json,subprocess
import verify_phase2,verify_inputs
H=Path(__file__).resolve().parent

def load(p):return json.loads(p.read_text(encoding="utf-8-sig"))
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 verify_phase2.main()
 first=H/"raw/f9b-runtime-20261121T134355Z-00e0d1ee";last=H/"raw/f9b-runtime-20261121T134947Z-1a56d73b"
 a=load(first/"result.json");b=load(last/"result.json")
 assert a["status"]=="STOP F9b runtime incomplete" and a["error"]=="Entry movement not observed on-window"
 assert b["status"]=="PASS F9b local motion evidence pending primary image review" and not b.get("error")
 for folder,r in [(first,a),(last,b)]:
  assert r["registration_exit"]==0 and r["cleanup_complete"] and r["POST"]==[] and r["state_changed"] is False
  assert r["all_packages_PRE_sha256"]==r["all_packages_POST_sha256"] and not any(r["package_delta_final"].values()) and not any(r["package_delta_after_registration"].values())
  assert r["source_PRE"]==r["source_POST"] and r["layout_PRE"]==r["layout_POST"] and r["layout_POST"]["files"]==1334
  assert r["pilot_log_captured_before_remove"] and digest(folder/"pilot.original.jsonl")==r["pilot_log_original_sha256"]
  assert r["removed_exact_fullname"]=="FilesDev_4.3.0.0_x64__ykqwq8d6ps0ag" and r["pointer_restore"]["restored_original_pointer"]
  assert r["motion_summary"]["lastIdleActive"]==r["motion_summary"]["lastIdlePump"]==0
 assert b["normal_close_exit"]==0 and b["lifecycle_acceptance"] and b["entry_position_changed"] is False and b["entry_visible_review_required"]
 counts=b["motion_summary"]["eventCounts"];states=b["motion_summary"]["lifecycleCounts"]
 assert {"AnimationLifecycle","FrameTick","IdleTransition","Retarget","TokenLoad"}.issubset(counts) and counts["FrameTick"]>0
 assert states["Created"]==states["Released"]>0 and states["Played"]>0 and states["Cancelled"]>0
 assert "Retarget" not in a["motion_summary"]["eventCounts"]
 text=(last/"pilot.original.jsonl").read_text(encoding="utf-8");assert "tokens loaded; entry consumes demo.enter" in text and "rendering=False originalVisualRestored=true" in text and "STOP" not in text
 metadata=load(last/"module_metadata.json");assert metadata["Complete"] and metadata["TerminalError"]==0
 expected={"FluidMotion.L1.dll":"94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2","fluidmotion.dll":"add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d"}
 assert set(b["loaded_modules"])==set(expected)
 for n,h in expected.items():assert b["loaded_modules"][n]["sha256"]==h and b["loaded_modules"][n]["mapped_path"].replace("\\","/").endswith("/layout/"+n)
 assert b["token_input_sha256"]=="8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e" and counts["TokenLoad"]>0
 review=load(H/"PRIMARY_RUNTIME_ACCEPTANCE.json");assert review["actual_result_sha256"]==digest(last/"result.json") and review["entry_position_changed"] is False and review["negative_entry_displacement_accepted"] is False
 assert not review["further_runtime_required"] and review["pilot_original_sha256"]==digest(last/"pilot.original.jsonl")
 for n,h in review["selected_images"].items():assert digest(last/n)==h and (last/n).read_bytes().startswith(b"\x89PNG\r\n\x1a\n")
 geometry=load(last/"motion_geometry.json");base=geometry["base"];hover=geometry["hover"];rest=geometry["rest"];assert base["Dpi"]==120 and hover["Left"]-base["Left"]==10 and rest["Left"]==base["Left"]
 crop=load(last/"crop.json");assert crop["left"]<crop["right"] and crop["top"]<crop["bottom"]
 assert crop["settings"]["Window"]["Left"]<=crop["left"] and crop["right"]<=crop["settings"]["Window"]["Right"]
 binding=load(H/"RUNTIME_BINDING_v2.json");instruction=load(H/"PRIMARY_RUNTIME_EXECUTE_v2.json")
 assert instruction["binding_sha256"]==digest(H/"RUNTIME_BINDING_v2.json") and instruction["authority_sha256"]==digest(H/"RUNTIME_AUTHORITY_CONTEXT.json")
 for name,h in binding["code_hashes"].items():assert digest(verify_inputs.R/name)==h
 assert digest(H/"files_f9b_pilot_v4.patch")==binding["host_patch_sha256"]
 assert load(H/"raw/runtime_preparation_v2/result.json")["binding_sha256"]==digest(H/"RUNTIME_BINDING_v2.json")
 layout=H/"raw/f9b-runtime-layout-20261121T132345Z-9a66a571";assert digest(layout/"inventory.json")==binding["inventory_sha256"] and digest(layout/"destination_hashes.json")==binding["destination_hashes_sha256"]
 inventory=load(layout/"inventory.json");destination=load(layout/"destination_hashes.json");assert len(inventory)==len(destination)==1334
 for x,y in zip(inventory,destination):assert all(x[k]==y[k] for k in ["destination","sha256","bytes"])
 assert sum(x["kind"]=="server-output" for x in inventory)==22
 print(json.dumps({"status":"PASS F9b recorded local acceptance and primary image review","visible_scope":"opacity entry +8DIP hover/rest; entry position false","events":len(counts),"Created":states["Created"],"Released":states["Released"],"cleanup":"exact package removed, original log captured first","CI_scope":"evidence and actual private behavior; no cloud GUI/runtime reproduction"},indent=2))
if __name__=="__main__":main()
