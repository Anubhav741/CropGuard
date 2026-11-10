using System;
using A=VillageSimulation.Action;
public static class VillageChecks {
 [UnityEditor.MenuItem("AquaGuardian/Run simulation checks")]
 public static void Run() {
  var good=new VillageSimulation(); good.Act(A.StoreRain); good.Act(A.Recharge); good.Act(A.RepairLeak);good.Act(A.PlantMillet);
  for(int i=0;i<2260;i++) { if(i==350) { good.Act(A.Drip); good.Act(A.SupplyPond); } if(good.pollution&&!good.pollutionHandled)good.Act(A.CleanPollution);if(good.Ready)good.Act(A.Harvest);good.Tick(.1f); }
  UnityEngine.Debug.Log($"Good: win={good.won} pond={good.pond} aquifer={good.aquifer} health={good.health} growth={good.growth} harvest={good.harvests} coins={good.coins} score={good.score}");
  var bad=new VillageSimulation();bad.Act(A.PumpFarm);bad.Act(A.SupplyWell);bad.Tick(226);
  UnityEngine.Debug.Log($"Bad: win={bad.won} health={bad.health} aquifer={bad.aquifer} finished={bad.finished} time={bad.time}");
  if(!good.won||bad.won||!bad.finished) throw new Exception("Balance check failed");
  var dry=new VillageSimulation(); dry.pond=0; if(dry.Act(A.Drip))throw new Exception("Dry pond accepted irrigation");
  if(!good.capturedRain)throw new Exception("Rain capture did not happen");
  var a=new VillageSimulation();var b=new VillageSimulation();a.Act(A.StoreRain);b.Act(A.StoreRain);a.Tick(45);for(int i=0;i<450;i++)b.Tick(.1f);
  if(Math.Abs(a.pond-b.pond)>.3f)throw new Exception("Frame rate dependence");
  var switcher=new VillageSimulation(); switcher.pond=50; switcher.Act(A.Drip); int points=switcher.score, coins=switcher.coins; switcher.Act(A.Flood); switcher.Act(A.Drip); if(switcher.score!=points || switcher.coins!=coins)throw new Exception("Drip switching repeats purchase/reward");
  var paused=new VillageSimulation();paused.Tick(0);if(paused.time!=0)throw new Exception("Pause advances simulation");
  UnityEngine.Debug.Log("PASS: conservation wins, overpumping loses, dry irrigation blocked, rain stored, frame-step and pause checks");
 }
}
