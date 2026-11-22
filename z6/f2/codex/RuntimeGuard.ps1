function Get-F2Sha([string]$Path) {
    $sha=[System.Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash([System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path).Path))).Replace('-','').ToLowerInvariant() }
    finally { $sha.Dispose() }
}
function Assert-F2Authority([string]$AuthorityFile,[string]$InstructionFile,[string]$BindingFile,[bool]$ValidateOnly) {
    $context=Get-Content -LiteralPath $AuthorityFile -Raw -Encoding UTF8|ConvertFrom-Json
    $instruction=Get-Content -LiteralPath $InstructionFile -Raw -Encoding UTF8|ConvertFrom-Json
    $binding=Get-Content -LiteralPath $BindingFile -Raw -Encoding UTF8|ConvertFrom-Json
    $repo=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    $kinds=if($ValidateOnly){@('Primary F2 runtime preparation only','Primary F2 runtime execute')}else{@('Primary F2 runtime execute')}
    if($kinds -cnotcontains $instruction.instruction_kind -or [string]::IsNullOrWhiteSpace($instruction.root_verbatim)){throw 'F2 runtime not released'}
    if($instruction.authority_sha256 -cne (Get-F2Sha $AuthorityFile) -or $instruction.binding_sha256 -cne (Get-F2Sha $BindingFile)){throw 'Instruction/context binding mismatch'}
    if(-not $ValidateOnly){
        if($context.grant_status -cne 'granted' -or $context.grant_kind -cne 'F2 temporary package runtime' -or $context.source -cne 'direct user response in current root conversation' -or [string]::IsNullOrWhiteSpace($context.user_verbatim)){throw 'Direct F2 package authorization is required'}
    }
    foreach($property in $binding.code_hashes.PSObject.Properties){if((Get-F2Sha (Join-Path $repo $property.Name)) -cne $property.Value){throw ('Runtime code drift: '+$property.Name)}}
    if($instruction.layout_sha256 -cne $binding.layout_sha256 -or $instruction.manifest_sha256 -cne $binding.manifest_sha256){throw 'Layout binding mismatch'}
    return $binding
}
