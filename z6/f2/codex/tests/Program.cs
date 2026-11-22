using FluentHub.Views;
int checks=0;
void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
var queue=new Queue<Action>();var trace=new List<string>();
var state=new RootRebuildState(a=>{queue.Enqueue(a);return true;},trace.Add);
int generation=state.Begin();Check(generation>0 && state.Begin()==0,"one pending");
int created=0;Check(state.OldDisposed(generation,()=>{Check(state.Begin()==0,"pending during creation");created++;}),"queued after disposal");
Check(created==0,"no inline creation");queue.Dequeue()();Check(created==1,"deferred create");
generation=state.Begin();state.OldDisposed(generation,()=>created++);state.Close();queue.Dequeue()();Check(created==1,"closed rejects stale callback");
foreach(var mode in new[]{"reject","enqueue_throw","create_throw","inline"}){
 trace.Clear();Action? callback=null;
 var s=new RootRebuildState(a=>{if(mode=="enqueue_throw")throw new InvalidOperationException();if(mode=="reject")return false;if(mode=="inline")a();else callback=a;return true;},trace.Add);
 int g=s.Begin();s.OldDisposed(g,()=>throw new InvalidOperationException());callback?.Invoke();Check(trace.Any(x=>x.StartsWith("F2 STOP")),mode+" STOP");Check(s.Begin()>0,mode+" clears pending");
}
var counts=new PilotEventCounts();int reads=0;
IReadOnlyList<object?>? Forbidden(){reads++;throw new Exception("payload accessed");}
foreach(var item in new[]{(3,"x"),(103,"x"),(4,"x"),(88,"FrameTick"),(88,"FrameTickAccept"),(88,"PresentStats"),(88,"Retarget"),(88,"TokenLoad")})counts.Observe(item.Item1,item.Item2,Forbidden);
Check(reads==0,"restricted and count-only payload lazy");
Check(counts.Observe(1,"AnimationLifecycle",()=>new object?[]{null,null,null,null,"Created"})=="state=Created","lifecycle state only");
Check(counts.Observe(5,"IdleTransition",()=>new object?[]{null,0,0})=="activeCount=0 pumpOn=0","idle fields");
Check(counts.Summary().Contains("Created=1"),"actual summary");
Console.WriteLine($"PASS {checks} checks");
