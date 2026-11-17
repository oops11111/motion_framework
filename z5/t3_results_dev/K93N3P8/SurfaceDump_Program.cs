// Z5 dev-only tool (lives in %TEMP%, not in any repo). Dumps the public surface of an assembly
// and re-runs the same reflection checks as Z3aL1DevSmoke.CheckSurface against it.
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

string path = Path.GetFullPath(args[0]);
string outFile = args[1];
var dir = Path.GetDirectoryName(path)!;
var alc = new AssemblyLoadContext("z5surface", isCollectible: false);
alc.Resolving += (ctx, name) => { var p = Path.Combine(dir, name.Name + ".dll"); return File.Exists(p) ? ctx.LoadFromAssemblyPath(p) : null; };
var asm = alc.LoadFromAssemblyPath(path);
var sb = new StringBuilder();
const BindingFlags F = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
Type[] types;
try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray()!; }
foreach (var t in types.Where(t => t.IsVisible).OrderBy(t => t.FullName, StringComparer.Ordinal))
{
    string kind = t.IsEnum ? "enum" : t.IsValueType ? "struct" : t.IsInterface ? "interface" : (t.IsAbstract && t.IsSealed) ? "static class" : t.IsSealed ? "sealed class" : "class";
    sb.Append("TYPE ").Append(kind).Append(' ').Append(t.FullName).Append(" : ").Append(t.BaseType?.FullName ?? "-").Append('\n');
    var members = new List<string>();
    foreach (var m in t.GetMembers(F))
    {
        if (m is MethodInfo mi && mi.IsSpecialName) continue;
        string s = m switch
        {
            ConstructorInfo c => "ctor(" + string.Join(", ", c.GetParameters().Select(P)) + ")",
            MethodInfo mi2 => (mi2.IsStatic ? "static " : "") + mi2.ReturnType + " " + mi2.Name + "(" + string.Join(", ", mi2.GetParameters().Select(P)) + ")",
            PropertyInfo pi => "prop " + pi.PropertyType + " " + pi.Name + (pi.GetMethod?.IsPublic == true ? " get" : "") + (pi.SetMethod?.IsPublic == true ? " set" : ""),
            FieldInfo fi => (fi.IsLiteral ? "const " : fi.IsStatic ? "static " : "") + fi.FieldType + " " + fi.Name + (fi.IsLiteral ? " = " + fi.GetRawConstantValue() : ""),
            EventInfo ei => "event " + ei.EventHandlerType + " " + ei.Name,
            Type nt => "nested " + nt.FullName,
            _ => m.MemberType + " " + m.Name
        };
        members.Add(s);
    }
    foreach (var s in members.OrderBy(x => x, StringComparer.Ordinal)) sb.Append("  ").Append(s).Append('\n');
}
static string P(ParameterInfo p) => p.ParameterType + " " + p.Name + (p.HasDefaultValue ? " = " + (p.DefaultValue ?? "null") : "");

// CheckSurface (same checks as MotionSoftDemo/Z3aL1DevSmoke.cs CheckSurface)
var checks = new StringBuilder();
int fails = 0;
void Ok(bool c, string msg) { if (c) checks.Append("ok ").Append(msg).Append('\n'); else { fails++; checks.Append("FAIL ").Append(msg).Append('\n'); } }
Type? T(string n) => asm.GetType("MotionSoftDemo." + n);
var MS = T("MotionSystem"); var AH = T("AnimationHandle"); var MT = T("MotionTokens"); var SP = T("SpringParams");
Ok(MS != null && AH != null && MT != null && SP != null, "L1 types present");
foreach (string name in new[] { "MotionEngine", "MotionAnimation", "RiveView", "PointerFollow" })
    Ok(!types.Any(t => t.Name == name || t.FullName == "MotionSoftDemo." + name), "no public-or-otherwise type " + name);
bool HasM(Type? t, string n, Type[] a) => t?.GetMethod(n, a) != null;
Ok(HasM(MS, "Create", Type.EmptyTypes), "MotionSystem.Create()");
Ok(HasM(MS, "Dispose", Type.EmptyTypes), "MotionSystem.Dispose()");
Ok(HasM(MS, "LoadTokens", new[] { typeof(string) }), "MotionSystem.LoadTokens(json)");
Ok(HasM(MS, "SetReducedMotion", new[] { typeof(bool) }), "MotionSystem.SetReducedMotion(on)");
Ok(MS?.GetProperty("IsRunning")?.CanRead == true, "MotionSystem.IsRunning");
Ok(HasM(MS, "OnFrame", Type.EmptyTypes), "MotionSystem.OnFrame()");
Ok(HasM(MS, "Current", new[] { typeof(ulong), typeof(string) }), "MotionSystem.Current(target, property)");
Ok(HasM(MS, "CreateAnimation", new[] { typeof(ulong), typeof(string) }), "MotionSystem.CreateAnimation(target, transition?)");
Ok(HasM(AH, "Play", Type.EmptyTypes), "AnimationHandle.Play()");
Ok(HasM(AH, "Cancel", Type.EmptyTypes), "AnimationHandle.Cancel()");
Ok(HasM(AH, "Release", Type.EmptyTypes), "AnimationHandle.Release()");
var rt = AH?.GetMethod("Retarget");
Ok(rt != null, "AnimationHandle.Retarget");
if (rt != null)
{
    var ps = rt.GetParameters();
    Ok(ps.Length == 4 && ps[0].Name == "property" && ps[0].ParameterType == typeof(string)
        && ps[1].Name == "target" && ps[1].ParameterType == typeof(double)
        && ps[2].Name == "spring" && ps[2].ParameterType == SP
        && ps[3].Name == "from" && ps[3].ParameterType == typeof(double?)
        && ps[3].HasDefaultValue && ps[3].DefaultValue == null, "Retarget(property, target, spring, from?)");
}
Ok(HasM(MT, "Load", new[] { typeof(string) }), "MotionTokens.Load(json)");
bool P3(Type? t) { if (t == null) return false; foreach (var m in t.GetMethods(F)) foreach (var p in m.GetParameters()) if (p.HasDefaultValue && p.DefaultValue is double d && Math.Abs(d - 0.3) < 1e-12) return true; return false; }
Ok(!P3(MS) && !P3(AH) && !P3(MT), "no 0.3 default on L1 methods");
checks.Append("CHECKSURFACE fails=").Append(fails).Append('\n');
var enc = new UTF8Encoding(false);
File.WriteAllText(outFile, "# public surface of " + Path.GetFileName(path) + "\n" + sb.ToString(), enc);
File.WriteAllText(Path.ChangeExtension(outFile, ".checksurface.txt"), checks.ToString(), enc);
Console.Write(checks.ToString());
return fails == 0 ? 0 : 1;
