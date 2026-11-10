using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// A small, self-contained adventure. Simulation numbers are game units.
public class WaterAdventure : MonoBehaviour
{
    enum Phase { Intro, Rain, Recharge, Farm, Pollution, Finish }
    enum Kind { Drop, Recharge, Valve, Crop, Litter }
    sealed class Target { public GameObject go; public Kind kind; public bool done; public Vector3 position; }
    [Serializable] class Result { public string scenario = "VillageAdventure"; public int score, groundwaterBefore = 40, groundwaterAfter, waterSaved; public string sustainabilityRating; }
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void AquaGuardianResult(string json);
#endif
    readonly List<Target> targets = new List<Target>();
    CharacterController player; Transform avatar; Camera cam;
    Phase phase = Phase.Intro; Target nearby; GUIStyle panel, label, heading, button;
    int water = 40, saved, score, drops, litter; float timer, noticeUntil;
    string notice = "", lesson = ""; Vector2 touchDirection;
    Vector3 cameraOffset = new Vector3(0, 11, -12);
    Texture2D panelTexture;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartGame() { if (SceneManager.GetActiveScene().name == "Village") new GameObject("WaterAdventure").AddComponent<WaterAdventure>(); }
    void Start()
    {
        // Disable the previous prototype's scene objects; the replacement is constructed here.
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects()) if (root != gameObject) root.SetActive(false);
        BuildWorld();
    }
    void BuildWorld()
    {
        RenderSettings.ambientLight = new Color(.7f,.72f,.76f);
        var light = new GameObject("Sun").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.15f; light.transform.rotation = Quaternion.Euler(48,-35,0);
        Box("Grassland", new Vector3(0,-.5f,0), new Vector3(38,1,38), new Color(.37f,.68f,.31f));
        Box("Village path", new Vector3(0,.01f,0), new Vector3(5,.08f,31), new Color(.77f,.66f,.46f));
        Box("Crossroads", new Vector3(0,.02f,0), new Vector3(29,.09f,4), new Color(.77f,.66f,.46f));
        for (int i=0;i<3;i++) House(new Vector3(-12+i*6,0,12));
        for (int i=0;i<8;i++) Tree(new Vector3(-17+i*4.5f,0,-16));
        for (int i=0;i<5;i++) Tree(new Vector3(16,0,-13+i*6));
        Cylinder("Pond", new Vector3(-11,.05f,-8), new Vector3(5,.1f,5), new Color(.13f,.56f,.77f));
        Cylinder("Well", new Vector3(0,.65f,0), new Vector3(1.4f,.7f,1.4f), new Color(.57f,.58f,.56f));
        Cylinder("Well water", new Vector3(0,1.37f,0), new Vector3(1.1f,.04f,1.1f), new Color(.12f,.63f,.87f));
        Box("Farm soil", new Vector3(10,.05f,8), new Vector3(9,.14f,8), new Color(.48f,.29f,.17f));
        for (int i=0;i<4;i++) Box("Farm row", new Vector3(7+i*2,.16f,8), new Vector3(.5f,.12f,7), new Color(.25f,.5f,.17f));
        var p = GameObject.CreatePrimitive(PrimitiveType.Capsule); p.name="Guardian"; p.transform.position=new Vector3(0,1,-8); SetColor(p,new Color(.13f,.5f,.85f)); Destroy(p.GetComponent<Collider>());
        player=p.AddComponent<CharacterController>(); player.height=2; player.radius=.48f; avatar=p.transform;
        var eye1=Sphere("Eye",p.transform.position+new Vector3(-.18f,.35f,.46f),new Vector3(.14f,.14f,.14f),Color.white); eye1.transform.SetParent(p.transform,true);
        var eye2=Sphere("Eye",p.transform.position+new Vector3(.18f,.35f,.46f),new Vector3(.14f,.14f,.14f),Color.white); eye2.transform.SetParent(p.transform,true);
        var cameraObject=new GameObject("Adventure Camera"); cam=cameraObject.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.Skybox; cam.transform.position=avatar.position+cameraOffset; cam.transform.LookAt(avatar.position);
        // The elder introduces the goal in the opening panel.
        var elder=GameObject.CreatePrimitive(PrimitiveType.Capsule); elder.name="Village Elder"; elder.transform.position=new Vector3(-3,1,-5); SetColor(elder,new Color(.95f,.64f,.25f));
        // Objects are collected or operated in the world, not through answer choices.
        for (int i=0;i<5;i++) AddTarget(Kind.Drop, Sphere("Rain drop",new Vector3(-6+i*3, .8f,-11+(i%2)*3),Vector3.one*.7f,new Color(.18f,.78f,1)));
        AddTarget(Kind.Recharge,Cylinder("Recharge pit",new Vector3(-9,.3f,2),new Vector3(1.6f,.3f,1.6f),new Color(.29f,.38f,.27f)));
        AddTarget(Kind.Valve,Cylinder("Irrigation valve",new Vector3(6,.65f,8),new Vector3(.65f,.65f,.65f),new Color(.9f,.35f,.12f)));
        AddTarget(Kind.Crop,Box("Seed basket",new Vector3(10,.5f,4),new Vector3(1,1,1),new Color(.95f,.78f,.2f)));
        for(int i=0;i<4;i++) AddTarget(Kind.Litter,Box("Pollution",new Vector3(-14+i*2,.35f,-5-(i%2)*2),new Vector3(.65f,.65f,.65f),new Color(.69f,.18f,.24f)));
        RefreshTargets();
    }
    void House(Vector3 pos) { Box("House",pos+Vector3.up*1.5f,new Vector3(4,3,4),new Color(.95f,.77f,.51f)); Box("Roof",pos+Vector3.up*3.15f,new Vector3(4.5f,.45f,4.5f),new Color(.68f,.26f,.17f)); Box("Door",pos+new Vector3(0,1,-2.04f),new Vector3(1.1f,2,.12f),new Color(.33f,.18f,.1f)); }
    void Tree(Vector3 pos) { Cylinder("Tree trunk",pos+Vector3.up,new Vector3(.45f,1,.45f),new Color(.38f,.22f,.1f)); Sphere("Leaves",pos+Vector3.up*3,new Vector3(2.7f,2.7f,2.7f),new Color(.18f,.5f,.2f)); }
    static GameObject Box(string name,Vector3 pos,Vector3 scale,Color color) { return Primitive(name,PrimitiveType.Cube,pos,scale,color); }
    static GameObject Sphere(string name,Vector3 pos,Vector3 scale,Color color) { return Primitive(name,PrimitiveType.Sphere,pos,scale,color); }
    static GameObject Cylinder(string name,Vector3 pos,Vector3 scale,Color color) { return Primitive(name,PrimitiveType.Cylinder,pos,scale,color); }
    static GameObject Primitive(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Color color) { var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.position=pos; g.transform.localScale=scale; SetColor(g,color); return g; }
    static void SetColor(GameObject g,Color c) { var r=g.GetComponent<Renderer>(); if(r) r.material.color=c; }
    void AddTarget(Kind kind,GameObject go) { var collider=go.GetComponent<Collider>(); if(collider) collider.isTrigger=true; targets.Add(new Target { go=go,kind=kind,position=go.transform.position }); }
    bool Active(Kind kind) { return phase==Phase.Rain && kind==Kind.Drop || phase==Phase.Recharge && kind==Kind.Recharge || phase==Phase.Farm && (kind==Kind.Valve || kind==Kind.Crop) || phase==Phase.Pollution && kind==Kind.Litter; }
    void RefreshTargets() { foreach(var t in targets) if(t.go) t.go.SetActive(!t.done && Active(t.kind)); }
    void Update()
    {
        if(phase==Phase.Intro || phase==Phase.Finish) return;
        timer+=Time.deltaTime;
        Vector3 input=new Vector3(Input.GetAxisRaw("Horizontal")+touchDirection.x,0,Input.GetAxisRaw("Vertical")+touchDirection.y); input=Vector3.ClampMagnitude(input,1);
        player.Move((input*5f+Vector3.down*2f)*Time.deltaTime);
        var pos=avatar.position; pos.x=Mathf.Clamp(pos.x,-18,18); pos.z=Mathf.Clamp(pos.z,-18,18); avatar.position=pos;
        if(input.sqrMagnitude>.01f) avatar.forward=Vector3.Lerp(avatar.forward,input,Time.deltaTime*10);
        cam.transform.position=Vector3.Lerp(cam.transform.position,avatar.position+cameraOffset,Time.deltaTime*4); cam.transform.LookAt(avatar.position+Vector3.up);
        nearby=null; float distance=2.6f;
        foreach(var t in targets) if(!t.done && Active(t.kind)) { float d=Vector3.Distance(new Vector3(avatar.position.x,0,avatar.position.z),new Vector3(t.position.x,0,t.position.z)); if(d<distance) { distance=d; nearby=t; } }
        if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)) Interact();
        touchDirection=Vector2.zero;
    }
    void Interact()
    {
        if(nearby==null) { Show("Walk closer to a glowing mission object!"); return; }
        var item=nearby; item.done=true; score+=100;
        switch(item.kind)
        {
            case Kind.Drop: drops++; saved+=4; Show("Rain saved! " + drops + "/5 drops collected."); if(drops==5) { phase=Phase.Recharge; lesson="Rainwater can be stored and allowed to soak into the ground."; } break;
            case Kind.Recharge: water+=20; saved+=20; phase=Phase.Farm; lesson="Recharge pits help rain soak into the aquifer."; Show("Recharge pit built!"); break;
            case Kind.Valve: saved+=10; water+=8; Show("Drip irrigation saves water at the roots!"); AdvanceFarm(); break;
            case Kind.Crop: saved+=8; water+=6; Show("Millets need less water than thirsty crops!"); AdvanceFarm(); break;
            case Kind.Litter: litter++; saved+=3; Show("Pollution removed! " + litter + "/4"); if(litter==4) Finish(); break;
        }
        nearby=null; RefreshTargets();
    }
    void AdvanceFarm() { bool valve=false,crop=false; foreach(var t in targets) { if(t.kind==Kind.Valve) valve=t.done; if(t.kind==Kind.Crop) crop=t.done; } if(valve&&crop) { phase=Phase.Pollution; lesson="Drip irrigation and crop diversification reduce demand. Now clean the pond!"; } }
    void Finish()
    {
        phase=Phase.Finish; water=Mathf.Clamp(water,0,100); score+=water*5;
        var result=new Result { score=score,groundwaterAfter=water,waterSaved=saved,sustainabilityRating=water>=70?"Excellent":"Good" };
        string json=JsonUtility.ToJson(result); Debug.Log("AquaGuardian result: "+json);
#if UNITY_WEBGL && !UNITY_EDITOR
        AquaGuardianResult(json);
#endif
    }
    void Show(string text) { notice=text; noticeUntil=Time.time+4; }
    string Objective() { switch(phase) { case Phase.Rain:return "DROUGHT: collect 5 blue rain drops ("+drops+"/5)"; case Phase.Recharge:return "SURPLUS RAIN: build the recharge pit west of the well"; case Phase.Farm:return "FARM: activate drip irrigation and collect the millet seeds"; case Phase.Pollution:return "POLLUTION: clean 4 red pieces near the pond ("+litter+"/4)"; default:return ""; } }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("AquaGuardian/Test adventure sequence")]
    static void TestSequence()
    {
        var game = UnityEngine.Object.FindFirstObjectByType<WaterAdventure>();
        if (game == null || !Application.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
        game.phase = Phase.Rain; game.RefreshTargets();
        foreach (var item in game.targets) { if (item.kind == Kind.Drop) { game.nearby = item; game.Interact(); } }
        if (game.phase != Phase.Recharge || game.drops != 5) throw new Exception("Rain mission failed");
        foreach (var item in game.targets) if (item.kind == Kind.Recharge) { game.nearby = item; game.Interact(); }
        if (game.phase != Phase.Farm) throw new Exception("Recharge mission failed");
        foreach (var item in game.targets) if (item.kind == Kind.Valve || item.kind == Kind.Crop) { game.nearby = item; game.Interact(); }
        if (game.phase != Phase.Pollution) throw new Exception("Farm mission failed");
        foreach (var item in game.targets) if (item.kind == Kind.Litter) { game.nearby = item; game.Interact(); }
        if (game.phase != Phase.Finish || game.litter != 4 || game.score <= 0) throw new Exception("Cleanup or result failed");
        Debug.Log("Adventure sequence checks passed");
    }
#endif
    void Styles()
    {
        if(panel!=null) return;
        panelTexture=new Texture2D(1,1); panelTexture.SetPixel(0,0,new Color(.025f,.11f,.17f,.93f)); panelTexture.Apply();
        panel=new GUIStyle(GUI.skin.box); panel.normal.background=panelTexture;
        label=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true,normal={textColor=Color.white}};
        heading=new GUIStyle(label){fontSize=25,fontStyle=FontStyle.Bold}; button=new GUIStyle(GUI.skin.button){fontSize=18,fixedHeight=45,wordWrap=true};
    }
    void OnGUI()
    {
        Styles(); float scale=Mathf.Clamp(Mathf.Min(Screen.width/1100f,Screen.height/700f),.6f,1.3f); GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1)); float width=Screen.width/scale,height=Screen.height/scale;
        GUILayout.BeginArea(new Rect(12,12,Mathf.Min(580,width-24),phase==Phase.Intro?300:phase==Phase.Finish?330:170),panel);
        GUILayout.Label("AQUAGUARDIAN",heading);
        if(phase==Phase.Intro) { GUILayout.Label("The village elder needs a water hero! Explore the village, collect rain, recharge groundwater, help farmers and clean the pond.",label); GUILayout.Label("Move with WASD or arrow keys. Walk near each object and press E (or tap the hand button).",label); if(GUILayout.Button("Start the adventure",button)) { phase=Phase.Rain; RefreshTargets(); } }
        else if(phase==Phase.Finish) { GUILayout.Label("WATER HERO! You helped the village!",heading); GUILayout.Label("Stars: "+score+"   Groundwater: "+water+"%\nWater saved: "+saved+" game units\n\nRain harvesting, recharge, drip irrigation, millet and clean water all help protect groundwater.",label); if(GUILayout.Button("Play again",button)) SceneManager.LoadScene("Village"); }
        else { GUILayout.Label(Objective(),label); GUILayout.Label("Groundwater: "+water+"%     Stars: "+score,label); if(Time.time<noticeUntil) GUILayout.Label(notice,label); else if(!string.IsNullOrEmpty(lesson)) GUILayout.Label(lesson,label); if(nearby!=null) GUILayout.Label("Nearby: "+nearby.go.name+" — press E or tap the hand",label); }
        GUILayout.EndArea();
        if(phase!=Phase.Intro && phase!=Phase.Finish) {
            float y=height-122; GUILayout.BeginArea(new Rect(12,y,230,110),panel); GUILayout.BeginHorizontal(); if(GUILayout.RepeatButton("◀",button)) touchDirection.x=-1; if(GUILayout.RepeatButton("▲",button)) touchDirection.y=1; if(GUILayout.RepeatButton("▼",button)) touchDirection.y=-1; if(GUILayout.RepeatButton("▶",button)) touchDirection.x=1; GUILayout.EndHorizontal(); GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(width-172,y,160,75),panel); if(GUILayout.Button("INTERACT",button)) Interact(); GUILayout.EndArea();
        }
    }
}
