param([switch]$AllowPending)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
function Sha([string]$path) {
 $stream=[IO.File]::OpenRead($path);$algorithm=[Security.Cryptography.SHA256]::Create()
 try{return [BitConverter]::ToString($algorithm.ComputeHash($stream)).Replace('-','').ToLowerInvariant()}
 finally{$stream.Dispose();$algorithm.Dispose()}
}
function Assert-NoDeliveryReparse([IO.FileSystemInfo]$item,[string]$deliveryRoot) {
 for($current=$item;$current -and $current.FullName.StartsWith($deliveryRoot,[StringComparison]::OrdinalIgnoreCase);) {
  if($current.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Delivery reparse point refused'}
  if($current -is [IO.FileInfo]){$current=$current.Directory}else{$current=$current.Parent}
 }
}
$expected=@{}
foreach($line in Get-Content -LiteralPath "$root/SHA256SUMS" -Encoding UTF8) {
 if($line -cnotmatch '^([0-9a-f]{64})  (.+)$'){throw 'Invalid checksum row'}
 $digest=$Matches[1];$relative=$Matches[2]
 if($expected.ContainsKey($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..' -or [IO.Path]::IsPathRooted($relative)){throw 'Unsafe or duplicate checksum path'}
 $full=[IO.Path]::GetFullPath((Join-Path $root $relative))
 if(!$full.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Checksum path escaped delivery'}
 $item=Get-Item -LiteralPath $full
 Assert-NoDeliveryReparse $item $root
 if((Sha $full) -cne $digest){throw "SHA mismatch: $relative"}
 $expected[$relative]=$digest
}
$actual=@(Get-ChildItem -LiteralPath $root -File -Recurse|Where-Object FullName -ne "$root\SHA256SUMS"|ForEach-Object {$_.FullName.Substring($root.Length+1).Replace('\','/')})
if($actual.Count -ne $expected.Count -or @($actual|Where-Object {!$expected.ContainsKey($_)}).Count){throw 'Unlisted or missing delivery file'}
$manifest=Get-Content -LiteralPath "$root/manifest.json" -Raw -Encoding UTF8|ConvertFrom-Json
if((Sha (Join-Path $root $manifest.installer.path)) -cne $manifest.installer.sha256 -or (Sha (Join-Path $root $manifest.package.path)) -cne $manifest.package.sha256){throw 'Product byte binding mismatch'}
if($manifest.application_commit -cne '191f4d65b15e6fecf0118e0b120c7760ef3d8d53' -or $manifest.component_source_commit -cne '44401d3f002b427d828500331a3e9b4ada5881cd' -or $manifest.run -ne 45011904252){throw 'Fixed source binding mismatch'}
Write-Output "PASS content: $($expected.Count) files; no installer/application execution"
$acceptance=Join-Path $root 'ROOT_ACCEPTANCE.json'
if(!(Test-Path -LiteralPath $acceptance)) {
 Write-Output 'FINAL ACCEPTANCE PENDING'
 if(!$AllowPending){exit 2}
 return
}
$final=Get-Content -LiteralPath $acceptance -Raw -Encoding UTF8|ConvertFrom-Json
if($final.status -cne 'accepted internal delivery' -or $final.application_commit -cne $manifest.application_commit -or $final.component_source_commit -cne $manifest.component_source_commit -or $final.application_post_run -ne $manifest.run -or $final.manifest_sha256 -cne (Sha "$root/manifest.json") -or $final.installer_sha256 -cne $manifest.installer.sha256 -or $final.package_sha256 -cne $manifest.package.sha256 -or $final.mf_docs_merge_sha -cnotmatch '^[0-9a-f]{40}$' -or $final.mf_docs_post_run -le 0 -or $final.mf_docs_post_conclusion -cne 'success' -or $final.primary_review -cne 'accepted'){throw 'Final external acceptance/source/CI binding incomplete'}
Write-Output 'PASS final internal delivery acceptance; unsigned products unchanged'
