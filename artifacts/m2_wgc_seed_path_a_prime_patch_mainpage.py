from pathlib import Path
import re
p = Path(r'C:\Users\admin\motion_framework\MotionSoftDemo\MainPage.xaml.cs')
text = p.read_text(encoding='utf-8')

# backup
bak = p.with_suffix('.xaml.cs.bak_path_a_prime')
if not bak.exists():
    bak.write_text(text, encoding='utf-8')

# 1) bool field
old_bool = '_m2WgcSeedAbsProbe, _m2WgcSeedCalibProbe, _m2WgcSeedPathBPrimeProbe;'
new_bool = '_m2WgcSeedAbsProbe, _m2WgcSeedCalibProbe, _m2WgcSeedPathBPrimeProbe, _m2WgcSeedPathAPrimeProbe;'
assert old_bool in text, 'bool field not found'
text = text.replace(old_bool, new_bool, 1)

# 2) flag detection
old_flag = '        _m2WgcSeedPathBPrimeProbe = args0.Any(a => a.Equals("--m2-wgc-seed-path-b-prime", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_PATH_B_PRIME") == "1" || System.IO.File.Exists(@"C:\\Users\\admin\\motion_framework\\artifacts\\m2_wgc_seed_path_b_prime.flag");'
new_flag = old_flag + '\n        _m2WgcSeedPathAPrimeProbe = args0.Any(a => a.Equals("--m2-wgc-seed-path-a-prime", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_PATH_A_PRIME") == "1" || System.IO.File.Exists(@"C:\\Users\\admin\\motion_framework\\artifacts\\m2_wgc_seed_path_a_prime.flag");'
assert old_flag in text, 'flag not found'
text = text.replace(old_flag, new_flag, 1)

# 3) else-if before PathB
old_elif = '''            else if (_m2WgcSeedPathBPrimeProbe)
            {
                Log("M2-WGC-SEED-PATH-B-PRIME PATHB-A=V2-clamp probe (--m2-wgc-seed-path-b-prime / MF_M2_WGC_SEED_PATH_B_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedPathBPrimeProbe();
                t.Start();
            }'''
new_elif = '''            else if (_m2WgcSeedPathAPrimeProbe)
            {
                Log("M2-WGC-SEED-PATH-A-PRIME PATHB-A′=V2-clamp+V3-VS probe (--m2-wgc-seed-path-a-prime / MF_M2_WGC_SEED_PATH_A_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedPathAPrimeProbe();
                t.Start();
            }
            else if (_m2WgcSeedPathBPrimeProbe)
            {
                Log("M2-WGC-SEED-PATH-B-PRIME PATHB-A=V2-clamp probe (--m2-wgc-seed-path-b-prime / MF_M2_WGC_SEED_PATH_B_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedPathBPrimeProbe();
                t.Start();
            }'''
assert old_elif in text, 'elif not found'
text = text.replace(old_elif, new_elif, 1)

# 4) log file name ternary — insert PathA before PathB
old_log = '_m2WgcSeedPathBPrimeProbe ? "m2_wgc_seed_path_b_prime_ui.txt" :'
new_log = '_m2WgcSeedPathAPrimeProbe ? "m2_wgc_seed_path_a_prime_ui.txt" : (_m2WgcSeedPathBPrimeProbe ? "m2_wgc_seed_path_b_prime_ui.txt" :'
assert old_log in text, 'log ternary not found'
text = text.replace(old_log, new_log, 1)
# add one closing paren before the final semicolon of that statement
# The statement ends with: "comp_pilot_log.txt"))))...)));
# Find it uniquely
pat = re.compile(r'(string name = _lifecycle \? "m8_lifecycle_log\.txt".*"comp_pilot_log\.txt")(\)+)(;)')
m = pat.search(text)
assert m, 'log end not found'
# After inserting one extra '(', we need one extra ')'
text = text[:m.start(2)] + m.group(2) + ')' + m.group(3) + text[m.end():]

# 5) StreamWriter append: add || _m2WgcSeedPathAPrimeProbe after PathBPrimeProbe in the long OR chain
old_sw = '|| _m2WgcSeedCalibProbe || _m2WgcSeedPathBPrimeProbe, new UTF8Encoding'
new_sw = '|| _m2WgcSeedCalibProbe || _m2WgcSeedPathBPrimeProbe || _m2WgcSeedPathAPrimeProbe, new UTF8Encoding'
assert old_sw in text, 'streamwriter not found'
text = text.replace(old_sw, new_sw, 1)

p.write_text(text, encoding='utf-8')
print('patched ok; size', p.stat().st_size)
# verify
for needle in ['_m2WgcSeedPathAPrimeProbe', 'RunM2WgcSeedPathAPrimeProbe', 'm2_wgc_seed_path_a_prime.flag', 'MF_M2_WGC_SEED_PATH_A_PRIME']:
    print(needle, text.count(needle))
