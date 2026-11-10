using System;
using System.Collections.Generic;
[Serializable]
public sealed class JalSimulation {
 public enum Irrigation { None, Drip, Sprinkler, Flood, Pump }
 public enum Allocation { Balanced, FarmsFirst, HomesFirst, Ration }
 public enum Action { Capture, Waste, Recharge, Drip, Sprinkler, Flood, StopIrrigation, PumpFarm, Millet, Harvest, PumpOn, PumpOff, Repair, Plant, WaterTrees, Balanced, FarmsFirst, HomesFirst, Ration, Drainage, Embankment, Tank, ReleaseTank, Roof, School, Animals, Cleanup }
 public float time, climate=-1, pond, groundwater=44, soil=9, crop=14, food=38, happiness=61, prosperity=8, biodiversity=15, quality=100, animalHealth=58, schoolHealth=54, population=40, forestMoisture=8, floodRisk, captured, consumed, wasted, demand, shortage, tank;
 public int coins=80, score, harvests;
 public bool inlet,recharge,dripBuilt,sprinklerBuilt,millet,pump,repaired,trees,drainage,embankment,tankBuilt,roof,school,animals,polluted,clean,finished,won;
 public Irrigation irrigation; public Allocation allocation=Allocation.Ration;
 public string message="Find Ravi at the dry pond. The village is counting on you.";
 public readonly List<string> achievements=new List<string>(), history=new List<string>();
 public const float Duration=420;
 public float Rain { get {if(climate<0)return 0;float t=climate%210;return t>=8&&t<70?1.15f:t>=130&&t<172?1.75f:0;} }
 public bool Storm {get{return climate>=0&&climate%210>=130&&climate%210<172;}}
 public bool Clouds {get{return climate>=0&&(climate%210<8||Rain>0);}}
 public bool Ready {get{return crop>=99.9f;}} public int Cycle {get{return climate<210?1:2;}}
 public string Weather {get{return Storm?"HEAVY MONSOON":Rain>0?"RAIN":Clouds?"CLOUDS GATHERING":"DROUGHT";}}
 public string Objective {get {
 if(climate<0)return "Find Ravi at the dry pond.";
 if(!inlet&&captured<15)return "Open the pond inlet before rain runs away.";
 if(captured<15)return "Watch the channels carry rain into the pond.";
 if(irrigation==Irrigation.None)return "Visit Kavya at the farm. Activate irrigation.";
 if(!millet)return "Visit the seed shed. Diversify to millet.";
 if(Ready)return "Harvest the ripe field for food and coins.";
 if(polluted&&!clean)return "Contain the river spill with trained adults.";
 if(!repaired)return "Repair the leaking pipe at the hand pump.";
 if(!school)return "Connect the school to the water network.";
 if(!animals)return "Open the animal shelter's drinking trough.";
 if(!recharge)return "Build a recharge pit at the harvesting ground.";
 if(!drainage)return "Clear the river drain before heavy rain.";
 if(!trees)return "Plant a forest belt to retain water and soil.";
 return "Supply every sector through both seasonal cycles.";
 }}
 public void ArriveAtPond(){if(climate<0){climate=0;message="RAVI: Clouds are coming! Open the inlet and capture the first rain.";}}
 void Award(string name,int points){if(achievements.Contains(name))return;achievements.Add(name);score+=points;}
 bool Say(string text,bool ok=true){message=text;if(ok){history.Add(text);if(history.Count>8)history.RemoveAt(0);}return ok;}
 bool Buy(int cost){if(coins<cost){message="Need "+cost+" coins. Harvest ripe crops to earn more.";return false;}coins-=cost;return true;}
 public bool Act(Action a){if(finished)return false;switch(a){
 case Action.Capture:inlet=true;return Say("Inlet opened. Collection channels carry rain into the pond.");
 case Action.Waste:inlet=false;return Say("Inlet closed. Incoming rain flows downstream instead of entering storage.");
 case Action.Recharge:if(recharge)return Say("Recharge pit already working.",false);if(!Buy(15))return false;recharge=true;Award("Groundwater Guardian",80);return Say("Clean runoff now soaks into the aquifer through the recharge pit.");
 case Action.Drip:if(pond<3)return Say("Capture 3 pond-water units before irrigating.",false);if(!dripBuilt){if(!Buy(12))return false;dripBuilt=true;Award("Smart Farmer",60);}irrigation=Irrigation.Drip;return Say("Drip lines deliver water directly to roots. Watch the wet rows.");
 case Action.Sprinkler:if(pond<3)return Say("The pond is too low.",false);if(!sprinklerBuilt){if(!Buy(10))return false;sprinklerBuilt=true;}irrigation=Irrigation.Sprinkler;return Say("Sprinklers cover the field, but lose more water than drip lines.");
 case Action.Flood:if(pond<3)return Say("The pond is too low.",false);irrigation=Irrigation.Flood;return Say("Flood channels opened. Storage drains rapidly.");
 case Action.StopIrrigation:irrigation=Irrigation.None;return Say("Irrigation stopped. Rain and existing soil moisture support the crop.");
 case Action.PumpFarm:irrigation=Irrigation.Pump;return Say("Farm pump started. It draws down the shared aquifer.");
 case Action.Millet:if(millet)return Say("Millet already planted.",false);millet=true;Award("Crop Diversifier",60);return Say("Millet needs less water during the next dry spell.");
 case Action.Harvest:if(!Ready)return Say("Crop not ripe. Keep the soil moist and water clean.",false);crop=12;food=Clamp(food+40);harvests++;coins+=32;prosperity=Clamp(prosperity+12);score+=120;return Say("Harvest delivered! +32 coins, food for families and new growth.");
 case Action.PumpOn:pump=true;return Say("Continuous extraction fills the pond but lowers groundwater.");
 case Action.PumpOff:pump=false;return Say("Continuous pumping stopped. The shared aquifer is protected.");
 case Action.Repair:if(repaired)return Say("Pipe already repaired.",false);if(!Buy(6))return false;repaired=true;Award("Leak Detective",70);return Say("Leak fixed. More water reaches the community.");
 case Action.Plant:if(trees)return Say("Saplings planted. Water them during drought.",false);if(!Buy(10))return false;trees=true;forestMoisture=35;Award("Forest Friend",70);return Say("Saplings planted. Healthy roots retain water and reduce erosion.");
 case Action.WaterTrees:if(!trees)return Say("Plant saplings first.",false);if(pond<5)return Say("Need 5 pond-water units.",false);pond-=5;consumed+=5;forestMoisture=Clamp(forestMoisture+45);return Say("Saplings watered. Biodiversity recovers as the grove grows.");
 case Action.Balanced:allocation=Allocation.Balanced;return Say("Families, farms, animals and school share water fairly.");
 case Action.FarmsFirst:allocation=Allocation.FarmsFirst;return Say("Farm priority: other sectors receive less water.");
 case Action.HomesFirst:allocation=Allocation.HomesFirst;return Say("Home priority: farms, school and animals receive less.");
 case Action.Ration:allocation=Allocation.Ration;return Say("Emergency rationing saves water but slows growth.");
 case Action.Drainage:if(drainage)return Say("Drain already clear.",false);if(!Buy(6))return false;drainage=true;Award("Flood Planner",60);return Say("Stormwater now has a safe route away from homes.");
 case Action.Embankment:if(embankment)return Say("Homes already protected.",false);if(!Buy(8))return false;embankment=true;return Say("Raised barriers protect homes during heavy rain.");
 case Action.Tank:if(tankBuilt)return Say("Tank already built.",false);if(!Buy(12))return false;tankBuilt=true;return Say("Overflow can now be saved for the next drought.");
 case Action.ReleaseTank:if(tank<1)return Say("The tank is empty.",false);float release=Math.Min(tank,100-pond);tank-=release;pond+=release;return Say("Tank reserves released into the village pond.");
 case Action.Roof:if(roof)return Say("Roof gutters already connected.",false);if(!Buy(5))return false;roof=true;return Say("Community roofs now collect rain into storage.");
 case Action.School:if(school)return Say("School connection already open.",false);school=true;Award("Water for Learning",40);return Say("School connected. Fair allocation keeps children learning.");
 case Action.Animals:if(animals)return Say("Trough already open.",false);animals=true;Award("Every Life Matters",40);return Say("Trough opened. Animals receive their share of clean water.");
 case Action.Cleanup:if(!polluted||clean)return Say("No active spill. Check after storms.",false);clean=true;quality=Clamp(quality+30);Award("Pond Protector",90);return Say("Trained adults contain the spill. Children stay clear while water quality recovers.");
 }return false;}
 public void Tick(float dt){while(dt>0&&!finished){float s=Math.Min(dt,.1f);Step(s);dt-=s;}}
 void Step(float dt){time+=dt;if(climate>=0)climate+=dt;float r=Rain;
 if(climate>=90&&!polluted){polluted=true;message="TARA: A container is leaking beside the river. Ask trained adults to contain it!";}
 if(polluted&&!clean)quality-=.22f*dt;else if(clean)quality+=.1f*dt;
 groundwater+=r*(recharge&&quality>55?.25f+biodiversity*.001f:.02f)*dt;
 float input=r*(inlet?1:0)+(roof?r*.25f:0);
 if(pump){float n=Math.Min(groundwater,.65f*dt);groundwater-=n;input+=n/dt;}
 float added=Math.Min(100-pond,input*dt);pond+=added;captured+=Math.Min(added,(r*(inlet?1:0)+(roof?r*.25f:0))*dt);
 float overflow=Math.Max(0,input*dt-added);if(tankBuilt){float store=Math.Min(60-tank,overflow);tank+=store;overflow-=store;}
 wasted+=overflow+(!inlet?r*dt:0);if(captured>=10)Award("First Drop Saved",100);
 floodRisk=Clamp(floodRisk+(Storm&&!drainage?overflow/dt*.8f+.13f:-.8f)*dt);
 if(Storm&&!embankment)prosperity=Clamp(prosperity-floodRisk*.003f*dt);
 float leak=repaired?0:Math.Min(pond,.055f*dt);pond-=leak;wasted+=leak;pond-=Math.Min(pond,.025f*dt);
 float rate=irrigation==Irrigation.Drip?.15f:irrigation==Irrigation.Sprinkler?.28f:irrigation==Irrigation.Flood?.65f:irrigation==Irrigation.Pump?.42f:0;
 float efficiency=irrigation==Irrigation.Drip?4.8f:irrigation==Irrigation.Sprinkler?3.2f:2.1f;
 float factor=allocation==Allocation.HomesFirst?.5f:allocation==Allocation.FarmsFirst?1.3f:1;
 float water=Math.Min(irrigation==Irrigation.Pump?groundwater:pond,rate*factor*dt);if(irrigation==Irrigation.Pump)groundwater-=water;else pond-=water;
 consumed+=water;wasted+=water*(irrigation==Irrigation.Flood?.45f:irrigation==Irrigation.Sprinkler?.2f:.03f);
 soil=Clamp(soil+r*.8f*dt+water*efficiency-(millet?.25f:.5f)*dt+(trees?biodiversity*.0007f*dt:0));crop=Clamp(crop+(soil>20&&quality>40?(millet?.73f:.57f):-.38f)*dt);
 demand=(.13f+population*.001f)*(allocation==Allocation.Ration?.6f:1);float need=demand*dt,given=quality>45?Math.Min(pond,need):0;pond-=given;
 float rest=Math.Min(groundwater,need-given);groundwater-=rest;given+=rest;consumed+=given;
 bool supplied=given>=need*.95f;shortage=Math.Max(0,shortage+(supplied?-2:1)*dt);
 float fair=allocation==Allocation.Balanced?1:allocation==Allocation.Ration?.75f:.45f;
 happiness=Clamp(happiness+(supplied?(allocation==Allocation.FarmsFirst?-.08f:.045f):-.5f)*dt-(food<12?.16f*dt:0));
 animalHealth=Clamp(animalHealth+(animals&&supplied&&quality>45?.08f*fair:-.1f)*dt);schoolHealth=Clamp(schoolHealth+(school&&supplied&&fair>.7f?.085f:-.085f)*dt);
 food=Clamp(food-(.05f+population*.0004f)*dt);
 prosperity=Clamp(prosperity+(supplied&&food>15&&happiness>50&&fair>.7f?.055f:-.035f)*dt);
 population=Math.Max(8,Math.Min(70,population+(happiness>65&&food>25?.018f:happiness<35?-.055f:0)*dt));
 forestMoisture=Clamp(forestMoisture+(r*.9f-.18f)*dt);biodiversity=Clamp(biodiversity+(trees&&forestMoisture>12?.14f:-.018f)*dt);
 if(Storm&&biodiversity<25&&!drainage)soil=Clamp(soil-.18f*dt);
 pond=Clamp(pond);groundwater=Clamp(groundwater);quality=Clamp(quality);
 if(shortage>45||happiness<12||population<=10){finished=true;message=CollapseReason();}
 else if(climate>=Duration){finished=true;won=pond+tank>=15&&groundwater>=22&&harvests>=2&&food>=15&&happiness>=50&&animalHealth>=40&&schoolHealth>=40&&quality>=50;
 if(won){Award("Village Guardian",300);if(wasted<30)Award("Zero Waste",120);message="YOU SAVED THE VILLAGE! Two seasons, a shared water supply and a future for every family.";}else message=CollapseReason();}}
 public string CollapseReason(){if(groundwater<22)return "Groundwater depleted. Capture rain, recharge the aquifer and stop continuous pumping.";if(harvests<2||food<15)return "Food reserves too low. Irrigate efficient crops and return to harvest them.";if(quality<50)return "Pollution damaged the supply. Contain the river spill early with trained adults.";if(animalHealth<40||schoolHealth<40)return "School or animals left behind. Connect both and distribute water fairly.";if(pond+tank<15)return "Too little water remained. Store rain, repair leaks and irrigate efficiently.";return "The community struggled with shortages. Balance allocation and protect food reserves.";}
 static float Clamp(float v){return Math.Max(0,Math.Min(100,v));}
}
