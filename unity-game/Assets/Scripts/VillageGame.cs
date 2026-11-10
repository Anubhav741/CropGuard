using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class VillageGame : MonoBehaviour
{
    public VillageSimulation Sim { get; private set; }
    public VillageWorld World { get; private set; }
    bool intro=true, paused, resultSent, rainAnnounced, spillAnnounced;
    VillageWorld.Station nearby, opened;
    readonly HashSet<VillageWorld.Place> visited=new HashSet<VillageWorld.Place>();
    float uiScale=1, offsetX, offsetY, notificationUntil;
    string interactionFeedback="", notification="", speaker="MEERA / VILLAGE ELDER";
    GUIStyle title, body, small, caption, button, centered;
    Texture2D solid, buttonNormal, buttonHover;
    Color navy=new Color(.055f,.15f,.19f), cream=new Color(.99f,.97f,.89f), teal=new Color(.08f,.44f,.45f), gold=new Color(.98f,.74f,.29f);
    struct Command { public VillageSimulation.Action action; public string label; public Command(VillageSimulation.Action a,string l) { action=a;label=l; } }
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void AquaGuardianResult(string json);
#endif
    [Serializable] class Result { public string scenario="VillageSurvival"; public int score,groundwaterBefore=43,groundwaterAfter,waterSaved; public string sustainabilityRating; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { if(SceneManager.GetActiveScene().name=="Village") new GameObject("AquaGuardian - Living Village").AddComponent<VillageGame>(); }
    void Start() { Sim=new VillageSimulation(); World=new VillageWorld(); Application.targetFrameRate=60; Notify("Our village is thirsty. Walk to the pond before the clouds arrive. Your actions will change this place."); }
    void Notify(string message,string who="MEERA / VILLAGE ELDER") { notification=message; speaker=who; notificationUntil=Time.unscaledTime+9; }
    void Update() {
        if(World==null) return;
        if(intro) { if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space)) intro=false; World.Visuals(Sim,Time.time); return; }
        if(Sim.finished) { if(!resultSent) Finish(); return; }
        if(Input.GetKeyDown(KeyCode.Escape)) { if(opened!=null) opened=null; else paused=!paused; }
        if(paused) return;
        if(opened!=null) {
            if(Input.GetKeyDown(KeyCode.E)) { opened=null; return; }
            var commands=Commands(opened.place);
            for(int i=0;i<commands.Count;i++) if(Input.GetKeyDown(KeyCode.Alpha1+i)) Execute(commands[i]);
            return;
        }
        string before=Sim.lastMessage;
        Sim.Tick(Time.deltaTime);
        if(Sim.lastMessage!=before) Notify(Sim.lastMessage);
        if(!rainAnnounced && Sim.RainIn>0) { rainAnnounced=true; Notify("The first rain has arrived! At the pond, open the inlet to store it. A closed inlet lets the rain run away.","RAVI / WATER ENGINEER"); }
        if(!spillAnnounced && Sim.pollution) { spillAnnounced=true; Notify("There is a leaking container near the pond. Visit CLEANUP and call trained adults. Keep children away.","TARA / VILLAGE STUDENT"); }
        if(Input.GetMouseButtonDown(0) && !OverUI(Input.mousePosition)) { Ray ray=World.camera.ScreenPointToRay(Input.mousePosition); Plane plane=new Plane(Vector3.up,Vector3.zero); float distance; if(plane.Raycast(ray,out distance)) { World.walkTarget=ray.GetPoint(distance); World.walking=true; } }
        World.Move(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),Time.deltaTime);
        nearby=World.Nearest();
        if(nearby!=null && !visited.Contains(nearby.place)) { visited.Add(nearby.place); opened=nearby; interactionFeedback=""; World.walking=false; }
        else if(nearby!=null && Input.GetKeyDown(KeyCode.E)) { opened=nearby; interactionFeedback=""; World.walking=false; }
        World.Visuals(Sim,Time.time);
    }
    bool OverUI(Vector3 mouse) { var p=new Vector2((mouse.x-offsetX)/uiScale,(Screen.height-mouse.y-offsetY)/uiScale); return p.y<100 || p.y>605 || (p.x<330 && p.y<255) || (p.x>1060 && p.y<335); }
    void Execute(Command cmd) {
        bool ok=Sim.Act(cmd.action); Notify(Sim.lastMessage,ok?"YOUR ACTION CHANGED THE VILLAGE":"CHECK YOUR RESOURCES");
        if(ok) opened=null; else interactionFeedback=Sim.lastMessage;
        World.Visuals(Sim,Time.time);
    }
    List<Command> Commands(VillageWorld.Place place) {
        var a=new List<Command>();
        switch(place) {
            case VillageWorld.Place.Pond:
                a.Add(new Command(Sim.gate?VillageSimulation.Action.ReleaseRain:VillageSimulation.Action.StoreRain,Sim.gate?"Close inlet / let rain flow away":"Open inlet / store incoming rain"));
                a.Add(new Command(VillageSimulation.Action.Recharge,Sim.recharge?"Recharge bed installed":"Build recharge bed  /  18 coins")); break;
            case VillageWorld.Place.Farm:
                if(Sim.Ready) a.Add(new Command(VillageSimulation.Action.Harvest,"Harvest food  /  earn 28 coins"));
                a.Add(new Command(VillageSimulation.Action.Drip,Sim.dripInstalled?"Use installed drip lines":"Install drip lines from pond  /  12 coins"));
                a.Add(new Command(VillageSimulation.Action.Flood,"Flood field from pond  /  high water use"));
                a.Add(new Command(VillageSimulation.Action.PumpFarm,"Pump groundwater  /  lowers aquifer")); break;
            case VillageWorld.Place.Seeds:
                a.Add(new Command(VillageSimulation.Action.PlantMillet,Sim.millet?"Millet is already growing":"Diversify to millet  /  lower water demand")); break;
            case VillageWorld.Place.Well:
                a.Add(new Command(VillageSimulation.Action.SupplyPond,"Share stored pond water with homes"));
                a.Add(new Command(VillageSimulation.Action.SupplyWell,"Pump the well for homes"));
                a.Add(new Command(VillageSimulation.Action.Ration,"Use an emergency shared water ration"));
                a.Add(new Command(VillageSimulation.Action.RepairLeak,Sim.leakFixed?"Pipe repaired":"Repair leaking pipe  /  8 coins")); break;
            case VillageWorld.Place.Safety:
                a.Add(new Command(VillageSimulation.Action.CleanPollution,"Call trained adults and contain the spill"));
                a.Add(new Command(VillageSimulation.Action.IgnorePollution,"Leave the spill for now")); break;
            case VillageWorld.Place.Grove:
                a.Add(new Command(VillageSimulation.Action.PlantTrees,Sim.trees?"Saplings planted":"Plant saplings  /  10 coins + 6 water")); break;
        }
        return a;
    }
    string Context(VillageWorld.Place place) {
        switch(place) {
            case VillageWorld.Place.Pond: return "RAVI: Rain can run away, fill this pond, or soak into the ground. Open the inlet to collect it. A recharge bed helps clean rain replenish groundwater.\n\nPond: "+(int)Sim.pond+" / 100    Inlet: "+(Sim.gate?"OPEN":"CLOSED");
            case VillageWorld.Place.Farm: return "KAVYA: We need food and a healthy well. Drip irrigation uses less pond water. Flooding uses more. Pumping draws directly from the aquifer. Come back when the crop is ready.\n\nCrop: "+(int)Sim.crop+"%    Soil moisture: "+(int)Sim.moisture+"%";
            case VillageWorld.Place.Seeds: return "KAVYA: Our first crop is thirsty. Growing millet on this field lowers water demand and helps it cope with the dry season. Watch the field after you plant it.";
            case VillageWorld.Place.Well: return "MEERA: The well is shared by everyone. Stored rainwater can supply homes without pumping the aquifer. Fixing the leak reduces daily demand.\n\nAquifer: "+(int)Sim.aquifer+"%    Home supply: "+Sim.supply;
            case VillageWorld.Place.Safety: return Sim.pollution&&!Sim.pollutionHandled?"TARA: An unknown container is leaking near the pond. Children must stay away. Ask trained adults to contain it before water quality falls further.":"TARA: We watch the pond for pollution. During a storm, waste can enter a water source. Visit again if we report a spill.";
            default: return "MEERA: A village needs shade, soil and water. Plant a green belt once we have enough stored water. Saplings will appear here, and the village will grow.";
        }
    }
    void Finish() {
        resultSent=true;
        string json=JsonUtility.ToJson(new Result {score=Sim.score,groundwaterAfter=(int)Sim.aquifer,waterSaved=Sim.capturedRain?(int)Sim.pond:0,sustainabilityRating=Sim.won?"Water-smart":"Needs recovery"});
        Debug.Log("AquaGuardian result: "+json);
#if UNITY_WEBGL && !UNITY_EDITOR
        AquaGuardianResult(json);
#endif
    }
    Texture2D ColorTexture(Color color) { var t=new Texture2D(1,1); t.SetPixel(0,0,color); t.Apply(); return t; }
    void Styles() {
        if(title!=null) return;
        solid=Texture2D.whiteTexture; buttonNormal=ColorTexture(teal); buttonHover=ColorTexture(new Color(.12f,.57f,.54f));
        title=new GUIStyle(GUI.skin.label){fontSize=30,fontStyle=FontStyle.Bold,wordWrap=true}; title.normal.textColor=cream;
        body=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true}; body.normal.textColor=cream;
        small=new GUIStyle(body){fontSize=15}; caption=new GUIStyle(small){fontStyle=FontStyle.Bold}; caption.normal.textColor=gold;
        centered=new GUIStyle(small){alignment=TextAnchor.MiddleCenter};
        button=new GUIStyle(GUI.skin.button){fontSize=18,fontStyle=FontStyle.Bold,wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(18,12,4,4)};
        button.normal.background=buttonNormal; button.hover.background=buttonHover; button.active.background=buttonHover; button.normal.textColor=Color.white;button.hover.textColor=Color.white;button.active.textColor=Color.white;
    }
    void Fill(Rect r,Color color) { Color old=GUI.color;GUI.color=color;GUI.DrawTexture(r,solid);GUI.color=old; }
    void Panel(Rect r) { Fill(new Rect(r.x+4,r.y+4,r.width,r.height),new Color(0,0,0,.16f)); Fill(r,new Color(navy.r,navy.g,navy.b,.95f)); }
    void OnGUI() {
        if(World==null) return; Styles(); uiScale=Mathf.Min(Screen.width/1280f,Screen.height/720f);offsetX=(Screen.width-1280*uiScale)/2;offsetY=(Screen.height-720*uiScale)/2;
        GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(uiScale,uiScale,1));
        if(intro) { DrawIntro(); return; }
        DrawWorldLabels(); DrawHUD();
        if(Sim.finished) { DrawEnd(); return; }
        if(paused) { Panel(new Rect(390,230,500,250)); GUI.Label(new Rect(425,255,420,50),"VILLAGE PAUSED",title); GUI.Label(new Rect(425,310,420,60),"Your water and crops wait while paused.",body); if(GUI.Button(new Rect(425,400,420,48),"Resume  /  Esc",button))paused=false;return; }
        if(opened!=null) { DrawInteraction(); return; }
        if(Time.unscaledTime<notificationUntil) { Panel(new Rect(360,585,630,118)); GUI.Label(new Rect(380,598,590,25),speaker,caption); GUI.Label(new Rect(380,628,590,66),notification,small); }
        if(nearby!=null) { Panel(new Rect(430,495,420,70)); if(GUI.Button(new Rect(445,508,390,44),"E  /  MANAGE "+nearby.name,button)) opened=nearby; }
        GUI.Label(new Rect(20,682,330,25),"WASD / arrows: walk   E: interact   Esc: pause",small);
        if(GUI.Button(new Rect(1130,662,125,40),"Pause",button))paused=true;
    }
    void DrawIntro() {
        Panel(new Rect(65,90,570,540)); GUI.Label(new Rect(98,118,500,30),"AQUAGUARDIAN  /  LIVING VILLAGE",caption);
        GUI.Label(new Rect(98,160,480,90),"Bring water.\nBring life.",title);
        GUI.Label(new Rect(98,270,475,130),"You are the village water guardian. Walk to the pond, catch the monsoon, grow food and help your neighbours survive the dry season.",body);
        GUI.Label(new Rect(98,405,470,85),"WASD / arrows to move, or click a path.\nWalk into a coloured station ring to manage it.\nE reopens a station. Esc pauses.",small);
        GUI.Label(new Rect(98,500,470,35),"Decisions change the village around you.",caption);
        if(GUI.Button(new Rect(98,555,470,50),"ENTER THE VILLAGE  /  Enter",button))intro=false;
    }
    void Meter(float x,string label,float value,Color color) {
        GUI.Label(new Rect(x,19,160,23),label+"  "+Mathf.RoundToInt(value)+"%",small); Fill(new Rect(x,47,150,7),new Color(.25f,.32f,.34f)); Fill(new Rect(x,47,150*Mathf.Clamp01(value/100),7),color);
    }
    void DrawHUD() {
        Panel(new Rect(12,10,1256,77)); GUI.Label(new Rect(28,25,210,35),"AQUAGUARDIAN",caption);
        Meter(240,"POND",Sim.pond,new Color(.2f,.75f,.87f));Meter(410,"AQUIFER",Sim.aquifer,new Color(.31f,.68f,.81f));Meter(580,"VILLAGE",Sim.health,new Color(.49f,.78f,.38f));Meter(750,"GROWTH",Sim.growth,gold);Meter(920,"QUALITY",Sim.quality,new Color(.63f,.79f,.74f));
        GUI.Label(new Rect(1100,22,160,50),"COINS "+Sim.coins+"\nPOINTS "+Sim.score,small);
        Panel(new Rect(20,104,300,146)); GUI.Label(new Rect(36,116,265,27),"DAY "+Sim.Day+"  /  "+Sim.CurrentWeather.ToString().ToUpper(),caption);
        GUI.Label(new Rect(36,151,265,75),Sim.Objective,body);
        float left=Mathf.Max(0,VillageSimulation.Duration-Sim.time); GUI.Label(new Rect(36,224,260,22),"Season ends in "+((int)left/60)+":"+((int)left%60).ToString("00"),small);
        DrawMap();
    }
    Vector2 MapPoint(Vector3 p) { return new Vector2(1080+(p.x+25)/50*165,125+(22-p.z)/44*145); }
    void DrawMap() {
        Panel(new Rect(1065,104,190,205)); GUI.Label(new Rect(1080,110,160,22),"VILLAGE MAP",caption);
        foreach(var s in World.stations) { Vector2 p=MapPoint(s.position); Fill(new Rect(p.x-4,p.y-4,8,8),s.color); GUI.Label(new Rect(p.x-30,p.y+5,70,20),s.name,new GUIStyle(centered){fontSize=10}); }
        Vector2 player=MapPoint(World.avatar.position); Fill(new Rect(player.x-4,player.y-4,9,9),Color.white);
        GUI.Label(new Rect(1080,281,160,20),"White dot: you",small);
    }
    void DrawWorldLabels() {
        foreach(var s in World.stations) {
            Vector3 p=World.camera.WorldToScreenPoint(s.position+Vector3.up*2.5f); if(p.z<0)continue;
            float x=(p.x-offsetX)/uiScale, y=(Screen.height-p.y-offsetY)/uiScale;
            if(y<95 || y>590 || x<40 || x>1240)continue;
            Fill(new Rect(x-51,y-14,102,25),new Color(.04f,.14f,.17f,.8f)); GUI.Label(new Rect(x-50,y-14,100,25),s.name,centered);
        }
    }
    void DrawInteraction() {
        Panel(new Rect(330,145,620,485)); GUI.Label(new Rect(360,168,540,35),opened.name+"  /  WATER STATION",caption);
        GUI.Label(new Rect(360,209,540,110),Context(opened.place),body);
        GUI.Label(new Rect(360,320,540,32),interactionFeedback,small);
        var actions=Commands(opened.place);
        for(int i=0;i<actions.Count;i++) if(GUI.Button(new Rect(360,355+i*50,540,43),(i+1)+"  "+actions[i].label,button)) { Execute(actions[i]); break; }
        if(opened!=null && GUI.Button(new Rect(360,575,540,36),"Keep exploring  /  E or Esc",button))opened=null;
    }
    void DrawEnd() {
        Panel(new Rect(330,155,620,455)); GUI.Label(new Rect(365,182,550,28),"SEASON COMPLETE",caption);
        GUI.Label(new Rect(365,222,540,85),Sim.won?"Your village thrives.":"A village needs water.",title);
        GUI.Label(new Rect(365,310,540,105),Sim.lastMessage,body);
        GUI.Label(new Rect(365,430,540,60),"Score "+Sim.score+"   |   Harvests "+Sim.harvests+"   |   Aquifer "+(int)Sim.aquifer+"%\nTry another strategy and watch the village change.",small);
        if(GUI.Button(new Rect(365,525,540,50),"REBUILD THE VILLAGE  /  PLAY AGAIN",button)) SceneManager.LoadScene("Village");
    }
}
