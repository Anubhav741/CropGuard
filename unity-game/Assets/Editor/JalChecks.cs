using System;
using UnityEditor;
using UnityEngine;
using A=JalSimulation.Action;
public static class JalChecks {
 static void Assert(bool condition,string reason){if(!condition)throw new Exception("JAL CHECK FAILED: "+reason);}
 [MenuItem("Jalrakshak/Run gameplay simulation checks")]
 public static void Run(){
 var good=new JalSimulation();good.ArriveAtPond();good.Act(A.Capture);good.Act(A.Recharge);good.Act(A.Repair);good.Act(A.Millet);good.Act(A.Plant);good.Act(A.Drainage);good.Act(A.School);good.Act(A.Animals);good.Act(A.Balanced);good.Act(A.Roof);good.Act(A.Embankment);good.Act(A.Tank);
 for(int i=0;i<4300&&!good.finished;i++){if(good.pond>5&&!good.dripBuilt)good.Act(A.Drip);if(good.Ready)good.Act(A.Harvest);if(good.polluted&&!good.clean)good.Act(A.Cleanup);if(good.pond<15&&good.tank>1)good.Act(A.ReleaseTank);good.Tick(.1f);}
 Debug.Log("JAL GOOD win="+good.won+" pond="+good.pond+" groundwater="+good.groundwater+" food="+good.food+" harvests="+good.harvests+" happiness="+good.happiness+" school="+good.schoolHealth+" animals="+good.animalHealth);
 Assert(good.finished&&good.won,"a conservation strategy must survive two cycles");
 var bad=new JalSimulation();bad.ArriveAtPond();bad.Act(A.PumpOn);bad.Act(A.PumpFarm);bad.Act(A.FarmsFirst);bad.Tick(430);Assert(bad.finished&&!bad.won,"reckless extraction must lose");Assert(bad.groundwater<22,"extraction lowers groundwater");
 var dry=new JalSimulation();Assert(!dry.Act(A.Drip),"dry pond rejects irrigation");dry.Tick(10);Assert(dry.climate<0&&dry.Rain==0,"rain waits for reaching the pond");dry.ArriveAtPond();dry.Tick(9);Assert(dry.Rain>0,"rain follows pond event");
 var repeat=new JalSimulation();repeat.pond=50;repeat.Act(A.Drip);int coins=repeat.coins,score=repeat.score;repeat.Act(A.Flood);repeat.Act(A.Drip);Assert(coins==repeat.coins&&score==repeat.score,"upgrade cannot award repeat points");
 var drip=new JalSimulation();var flood=new JalSimulation();drip.pond=flood.pond=80;drip.Act(A.Drip);flood.Act(A.Flood);drip.Tick(20);flood.Tick(20);Assert(drip.pond>flood.pond+5,"flood uses more water than drip");
 var a=new JalSimulation();var b=new JalSimulation();a.ArriveAtPond();b.ArriveAtPond();a.Act(A.Capture);b.Act(A.Capture);a.Tick(50);for(int i=0;i<500;i++)b.Tick(.1f);Assert(Math.Abs(a.pond-b.pond)<.3f,"frame independence");float time=a.time;a.Tick(0);Assert(a.time==time,"pause stops simulation");
 Debug.Log("JALRAKSHAK CHECKS PASS: conservation win, depletion loss, triggered rain, irrigation tradeoffs, upgrade rewards, timing and pause.");
 }
}
