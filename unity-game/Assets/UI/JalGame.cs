using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using A=JalSimulation.Action;

public sealed class JalGame:MonoBehaviour {
 public JalSimulation Sim {get;private set;} public JalWorld World {get;private set;}
 JalWorld.Site near,opened;bool intro=true,paused,ledger,endLogged;float introTime,endTime,toastUntil;string toast="",feedback="";int lastAchievements;
 GUIStyle title,body,small,accent,button;Texture2D flat,normal,hover;float scale;
 Color ink=new Color(.035f,.12f,.14f),cream=new Color(.98f,.95f,.85f),gold=new Color(.95f,.72f,.3f),teal=new Color(.08f,.42f,.43f);
 struct Choice {public A action;public string label;public Choice(A a,string l){action=a;label=l;}}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(SceneManager.GetActiveScene().name=="Village"&&FindAnyObjectByType<JalGame>()==null)new GameObject("JALRAKSHAK / Game Director").AddComponent<JalGame>();}
 void Start(){Sim=new JalSimulation();World=new JalWorld();Application.targetFrameRate=60;SetCursor(false);}
 void SetCursor(bool capture){Cursor.lockState=capture?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!capture;}
 void OnDestroy(){SetCursor(false);if(World!=null)World.Dispose();if(normal!=null)Destroy(normal);if(hover!=null)Destroy(hover);}
 void Tell(string text){toast=text;toastUntil=Time.unscaledTime+9;}
 void Enter(){intro=false;SetCursor(true);Tell("MEERA: Our pond is empty and crops are dying. Find Ravi at the pond. Can you help us?");}
 void Open(JalWorld.Site site){opened=site;feedback="";SetCursor(false);}
 void Close(){opened=null;SetCursor(true);}
 void Update(){if(World==null)return;
 if(intro){introTime+=Time.deltaTime;World.Cinematic(introTime,false);World.Visuals(Sim,Time.time);if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))Enter();return;}
 if(Sim.finished){SetCursor(false);endTime+=Time.deltaTime;World.Cinematic(endTime,true);World.Visuals(Sim,Time.time);if(!endLogged){endLogged=true;Debug.Log("JALRAKSHAK RESULT win="+Sim.won+" score="+Sim.score+" harvests="+Sim.harvests+" aquifer="+Sim.groundwater);}return;}
 if(Input.GetKeyDown(KeyCode.Escape)){if(opened!=null){Close();return;}paused=!paused;ledger=false;SetCursor(!paused);}
 if(Input.GetKeyDown(KeyCode.Tab)&&opened==null){ledger=!ledger;SetCursor(!ledger&&!paused);}
 if(paused||ledger)return;
 if(opened!=null){if(Input.GetKeyDown(KeyCode.E)){Close();return;}var list=Choices(opened.place);for(int i=0;i<list.Count;i++)if(Input.GetKeyDown(KeyCode.Alpha1+i)){Execute(list[i]);break;}return;}
 World.Move(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y")),Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),Input.GetKeyDown(KeyCode.Space),Time.deltaTime);
 near=World.Nearest();if(near!=null&&near.place==JalWorld.Place.Pond&&Sim.climate<0){Sim.ArriveAtPond();Tell(Sim.message);Open(near);}
 else if(near!=null&&Input.GetKeyDown(KeyCode.E))Open(near);
 string before=Sim.message;if(opened==null)Sim.Tick(Time.deltaTime);if(before!=Sim.message)Tell(Sim.message);
 if(Sim.achievements.Count>lastAchievements){Tell("ACHIEVEMENT: "+Sim.achievements[Sim.achievements.Count-1]+"  /  "+Sim.message);lastAchievements=Sim.achievements.Count;}
 World.Visuals(Sim,Time.time);
 }
 void Execute(Choice c){if(Sim.Act(c.action)){Tell(Sim.message);Close();}else feedback=Sim.message;World.Visuals(Sim,Time.time);}
 List<Choice> Choices(JalWorld.Place p){var c=new List<Choice>();switch(p){
 case JalWorld.Place.Pond:c.Add(new Choice(A.Capture,"Open inlet / capture rain"));c.Add(new Choice(A.Waste,"Close inlet / let runoff escape"));break;
 case JalWorld.Place.Farm:if(Sim.Ready)c.Add(new Choice(A.Harvest,"Harvest food / +32 coins"));c.Add(new Choice(A.Drip,Sim.dripBuilt?"Activate drip irrigation":"Install drip irrigation / 12 coins"));c.Add(new Choice(A.Sprinkler,Sim.sprinklerBuilt?"Activate sprinklers":"Install sprinklers / 10 coins"));c.Add(new Choice(A.Flood,"Open flood irrigation channels"));c.Add(new Choice(A.StopIrrigation,"Stop irrigation"));c.Add(new Choice(A.PumpFarm,"Use groundwater farm pump"));break;
 case JalWorld.Place.Seeds:c.Add(new Choice(A.Millet,Sim.millet?"Millet is growing":"Plant drought-resilient millet"));break;
 case JalWorld.Place.Pump:c.Add(new Choice(A.PumpOn,"Pump continuously into storage"));c.Add(new Choice(A.PumpOff,"Stop continuous extraction"));c.Add(new Choice(A.Repair,Sim.repaired?"Leak repaired":"Repair leaking pipe / 6 coins"));break;
 case JalWorld.Place.Harvesting:c.Add(new Choice(A.Recharge,Sim.recharge?"Recharge pit is active":"Build recharge pit / 15 coins"));break;
 case JalWorld.Place.Homes:c.Add(new Choice(A.Embankment,Sim.embankment?"Flood barriers installed":"Protect homes / 8 coins"));break;
 case JalWorld.Place.River:c.Add(new Choice(A.Drainage,Sim.drainage?"Drain is clear":"Clear storm drain / 6 coins"));c.Add(new Choice(A.Cleanup,"Call trained adults to contain spill"));break;
 case JalWorld.Place.Tank:c.Add(new Choice(A.Tank,Sim.tankBuilt?"Tank is installed":"Install overflow tank / 12 coins"));c.Add(new Choice(A.ReleaseTank,"Release tank reserves into pond"));break;
 case JalWorld.Place.School:c.Add(new Choice(A.School,Sim.school?"School connected":"Connect the school water supply"));break;
 case JalWorld.Place.Animals:c.Add(new Choice(A.Animals,Sim.animals?"Trough connected":"Open the animal drinking trough"));break;
 case JalWorld.Place.Forest:c.Add(new Choice(A.Plant,Sim.trees?"Saplings are planted":"Plant native saplings / 10 coins"));c.Add(new Choice(A.WaterTrees,"Water saplings / 5 pond units"));break;
 case JalWorld.Place.Distribution:c.Add(new Choice(A.Balanced,"Share fairly across all sectors"));c.Add(new Choice(A.FarmsFirst,"Prioritise farms"));c.Add(new Choice(A.HomesFirst,"Prioritise homes"));c.Add(new Choice(A.Ration,"Emergency shared ration"));c.Add(new Choice(A.Roof,Sim.roof?"Roof gutters connected":"Connect roof harvesting / 5 coins"));break;
 }return c;}
 string Context(JalWorld.Place p){switch(p){
 case JalWorld.Place.Square:return Sim.happiness>65?"MEERA: The children are smiling again. Keep food and safe water ready for the next drought.":"MEERA: Our village has not received proper rain for months. The pond is empty and groundwater is disappearing. Please help every family, not just one field.";
 case JalWorld.Place.Pond:return Sim.Rain>0?"RAVI: THE RAINS HAVE ARRIVED! Open the inlet and watch the channels carry water into our pond. Stored rain will help us through the dry season.":"RAVI: Clouds gather when you reach the pond. Prepare the inlet now. Closing it sends precious runoff downstream. The pond visibly fills only when water reaches it.";
 case JalWorld.Place.Farm:return "KAVYA: My crops need water, but the village needs it too. Choose a system, then watch it work in the field. Return to harvest when the crop is ripe.\nCrop "+(int)Sim.crop+"% / soil "+(int)Sim.soil+"% / "+Sim.irrigation;
 case JalWorld.Place.Seeds:return "KAVYA: Millet can tolerate drier conditions than our thirsty first crop. Diversifying reduces irrigation demand without stopping food production.";
 case JalWorld.Place.Pump:return "ASHA: The well belongs to everyone. Pumping can help now, but continuous extraction takes water from tomorrow. The recharge ground is north of the pond.\nGroundwater "+(int)Sim.groundwater+"%";
 case JalWorld.Place.Homes:return "MEERA: A prosperous village builds new homes. Heavy monsoon water can damage them. Raised barriers help, and clearing the river drain keeps floodwater moving safely.";
 case JalWorld.Place.River:return Sim.polluted&&!Sim.clean?"TARA: An unknown container is leaking! Children should stay away. Call trained adults before contamination reaches our supply. The drain also needs a clear route for floodwater.":"TARA: The dry stream becomes a river during the monsoon. Clear the blocked drain before heavy rain. Protect both the water and the families downstream.";
 case JalWorld.Place.Harvesting:return "RAVI: Storage and recharge do different jobs. A recharge pit lets clean rain soak into the ground. A healthy forest helps that water stay in the soil.";
 case JalWorld.Place.Tank:return "RAVI: A full pond can overflow. This tank stores up to 60 extra units. Release those reserves when the next dry spell arrives.\nTank reserve: "+(int)Sim.tank+" / 60";
 case JalWorld.Place.School:return Sim.schoolHealth>65?"TARA: We can attend school again! Keep our connection open and the water fairly shared.":"TARA: Without safe water, children miss school. Connect our supply, then ensure the distribution centre gives us a fair share.";
 case JalWorld.Place.Animals:return Sim.animalHealth>65?"BALA: The goats are recovering. Our village needs water for every life.":"BALA: The animals are thirsty. Open their trough and give them a fair share of clean water. A connected pipe alone cannot fix an empty supply.";
 case JalWorld.Place.Forest:return "BALA: Plant native saplings and water them through drought. Roots retain moisture, slow erosion and support biodiversity. Watch the trees grow with their health.\nForest moisture: "+(int)Sim.forestMoisture+"%";
 default:return "ASHA: Families, farmers, animals and school depend on this network. Giving one sector too much leaves others behind. Roof gutters also collect rainfall.\nCurrent policy: "+Sim.allocation;
 }}
 Texture2D Texture(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
 void Styles(){if(title!=null)return;flat=Texture2D.whiteTexture;normal=Texture(teal);hover=Texture(new Color(.13f,.56f,.53f));title=new GUIStyle(GUI.skin.label){fontSize=32,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=cream;body=new GUIStyle(title){fontSize=18,fontStyle=FontStyle.Normal};small=new GUIStyle(body){fontSize=14};accent=new GUIStyle(small){fontStyle=FontStyle.Bold};accent.normal.textColor=gold;button=new GUIStyle(GUI.skin.button){fontSize=17,fontStyle=FontStyle.Bold,wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(15,12,5,5)};button.normal.background=normal;button.hover.background=hover;button.active.background=hover;button.normal.textColor=cream;button.hover.textColor=cream;button.active.textColor=cream;}
 void Fill(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,flat);GUI.color=old;}
 void Panel(Rect r){Fill(new Rect(r.x+4,r.y+4,r.width,r.height),new Color(0,0,0,.2f));Fill(r,new Color(ink.r,ink.g,ink.b,.96f));}
 void Text(float x,float y,float w,float h,string t,GUIStyle s=null){GUI.Label(new Rect(x,y,w,h),t,s??body);}
 bool Button(float x,float y,float w,float h,string t){return GUI.Button(new Rect(x,y,w,h),t,button);}
 void OnGUI(){if(World==null)return;Styles();scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
 if(intro){Panel(new Rect(55,80,545,565));Text(85,110,470,28,"AN EXPLORATION & WATER SURVIVAL ADVENTURE",accent);Text(85,157,470,100,"JALRAKSHAK\nTHE LAST DROP",title);Text(85,280,470,120,"A village on the edge of drought. A monsoon on the horizon. Walk into the world and help your neighbours build a future, one drop at a time.");Text(85,415,470,95,"WASD / arrows: move   Mouse: camera\nShift: run   Space: jump   E: interact\nEsc: pause   Tab: village map & water budget",small);if(Button(85,555,470,55,"ENTER THE VILLAGE / Enter"))Enter();return;}
 HUD();if(Sim.finished){End();return;}
 if(paused){Panel(new Rect(400,200,480,320));Text(435,230,410,55,"VILLAGE PAUSED",title);Text(435,290,410,55,"Water, crops and villagers wait while paused.");if(Button(435,365,410,48,"Resume / Esc")){paused=false;SetCursor(true);}if(Button(435,430,410,48,"Restart the village"))Restart();return;}
 if(ledger){Ledger();return;}
 if(opened!=null){Interaction();return;}
 if(near!=null){Panel(new Rect(430,540,420,70));Text(450,550,380,25,near.person,accent);Text(450,577,380,27,"E / INTERACT WITH "+near.title,small);}
 if(Time.unscaledTime<toastUntil){Panel(new Rect(310,622,800,80));Text(330,635,760,58,toast,small);}
 Text(22,686,285,22,"E interact / Tab budget / Esc pause",small);Fill(new Rect(638,358,4,4),cream);
 }
 void Meter(float x,string label,float v,Color c){Text(x,21,145,22,label+" "+(int)v+"%",small);Fill(new Rect(x,50,138,6),new Color(.25f,.30f,.29f));Fill(new Rect(x,50,138*Mathf.Clamp01(v/100),6),c);}
 void HUD(){Panel(new Rect(12,10,1256,68));Text(28,27,205,30,"JALRAKSHAK",accent);Meter(215,"POND",Sim.pond,new Color(.2f,.7f,.8f));Meter(370,"GROUNDWATER",Sim.groundwater,new Color(.3f,.65f,.75f));Meter(525,"FOOD",Sim.food,gold);Meter(680,"HAPPINESS",Sim.happiness,new Color(.5f,.75f,.4f));Meter(835,"PROSPERITY",Sim.prosperity,gold);Text(1000,23,245,42,"COINS "+Sim.coins+"   POINTS "+Sim.score+"\nCYCLE "+Sim.Cycle+" / 2  •  "+Sim.Weather,small);
 Panel(new Rect(20,95,310,125));Text(37,108,275,22,"SAVE THE VILLAGE WATER",accent);Text(37,141,275,70,Sim.Objective);}
 void Interaction(){Panel(new Rect(660,100,595,590));Text(686,120,540,25,opened.person,accent);Text(686,151,540,45,opened.title,title);Text(686,205,540,135,Context(opened.place),body);Text(686,340,540,35,feedback,accent);var c=Choices(opened.place);for(int i=0;i<c.Count;i++)if(Button(686,382+i*40,540,35,(i+1)+" / "+c[i].label)){Execute(c[i]);break;}if(opened!=null&&Button(686,637,540,35,"Keep exploring / E"))Close();}
 void Ledger(){Panel(new Rect(90,100,1100,570));Text(120,125,500,45,"VILLAGE MAP & WATER BUDGET",title);
 Fill(new Rect(120,190,450,400),new Color(.21f,.29f,.24f));foreach(var site in World.sites){Vector2 p=new Vector2(345+site.position.x*5.8f,390-site.position.z*5.5f);Fill(new Rect(p.x-4,p.y-4,8,8),site.color);Text(p.x-48,p.y+6,100,28,site.title,new GUIStyle(small){fontSize=10,alignment=TextAnchor.UpperCenter});}Vector3 a=World.avatar.position;Fill(new Rect(341+a.x*5.8f,386-a.z*5.5f,9,9),Color.white);
 Text(610,195,520,280,"Captured rain: "+(int)Sim.captured+"   Consumed: "+(int)Sim.consumed+"   Wasted: "+(int)Sim.wasted+"\nStored: "+(int)(Sim.pond+Sim.tank)+"   Demand/sec: "+Sim.demand.ToString("0.00")+"\nGroundwater: "+(int)Sim.groundwater+"   Quality: "+(int)Sim.quality+"%\nSoil moisture: "+(int)Sim.soil+"%   Crop: "+(int)Sim.crop+"%\nAnimals: "+(int)Sim.animalHealth+"%   School: "+(int)Sim.schoolHealth+"%\nPopulation: "+(int)Sim.population+"   Biodiversity: "+(int)Sim.biodiversity+"%\nFlood risk: "+(int)Sim.floodRisk+"%   Harvests: "+Sim.harvests+"\nDistribution: "+Sim.allocation);
 Text(610,470,520,85,"ACHIEVEMENTS\n"+string.Join(" • ",Sim.achievements.ToArray()),small);Text(120,605,850,25,"White dot: you. Map pauses the simulation. Quantities are illustrative game units.",small);if(Button(940,607,220,40,"Return / Tab")){ledger=false;SetCursor(true);}}
 void End(){Panel(new Rect(300,145,680,490));Text(335,170,610,25,"YOUR VILLAGE / SEASONAL OUTCOME",accent);Text(335,215,610,85,Sim.won?"YOU SAVED THE VILLAGE!":"THE VILLAGE NEEDS YOU",title);Text(335,310,610,100,Sim.message);Text(335,425,610,90,"Score "+Sim.score+" / Harvests "+Sim.harvests+" / Groundwater "+(int)Sim.groundwater+"%\n"+string.Join(" • ",Sim.achievements.ToArray()),small);if(Button(335,550,610,50,"RESTART / TRY A DIFFERENT STRATEGY"))Restart();}
 void Restart(){SceneManager.LoadScene("Village");}
}
