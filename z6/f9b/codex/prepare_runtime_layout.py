"""Default dryrun inventories byte sources; execute-layout only copies workspace bytes."""
from pathlib import Path,PureWindowsPath
import argparse,datetime,hashlib,json,os,re,shutil,stat,subprocess,uuid,xml.etree.ElementTree as ET
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
SOURCE=ROOT/"m0/out/f9b-inputs-20261121T125037Z-fab35294/source"
REFERENCE=SOURCE.parent/"reference"
PACK={"FluidMotion.L1.dll":"94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2","fluidmotion.dll":"add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d","demo_motion.json":"8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e"}
NUGET=Path.home()/".nuget/packages"
BIN=SOURCE/"src/Files.App/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64"
SERVER=SOURCE/"src/Files.App.Server/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64"
PIN="cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
BOUND={"Files.exe":"e5e8765c4fb1a69429c919b508e99b5e49773eb173b91cee58a2a8e3bd3d30b3","Files.dll":"ef37c9e2ad646e9279c0d5f2c51119162cea33675b005677cc936a4e3b66cbf2","AppxManifest.xml":"ad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4","Files.App.build.appxrecipe":"07e4e95c51a294055ce1a59f963a538b381e14dcb0d3f4ccdb02a723112dd60f"}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p,v):p.write_text(json.dumps(v,indent=2,ensure_ascii=False)+"\n",encoding="utf-8",newline="\n")
def state():
 def g(*a):return subprocess.check_output(["git","--no-optional-locks","-C",str(SOURCE),*a]).decode().strip()
 return {"head":g("rev-parse","HEAD"),"status":subprocess.check_output(["git","--no-optional-locks","-C",str(SOURCE),"status","--porcelain"]).decode().rstrip("\r\n"),"diff":g("diff","HEAD","--")}
def safe_ancestors(p):
 for x in [p,*p.parents]:
  if x.exists():assert not x.lstat().st_file_attributes & stat.FILE_ATTRIBUTE_REPARSE_POINT,"Reparse point: "+str(x)
def dest_parts(value):
 assert value and not any(x in value for x in [":","$","%"]),"ADS/variable/empty destination"
 w=PureWindowsPath(value);assert not w.is_absolute() and not w.drive and not w.root,"Absolute destination"
 parts=value.replace("\\","/").split("/")
 assert all(x and x not in [".",".."] and x==x.rstrip(" .") and not re.search(r'[<>"|?*\x00-\x1f]',x) and not re.fullmatch(r'(?i)(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..*)?',x) for x in parts),"Unsafe destination: "+value
 return parts
def source_info(p):
 safe_ancestors(p);assert p.is_file(),"Missing source"
 root=SOURCE if p.is_relative_to(SOURCE) else NUGET if p.is_relative_to(NUGET) else REFERENCE if p.parent==REFERENCE and p.name in PACK and sha(p)==PACK[p.name] else None
 assert root is not None,"Unapproved source root"
 return {"source":("%BUILDROOT%/" if root==SOURCE else "%NUGET%/" if root==NUGET else "%VERIFIED_REFERENCE%/")+p.relative_to(root).as_posix(),"sha256":sha(p),"bytes":p.stat().st_size}
def main():
 ap=argparse.ArgumentParser();ap.add_argument("--execute-layout",action="store_true");ap.add_argument("--inventory");args=ap.parse_args()
 a=ROOT/"m0/out"/("f9b-runtime-layout-"+datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8]);a.mkdir()
 result={"mode":"execute-layout" if args.execute_layout else "dryrun inventory only","status":"STOP","registration":False,"activation":False,"route":"F9b NON-ORIGINAL v4 build + manually assembled development layout"}
 try:
  pre=state();assert pre["head"]==PIN
  expected=json.loads((HERE/"host_source_v4.json").read_text())
  actual={line[3:] for line in pre["status"].splitlines()}
  # state status retains leading status whitespace in this collector below.
  assert actual=={r["path"] for r in expected},"Source changed beyond allowed v4 patch"
  for r in expected:assert sha(SOURCE/r["path"])==r["post_sha256"],"v4 host source drift"
  for name,h in PACK.items():assert sha(REFERENCE/name)==sha(BIN/name)==h,"Verified reference/output pack drift"
  for n,h in BOUND.items():assert sha(BIN/n)==h,"Bound input drift: "+n
  rows=[];sources=[];keys=set()
  def add(src,dest,kind):
   safe_ancestors(src);src=src.resolve(strict=True)
   assert not any(":" in part for part in src.parts[1:]),"Source ADS"
   parts=dest_parts(dest);key="/".join(parts).casefold();assert key not in keys,"Case collision: "+dest
   assert not any(key.startswith(k+"/") or k.startswith(key+"/") for k in keys),"File-directory prefix collision"
   keys.add(key);rows.append({"destination":"/".join(parts),"kind":kind,**source_info(src)});sources.append(src)
  recipe=ET.parse(BIN/"Files.App.build.appxrecipe").getroot()
  entries=[e for e in recipe.iter() if e.tag.split("}")[-1]=="AppxPackagedFile"]
  for e in entries:
   include=e.attrib["Include"];assert "$" not in include and "%" not in include,"Unresolved source variable"
   source=Path(include);assert source.is_absolute(),"Relative source requires explicit resolution"
   add(source,next(c.text for c in e if c.tag.split("}")[-1]=="PackagePath"),"recipe")
  add(BIN/"AppxManifest.xml","AppxManifest.xml","manifest")
  for member in SERVER.rglob("*"):safe_ancestors(member)
  serverfiles=sorted(f for f in SERVER.rglob("*") if f.is_file());assert len(serverfiles)==22
  for f in serverfiles:add(f,"Files.App.Server/"+f.relative_to(SERVER).as_posix(),"server-output")
  manifest=ET.parse(BIN/"AppxManifest.xml").getroot();identity=next(x.attrib for x in manifest if x.tag.split("}")[-1]=="Identity")
  assert identity=={"Name":"FilesDev","Publisher":"CN=Files","Version":"4.3.0.0","ProcessorArchitecture":"x64"},identity
  targets=[]
  for e in manifest.iter():
   for k,v in e.attrib.items():
    if k.split("}")[-1] in ["Executable","Logo","Square44x44Logo","Square150x150Logo","Wide310x150Logo","Square310x310Logo","SmallLogo","Image"]:targets.append((v,"resource" if k.split("}")[-1]!="Executable" else "executable"))
   if e.tag.split("}")[-1]=="Path" and e.text:targets.append((e.text,"server-or-winrt"))
  target_receipts=[]
  for v,kind in targets:
   parts=dest_parts(v);key="/".join(parts).casefold();exact=key in keys
   family=[]
   if not exact and kind=="resource":
    w=PureWindowsPath(v);parent="/".join(parts[:-1]).casefold();stem=w.stem.casefold();suffix=w.suffix.casefold()
    family=[r["destination"] for r in rows if r["destination"].casefold().startswith(parent+"/") and PureWindowsPath(r["destination"]).name.casefold().startswith(stem+".") and PureWindowsPath(r["destination"]).suffix.casefold()==suffix]
   assert exact or (kind=="resource" and family and "resources.pri" in keys),"Unresolved manifest target: "+v
   target_receipts.append({"declared":v,"kind":kind,"exact_file":exact,"qualified_family":family,"resource_resolution":"qualified family and PRI presence only; runtime not verified" if family else "exact mapped file"})
  save(a/"inventory.json",rows);save(a/"manifest_targets.json",target_receipts)
  result.update(source_PRE=pre,identity=identity,recipe_entries=len(entries),server_files=len(serverfiles),mapping_files=len(rows),inventory_sha256=sha(a/"inventory.json"),bound=BOUND)
  if args.execute_layout:
   assert args.inventory,"Previously dryrun inventory required"
   assert rows==json.loads(Path(args.inventory).read_text(encoding="utf-8")),"Source/mapping drift since dryrun"
   layout=a/"layout";assert not layout.exists();layout.mkdir();safe_ancestors(layout)
   copied=[]
   for row,src in zip(rows,sources):
    assert sha(src)==row["sha256"],"Source changed before copy";safe_ancestors(src)
    dst=layout.joinpath(*dest_parts(row["destination"]));assert dst.resolve().is_relative_to(layout.resolve())
    dst.parent.mkdir(parents=True,exist_ok=True);safe_ancestors(dst.parent);assert not dst.exists()
    with src.open("rb") as fi,dst.open("xb") as fo:shutil.copyfileobj(fi,fo)
    assert sha(dst)==row["sha256"];safe_ancestors(dst)
    copied.append({"destination":row["destination"],"sha256":sha(dst),"bytes":dst.stat().st_size})
   for row,src in zip(rows,sources):assert sha(src)==row["sha256"],"Source changed after copy"
   save(a/"destination_hashes.json",copied);result["layout_relative"]=layout.relative_to(ROOT).as_posix();result["destination_hashes_sha256"]=sha(a/"destination_hashes.json")
  post=state();assert pre==post;result["source_POST"]=post;result["status"]="PASS F9b runtime layout byte-copy only" if args.execute_layout else "PASS dryrun mapping only"
 except Exception as exc:result["error"]=str(exc)
 finally:save(a/"result.json",result);print(json.dumps({"attempt":a.relative_to(ROOT).as_posix(),**result},indent=2))
 return 0 if result["status"].startswith("PASS") else 1
if __name__=="__main__":raise SystemExit(main())
