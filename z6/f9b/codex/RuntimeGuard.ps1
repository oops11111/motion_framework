function Get-F9bSha([string]$Path) {
    $sha=[System.Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash([System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path).Path))).Replace('-','').ToLowerInvariant() }
    finally { $sha.Dispose() }
}
function Assert-F9bAuthority([string]$AuthorityFile,[string]$InstructionFile,[string]$BindingFile,[bool]$ValidateOnly) {
    $context=Get-Content -LiteralPath $AuthorityFile -Raw -Encoding UTF8|ConvertFrom-Json
    $instruction=Get-Content -LiteralPath $InstructionFile -Raw -Encoding UTF8|ConvertFrom-Json
    $binding=Get-Content -LiteralPath $BindingFile -Raw -Encoding UTF8|ConvertFrom-Json
    $repo=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    $original=Get-Content -LiteralPath (Join-Path $repo 'z6/f9b/codex/AUTHORIZATION.json') -Raw -Encoding UTF8|ConvertFrom-Json
    if($context.grant_kind -cne 'F9b FilesDev pilot runtime' -or $context.source -cne 'Direct user response in current root conversation' -or $context.user_verbatim -cne $original.user_verbatim -or $context.original_authorization_sha256 -cne (Get-F9bSha (Join-Path $repo 'z6/f9b/codex/AUTHORIZATION.json'))) { throw 'F9b direct opening context mismatch' }
    $kind=if($ValidateOnly){@('Primary F9b runtime preparation only','Primary F9b runtime execute')}else{@('Primary F9b runtime execute')}
    if($kind -cnotcontains $instruction.instruction_kind -or [string]::IsNullOrWhiteSpace($instruction.root_verbatim)) { throw 'Primary runtime execution has not been released' }
    if($instruction.authority_sha256 -cne (Get-F9bSha $AuthorityFile) -or $instruction.binding_sha256 -cne (Get-F9bSha $BindingFile)) { throw 'Instruction not bound to authority and build/layout binding' }
    foreach($key in @('inventory_sha256','destination_hashes_sha256','manifest_sha256','host_patch_sha256')) { if($instruction.$key -cne $binding.$key){throw ('Instruction binding mismatch: '+$key)} }
    foreach($property in $binding.code_hashes.PSObject.Properties) { if((Get-F9bSha (Join-Path $repo $property.Name)) -cne $property.Value){throw ('Runtime code drift: '+$property.Name)} }
    return $binding
}
