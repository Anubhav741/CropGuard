using System.Collections.Generic;
using UnityEngine;

// Entirely Unity meshes/materials. No generated images or downloaded assets.
public sealed class VillageWorld
{
    public enum Place { Pond, Farm, Seeds, Well, Safety, Grove }
    public sealed class Station { public Place place; public string name; public Vector3 position; public Color color; public Transform marker; }
    public readonly List<Station> stations=new List<Station>();
    public CharacterController player;
    public Transform avatar;
    public Camera camera;
    public Vector3 walkTarget;
    public bool walking;
    readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
    readonly List<Transform> rain=new List<Transform>(), crops=new List<Transform>(), houses=new List<Transform>(), saplings=new List<Transform>();
    readonly List<Renderer> land=new List<Renderer>();
    readonly List<Transform> villagers=new List<Transform>();
    Material groundMaterial, cropMaterial, waterMaterial;
    Transform water, leftLeg, rightLeg, leftArm, rightArm, dripLines, spill, gate, pumpRotor;
    Shader shader;
    float stride;
    readonly Color dry=new Color(.72f,.53f,.29f), green=new Color(.38f,.62f,.29f), blue=new Color(.12f,.67f,.76f), roof=new Color(.76f,.30f,.20f);
    public VillageWorld() {
        shader=Resources.Load<Shader>("VillageColor");
        if(shader==null || !shader.isSupported) throw new System.InvalidOperationException("VillageColor shader is missing or unsupported");
        Build();
    }
    Material Mat(Color color) { Material m; if(!materials.TryGetValue(color,out m)) { m=new Material(shader); m.color=color; materials.Add(color,m); } return m; }
    GameObject Shape(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Color color,bool solid=false) {
        var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.position=pos; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=Mat(color);
        var collider=go.GetComponent<Collider>(); if(!solid) { collider.enabled=false; Object.Destroy(collider); }
        return go;
    }
    GameObject Box(string name,Vector3 pos,Vector3 size,Color color,bool solid=false) { return Shape(name,PrimitiveType.Cube,pos,size,color,solid); }
    GameObject Ball(string name,Vector3 pos,Vector3 size,Color color) { return Shape(name,PrimitiveType.Sphere,pos,size,color); }
    GameObject Disc(string name,Vector3 pos,Vector3 size,Color color,bool solid=false) { return Shape(name,PrimitiveType.Cylinder,pos,size,color,solid); }
    void Child(GameObject go,Transform parent) { go.transform.SetParent(parent,true); }
    void Roof(Vector3 center,Transform parent) {
        var mesh=new Mesh(); mesh.vertices=new [] {new Vector3(-2,0,-1.8f),new Vector3(2,0,-1.8f),new Vector3(0,1.55f,-1.8f),new Vector3(-2,0,1.8f),new Vector3(2,0,1.8f),new Vector3(0,1.55f,1.8f)};
        mesh.triangles=new [] {0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3}; mesh.RecalculateNormals();
        var go=new GameObject("Terracotta roof"); go.AddComponent<MeshFilter>().sharedMesh=mesh; go.AddComponent<MeshRenderer>().sharedMaterial=Mat(roof); go.transform.position=center; Child(go,parent);
    }
    Transform House(Vector3 p,int index) {
        var root=new GameObject("Home "+(index+1)).transform; root.position=p;
        Child(Box("Limewashed walls",p+new Vector3(0,1.3f,0),new Vector3(3.5f,2.6f,3.1f),new Color(.96f,.84f,.59f),true),root); Roof(p+Vector3.up*2.6f,root);
        Child(Box("Door",p+new Vector3(0,.85f,-1.57f),new Vector3(.75f,1.7f,.08f),new Color(.20f,.32f,.31f)),root);
        for(int x=-1;x<=1;x+=2) Child(Box("Window",p+new Vector3(x,1.6f,-1.6f),new Vector3(.55f,.65f,.10f),new Color(.22f,.60f,.63f)),root);
        Child(Box("Doorstep",p+new Vector3(0,.12f,-1.9f),new Vector3(1.1f,.24f,.7f),new Color(.70f,.62f,.44f)),root);
        return root;
    }
    Transform Tree(Vector3 p,float size) {
        var root=new GameObject("Village tree").transform; root.position=p;
        Child(Disc("Trunk",p+Vector3.up*1.2f,new Vector3(.42f,1.2f,.42f),new Color(.40f,.27f,.17f),true),root);
        Child(Ball("Canopy",p+Vector3.up*3,new Vector3(2.8f,2.8f,2.8f),new Color(.29f,.48f,.23f)),root);
        Child(Ball("Canopy top",p+new Vector3(.5f,4,.2f),Vector3.one*1.9f,new Color(.41f,.61f,.26f)),root); root.localScale=Vector3.one*size; return root;
    }
    void StationAt(Place place,string name,Vector3 p,Color color) {
        var marker=Disc(name+" interaction ring",p+Vector3.up*.06f,new Vector3(2.9f,.025f,2.9f),color).transform;
        stations.Add(new Station {place=place,name=name,position=p,color=color,marker=marker});
        Box(name+" sign post",p+new Vector3(1.5f,1,0),new Vector3(.15f,2,.15f),new Color(.38f,.25f,.16f));
        Box(name+" sign",p+new Vector3(1.5f,1.85f,0),new Vector3(1.7f,.65f,.18f),new Color(.96f,.83f,.56f));
    }
    void Build() {
        RenderSettings.fog=false; RenderSettings.ambientLight=Color.white;
        Box("Village ground",new Vector3(0,-.55f,0),new Vector3(50,1,44),dry,true);
        groundMaterial=new Material(Mat(dry));
        for(int x=-2;x<=2;x++) for(int z=-2;z<=2;z++) { var go=Box("Living soil",new Vector3(x*9,.002f,z*8),new Vector3(8.95f,.025f,7.95f),dry); go.GetComponent<Renderer>().sharedMaterial=groundMaterial; land.Add(go.GetComponent<Renderer>()); }
        Box("Main path",new Vector3(0,.035f,0),new Vector3(3,.06f,39),new Color(.89f,.72f,.46f));
        Box("Cross path",new Vector3(0,.045f,2),new Vector3(42,.06f,2.8f),new Color(.89f,.72f,.46f));
        for(int i=0;i<18;i++) { var crack=Box("Drought crack",new Vector3(-21+(i*7)%43,.04f,-18+(i*11)%36),new Vector3(1.2f,.012f,.08f),new Color(.55f,.38f,.22f)); crack.transform.rotation=Quaternion.Euler(0,i*47,0); }
        // Pond: an oval basin with individual stones and a rising water surface.
        Disc("Pond bed",new Vector3(-13,.07f,5),new Vector3(10,.07f,8),new Color(.43f,.34f,.24f));
        for(int i=0;i<16;i++) { float a=i*Mathf.PI/8; var stone=Box("Pond bank",new Vector3(-13+Mathf.Cos(a)*4.8f,.25f,5+Mathf.Sin(a)*3.8f),new Vector3(1.75f,.5f,.9f),new Color(.66f,.61f,.48f)); stone.transform.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0); }
        water=Disc("Stored rainwater",new Vector3(-13,.20f,5),new Vector3(8,.025f,6),blue).transform; waterMaterial=new Material(Mat(blue)); water.GetComponent<Renderer>().sharedMaterial=waterMaterial;
        Box("Rain inlet channel",new Vector3(-13,.12f,11.3f),new Vector3(1.2f,.16f,5),new Color(.49f,.45f,.32f));
        gate=Box("Rain gate",new Vector3(-13,.8f,9),new Vector3(1.4f,1.3f,.25f),new Color(.17f,.39f,.43f)).transform;
        StationAt(Place.Pond,"POND",new Vector3(-9,0,.1f),blue);
        // Farm with irrigation lines and crops that visibly grow.
        Box("Farm soil",new Vector3(12,.08f,7),new Vector3(10,.15f,9),new Color(.43f,.29f,.19f));
        cropMaterial=new Material(Mat(new Color(.64f,.65f,.25f)));
        for(int x=0;x<5;x++) for(int z=0;z<5;z++) { var crop=Box("Millet / crop",new Vector3(8+x*1.8f,.5f,3.6f+z*1.6f),new Vector3(.24f,1,.24f),green).transform; crop.GetComponent<Renderer>().sharedMaterial=cropMaterial; crops.Add(crop); }
        dripLines=new GameObject("Drip irrigation network").transform;
        for(int x=0;x<5;x++) Child(Box("Drip line",new Vector3(8+x*1.8f,.23f,7),new Vector3(.09f,.09f,8),new Color(.15f,.43f,.56f)),dripLines);
        dripLines.gameObject.SetActive(false);
        for(int i=0;i<7;i++) { Box("Fence post",new Vector3(7+i*1.6f,.7f,12),new Vector3(.18f,1.4f,.18f),new Color(.65f,.47f,.25f)); }
        Box("Fence rail",new Vector3(12,1,12),new Vector3(10,.12f,.12f),new Color(.65f,.47f,.25f));
        StationAt(Place.Farm,"FARM",new Vector3(9,0,0),new Color(.78f,.63f,.23f));
        var shed=House(new Vector3(18,0,-5),9); shed.localScale=Vector3.one*.65f;
        StationAt(Place.Seeds,"SEEDS",new Vector3(15,0,-7),new Color(.55f,.73f,.29f));
        // Community homes and shared well.
        Disc("Well masonry",new Vector3(0,.6f,-7),new Vector3(2.2f,.6f,2.2f),new Color(.73f,.67f,.50f),true);
        Disc("Well opening",new Vector3(0,1.23f,-7),new Vector3(1.5f,.02f,1.5f),new Color(.12f,.32f,.38f));
        for(int x=-1;x<=1;x+=2) Box("Well post",new Vector3(x,2,-7),new Vector3(.18f,2.5f,.18f),new Color(.43f,.29f,.17f));
        Box("Well beam",new Vector3(0,3.1f,-7),new Vector3(2.5f,.2f,.25f),new Color(.43f,.29f,.17f));
        pumpRotor=Box("Pump handle",new Vector3(2,.9f,-7),new Vector3(1.4f,.15f,.15f),new Color(.16f,.46f,.51f)).transform;
        StationAt(Place.Well,"WELL",new Vector3(0,0,-10),new Color(.25f,.62f,.68f));
        for(int i=0;i<5;i++) houses.Add(House(new Vector3(-13+i*6,0,17),i));
        for(int i=0;i<3;i++) { var npc=Disc("Villager",new Vector3(-5+i*5,.7f,12),new Vector3(.55f,.7f,.55f),new Color(.85f,.43f+i*.1f,.25f)).transform; Child(Ball("Head",npc.position+Vector3.up*.9f,Vector3.one*.58f,new Color(.71f,.46f,.29f)),npc); villagers.Add(npc); }
        StationAt(Place.Safety,"CLEANUP",new Vector3(-18,0,-5),new Color(.84f,.43f,.25f));
        spill=new GameObject("Contamination spill").transform;
        Child(Disc("Pollution stain",new Vector3(-17,.12f,-1),new Vector3(3,.02f,2),new Color(.44f,.25f,.39f)),spill);
        var barrel=Disc("Leaking barrel",new Vector3(-18,.55f,-1),new Vector3(.8f,.55f,.8f),new Color(.66f,.27f,.18f)); Child(barrel,spill); barrel.transform.rotation=Quaternion.Euler(0,0,65);
        spill.gameObject.SetActive(false);
        StationAt(Place.Grove,"GROVE",new Vector3(8,0,-13),new Color(.35f,.64f,.32f));
        for(int i=0;i<3;i++) saplings.Add(Tree(new Vector3(10+i*3,0,-15),.35f));
        for(int i=0;i<6;i++) Tree(new Vector3(-23,0,-17+i*6),.7f+(i%2)*.2f);
        for(int i=0;i<6;i++) Tree(new Vector3(23,0,-17+i*6),.8f);
        for(int i=0;i<6;i++) Ball("Distant hill",new Vector3(-35+i*14,-1,32),new Vector3(18,9+(i%3)*3,12),new Color(.55f,.65f,.43f));
        BuildPlayer();
        camera=new GameObject("Village Camera").AddComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=12; camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.77f,.85f,.77f); camera.nearClipPlane=.1f; camera.farClipPlane=130;
        camera.transform.position=avatar.position+new Vector3(0,20,-19); camera.transform.LookAt(avatar.position+Vector3.forward*2);
        for(int i=0;i<60;i++) rain.Add(Box("Rain streak",Vector3.zero,new Vector3(.035f,.75f,.035f),new Color(.63f,.83f,.89f)).transform);
    }
    void BuildPlayer() {
        avatar=new GameObject("Player - Water Guardian").transform; avatar.position=new Vector3(0,.1f,-13);
        player=avatar.gameObject.AddComponent<CharacterController>(); player.height=1.8f; player.radius=.32f; player.center=new Vector3(0,.9f,0); player.stepOffset=.3f;
        var body=new GameObject("Avatar").transform; body.SetParent(avatar,false);
        Child(Box("Teal jacket",new Vector3(0,1.05f,-13),new Vector3(.66f,.75f,.42f),new Color(.07f,.52f,.56f)),body);
        Child(Ball("Head",new Vector3(0,1.70f,-13),Vector3.one*.6f,new Color(.76f,.53f,.33f)),body);
        Child(Disc("Sun hat",new Vector3(0,2.0f,-13),new Vector3(.87f,.06f,.87f),new Color(.96f,.76f,.33f)),body);
        Child(Disc("Hat crown",new Vector3(0,2.11f,-13),new Vector3(.57f,.1f,.57f),new Color(.95f,.69f,.25f)),body);
        Child(Box("Backpack",new Vector3(0,1.08f,-13.3f),new Vector3(.45f,.55f,.22f),new Color(.92f,.44f,.19f)),body);
        leftLeg=Box("Left boot",new Vector3(-.20f,.4f,-13),new Vector3(.25f,.65f,.3f),new Color(.16f,.23f,.27f)).transform; leftLeg.SetParent(body,true);
        rightLeg=Box("Right boot",new Vector3(.20f,.4f,-13),new Vector3(.25f,.65f,.3f),new Color(.16f,.23f,.27f)).transform; rightLeg.SetParent(body,true);
        leftArm=Box("Left arm",new Vector3(-.44f,1.05f,-13),new Vector3(.2f,.65f,.23f),new Color(.07f,.52f,.56f)).transform; leftArm.SetParent(body,true);
        rightArm=Box("Right arm",new Vector3(.44f,1.05f,-13),new Vector3(.2f,.65f,.23f),new Color(.07f,.52f,.56f)).transform; rightArm.SetParent(body,true);
    }
    public void Move(Vector2 axes,float dt) {
        Vector3 direction=new Vector3(axes.x,0,axes.y);
        if(direction.sqrMagnitude>.01f) walking=false;
        else if(walking) { direction=walkTarget-avatar.position; direction.y=0; if(direction.magnitude<.35f) { walking=false; direction=Vector3.zero; } }
        direction=Vector3.ClampMagnitude(direction,1); player.Move((direction*6+Vector3.down*4)*dt);
        if(avatar.position.y<-.5f) { player.enabled=false; avatar.position=new Vector3(avatar.position.x,.1f,avatar.position.z); player.enabled=true; }
        var p=avatar.position; p.x=Mathf.Clamp(p.x,-22,22); p.z=Mathf.Clamp(p.z,-19,20); avatar.position=p;
        var body=avatar.GetChild(0); if(direction.sqrMagnitude>.01f) body.rotation=Quaternion.Slerp(body.rotation,Quaternion.LookRotation(direction),dt*12);
        stride+=dt*12*direction.magnitude; float swing=Mathf.Sin(stride)*28*direction.magnitude;
        leftLeg.localRotation=Quaternion.Euler(swing,0,0); rightLeg.localRotation=Quaternion.Euler(-swing,0,0); leftArm.localRotation=Quaternion.Euler(-swing*.7f,0,0); rightArm.localRotation=Quaternion.Euler(swing*.7f,0,0);
        camera.transform.position=Vector3.Lerp(camera.transform.position,avatar.position+new Vector3(0,20,-19),1-Mathf.Exp(-dt*5)); camera.transform.rotation=Quaternion.Euler(46,0,0);
    }
    public Station Nearest() { Station best=null; float distance=3.3f; foreach(var s in stations) { float d=Vector2.Distance(new Vector2(avatar.position.x,avatar.position.z),new Vector2(s.position.x,s.position.z)); if(d<distance) { best=s; distance=d; } } return best; }
    public void Visuals(VillageSimulation sim,float clock) {
        float healthy=Mathf.Clamp01((sim.growth*.65f+sim.health*.35f-15)/80);
        groundMaterial.color=Color.Lerp(dry,green,healthy);
        float fill=sim.pond/100; water.localScale=new Vector3(2+7*fill,.02f,1.5f+5.5f*fill); water.position=new Vector3(-13,.18f+fill*.12f,5);
        waterMaterial.color=Color.Lerp(new Color(.46f,.39f,.22f),blue,sim.quality/100);
        gate.localPosition=new Vector3(-13,sim.gate?1.7f:.8f,9);
        cropMaterial.color=sim.Ready?new Color(.91f,.72f,.22f):Color.Lerp(new Color(.55f,.38f,.20f),new Color(.37f,.66f,.24f),sim.moisture/70);
        for(int i=0;i<crops.Count;i++) { float h=.12f+sim.crop/100*1.4f; crops[i].localScale=new Vector3(sim.millet?.23f:.4f,h,sim.millet?.23f:.4f); var p=crops[i].position; p.y=.21f+h*.5f; crops[i].position=p; crops[i].rotation=Quaternion.Euler(Mathf.Sin(clock*2+i)*3,0,0); }
        dripLines.gameObject.SetActive(sim.irrigation==VillageSimulation.Irrigation.Drip);
        spill.gameObject.SetActive(sim.pollution&&!sim.pollutionHandled);
        for(int i=0;i<houses.Count;i++) { bool visible=i<2 || sim.growth>=(i-1)*15+15; houses[i].gameObject.SetActive(visible); float h=sim.health<25?.5f:1; houses[i].localScale=new Vector3(1,h,1); }
        for(int i=0;i<saplings.Count;i++) saplings[i].localScale=Vector3.one*(sim.trees?.9f:.25f);
        for(int i=0;i<villagers.Count;i++) { var p=villagers[i].position; p.x=-6+i*5+Mathf.Sin(clock*.3f+i)*1.3f; villagers[i].position=p; }
        pumpRotor.rotation=Quaternion.Euler(sim.irrigation==VillageSimulation.Irrigation.GroundwaterPump?Mathf.Sin(clock*10)*35:0,0,0);
        bool wet=sim.RainIn>0;
        for(int i=0;i<rain.Count;i++) { rain[i].gameObject.SetActive(wet); if(wet) { float x=(i*7.31f)%29-14, z=(i*11.7f)%25-10; rain[i].position=new Vector3(avatar.position.x+x,1+Mathf.Repeat(i*.53f-clock*11,12),avatar.position.z+z); } }
        foreach(var s in stations) { float pulse=1+.05f*Mathf.Sin(clock*3); s.marker.localScale=new Vector3(2.9f*pulse,.025f,2.9f*pulse); }
        camera.backgroundColor=wet?new Color(.53f,.66f,.69f):new Color(.77f,.85f,.77f);
    }
}
