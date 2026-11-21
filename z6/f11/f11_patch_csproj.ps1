# Z6 F11: rewrite the side-fork FmL1PackDir hard-coded path into overridable properties and add a build-time sha256 check.
param([Parameter(Mandatory=$true)][string]$Csproj)
$raw = [IO.File]::ReadAllBytes($Csproj)
$bom = ($raw.Length -ge 3 -and $raw[0] -eq 0xEF -and $raw[1] -eq 0xBB -and $raw[2] -eq 0xBF)
$enc = New-Object Text.UTF8Encoding($bom)
$t = [IO.File]::ReadAllText($Csproj)
$m = [regex]::Match($t, '(?m)^([ \t]*)<FmL1PackDir>[^<]*</FmL1PackDir>\r?\n')
if (-not $m.Success) { throw 'FmL1PackDir line not found' }
$i = $m.Groups[1].Value
$nl = "`r`n"
if ($t -notmatch "`r`n") { $nl = "`n" }
$new = @(
 "$i<!-- Z6-F11 PILOT-ONLY: pack location is configurable (override with -p:FmL1Root=... or -p:FmL1PackDir=... / -p:FmL1Sums=...). Default = the Z6 F7 worktree pack. -->",
 "$i<FmL1Root Condition=`"'`$(FmL1Root)' == ''`">`$(USERPROFILE)\mf_z6\f7\</FmL1Root>",
 "$i<FmL1PackDir Condition=`"'`$(FmL1PackDir)' == ''`">`$(FmL1Root)pilot\l1pack\bin\x64\Release\net8.0\</FmL1PackDir>",
 "$i<FmL1Sums Condition=`"'`$(FmL1Sums)' == ''`">`$(FmL1Root)pilot\l1pack\SHA256SUMS</FmL1Sums>"
) -join $nl
$t = $t.Remove($m.Index, $m.Length).Insert($m.Index, $new + $nl)
$target = @'
	<!-- Z6-F11 PILOT-ONLY: build-time sha256 check of the consumed pack files against the F7 SHA256SUMS; any mismatch fails the build. -->
	<Target Name="FmL1VerifyPack" BeforeTargets="BeforeBuild;ResolveAssemblyReferences">
		<Error Condition="!Exists('$(FmL1Sums)')" Text="FmL1VerifyPack: SHA256SUMS not found: $(FmL1Sums)" />
		<PropertyGroup>
			<_FmSumsText>$([System.IO.File]::ReadAllText('$(FmL1Sums)'))</_FmSumsText>
			<_FmExpL1>$([System.Text.RegularExpressions.Regex]::Match('$(_FmSumsText)', '(?m)^([0-9a-f]{64})  FluidMotion\.L1\.dll\s').Groups[1].Value)</_FmExpL1>
			<_FmExpNative>$([System.Text.RegularExpressions.Regex]::Match('$(_FmSumsText)', '(?m)^([0-9a-f]{64})  fluidmotion\.dll\s').Groups[1].Value)</_FmExpNative>
			<_FmExpJson>$([System.Text.RegularExpressions.Regex]::Match('$(_FmSumsText)', '(?m)^([0-9a-f]{64})  demo_motion\.json\s').Groups[1].Value)</_FmExpJson>
		</PropertyGroup>
		<Error Condition="'$(_FmExpL1)' == '' or '$(_FmExpNative)' == '' or '$(_FmExpJson)' == ''" Text="FmL1VerifyPack: expected sha256 entries missing in $(FmL1Sums)" />
		<Error Condition="!Exists('$(FmL1PackDir)FluidMotion.L1.dll') or !Exists('$(FmL1PackDir)fluidmotion.dll') or !Exists('$(FmL1PackDir)demo_motion.json')" Text="FmL1VerifyPack: pack file missing under $(FmL1PackDir)" />
		<GetFileHash Files="$(FmL1PackDir)FluidMotion.L1.dll" Algorithm="SHA256"><Output TaskParameter="Hash" PropertyName="_FmGotL1" /></GetFileHash>
		<GetFileHash Files="$(FmL1PackDir)fluidmotion.dll" Algorithm="SHA256"><Output TaskParameter="Hash" PropertyName="_FmGotNative" /></GetFileHash>
		<GetFileHash Files="$(FmL1PackDir)demo_motion.json" Algorithm="SHA256"><Output TaskParameter="Hash" PropertyName="_FmGotJson" /></GetFileHash>
		<Error Condition="'$(_FmGotL1)' != '$(_FmExpL1)'" Text="FmL1VerifyPack: sha256 mismatch FluidMotion.L1.dll expected=$(_FmExpL1) actual=$(_FmGotL1)" />
		<Error Condition="'$(_FmGotNative)' != '$(_FmExpNative)'" Text="FmL1VerifyPack: sha256 mismatch fluidmotion.dll expected=$(_FmExpNative) actual=$(_FmGotNative)" />
		<Error Condition="'$(_FmGotJson)' != '$(_FmExpJson)'" Text="FmL1VerifyPack: sha256 mismatch demo_motion.json expected=$(_FmExpJson) actual=$(_FmGotJson)" />
		<Message Importance="high" Text="FmL1VerifyPack: OK FluidMotion.L1.dll=$(_FmGotL1) fluidmotion.dll=$(_FmGotNative) demo_motion.json=$(_FmGotJson)" />
	</Target>

'@
$target = $target.Replace("`r`n", "`n").Replace("`n", $nl)
$k = $t.LastIndexOf('</Project>')
$t = $t.Insert($k, $target)
[IO.File]::WriteAllText($Csproj, $t, $enc)
'patched'
