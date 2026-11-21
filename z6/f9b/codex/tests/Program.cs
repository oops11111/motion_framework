using Files.App.Views;
using System.Text.Json;

int checks = 0;
void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; Console.WriteLine("ok " + label); }
var log = new List<string>();
var evidence = new FilesMotionEvidence(log.Add);
int payloadReads = 0;
IReadOnlyList<object?>? Forbidden() { payloadReads++; throw new Exception("Restricted payload getter was read"); }
foreach (var (id, name) in new[] { (3,"FrameTick"),(103,"FrameTickAccept"),(4,"PresentStats"),(99,"FrameTick"),(99,"PresentStats") }) evidence.Observe(id,name,Forbidden);
Check(payloadReads == 0, "restricted IDs and aliases never evaluate lazy payload getter");
evidence.Observe(2,"Retarget",Forbidden); evidence.Observe(6,"TokenLoad",Forbidden);
Check(payloadReads == 0, "Retarget and TokenLoad name-only paths never evaluate payload getter");
foreach (string state in new[] {"Created","Played","Cancelled","Released"}) evidence.Observe(1,"AnimationLifecycle",() => new object?[] {0,1,1,"demo.enter",state});
evidence.Observe(5,"IdleTransition",() => new object?[] {0,0,0});
Check(!JsonSerializer.IsReflectionEnabledByDefault,"fixture matches real host JSON reflection-disabled feature");
bool oldSerializeThrows=false;
try { _=JsonSerializer.Serialize(evidence.Snapshot()); } catch (InvalidOperationException) { oldSerializeThrows=true; }
Check(oldSerializeThrows,"old reflection-based disposal summary really fails under host feature");
using var document=JsonDocument.Parse(evidence.SummaryJson());
var root=document.RootElement;
Check(root.GetProperty("eventCounts").GetProperty("FrameTick").GetInt32()==3 && root.GetProperty("eventCounts").GetProperty("PresentStats").GetInt32()==2,"explicit summary preserves counter shape without reflection");
Check(root.GetProperty("lastIdleActive").GetInt32()==0 && root.GetProperty("lastIdlePump").GetInt32()==0 && root.GetProperty("CreatedEqualsReleased").GetBoolean(),"explicit summary preserves final idle and balance");
using var empty=JsonDocument.Parse(new FilesMotionEvidence(_=>{}).SummaryJson());
Check(empty.RootElement.GetProperty("lastIdleActive").ValueKind==JsonValueKind.Null && empty.RootElement.GetProperty("lastIdlePump").ValueKind==JsonValueKind.Null,"explicit summary retains missing idle as null");
var snapshot=evidence.Snapshot();var counts=(Dictionary<string,int>)snapshot["eventCounts"]!;
Check(counts["FrameTick"]==3 && counts["PresentStats"]==2,"restricted events counted with103 alias");
Check(counts.Keys.Count>=5 && (bool)snapshot["CreatedEqualsReleased"]!,"necessary lifecycle event balance retained");
Check((int)snapshot["lastIdleActive"]! == 0 && (int)snapshot["lastIdlePump"]! == 0,"last idle fields retained without timing data");
Check(log.Count==5 && log.All(x => x.StartsWith("lifecycle ") || x=="idle active=0 pump=0"),"only lifecycle and idle minimal fields emitted");
var queue=new Queue<Action>();var gate=new FilesMotionLifetime();int invoked=0;
Check(gate.Schedule(a=>{queue.Enqueue(a);return true;},()=>invoked++),"real action accepted into fake dispatcher");
queue.Dequeue()();Check(invoked==1,"queued action executes while owner live");
gate.Schedule(a=>{queue.Enqueue(a);return true;},()=>invoked++);
Check(gate.Close() && !gate.Close(),"close idempotent");queue.Dequeue()();Check(invoked==1,"queued old generation rejected after close");
Check(!gate.Schedule(_=>throw new Exception("closed gate enqueued"),()=>invoked++),"no enqueue after close");
var rejected=new FilesMotionLifetime();Check(!rejected.Schedule(_=>false,()=>invoked++),"actual queue rejection reported to caller");
Console.WriteLine($"PASS private host logic only checks={checks} GUI=NOT_EXECUTED native=NOT_EXECUTED");
