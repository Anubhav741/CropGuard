using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CropGuardGame : MonoBehaviour {
    enum Screen { Menu, Instructions, Settings, Playing, Identify, Treatment, Ready, Paused, Complete, GameOver }
    class Crop { public GameObject obj; public int disease; public bool healed; public FarmQuestion question; public int[] order; }
    Screen screen; readonly List<Crop> crops = new List<Crop>();
    readonly string[] diseases = { "Leaf Spot", "Powdery Mildew", "Rust" };
    readonly string[] treatments = { "Fungicide Spray", "Organic Neem Spray", "Water Only" };
    readonly string[] clues = { "Brown spots with yellow edges", "White powder across the leaves", "Orange-brown patches on leaves" };
    readonly int[] answers = { 0, 1, 0 };
    int score, lives = 3, level = 1, selected = -1, healed; Crop target;
    Camera cam; Rigidbody2D body; GameObject farmer, world, canvasRoot; Transform ui; TMP_FontAsset font; TextMeshProUGUI hud, hint; float messageUntil; string message = ""; float stormTime = 60f;
    Color green = new Color(.17f,.55f,.10f), brown = new Color(.43f,.22f,.10f), cream = new Color(1f,.91f,.72f), ink = new Color(.18f,.11f,.06f);
    void Start() {
        Application.targetFrameRate = 60; level = SceneManager.GetActiveScene().name == "GameLevel3" ? 3 : SceneManager.GetActiveScene().name == "GameLevel2" ? 2 : 1;
        font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        var cameraObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraObj.tag = "MainCamera"; cam = cameraObj.GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.4f; cam.backgroundColor = new Color(.3f,.7f,.9f); cam.transform.position = new Vector3(0,0,-10);
        var ev = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        BuildWorld(); BuildCanvas(); screen = SceneManager.GetActiveScene().name == "MainMenu" ? Screen.Menu : Screen.Playing; if(level==2) { message="MOONLIGHT RESCUE: Save 6 crops before the storm!"; messageUntil=Time.time+6; } Refresh();
    }
    Sprite Sprite(string name) { var t = Resources.Load<Texture2D>(name); if (!t) return null; return UnityEngine.Sprite.Create(t, new Rect(0,0,t.width,t.height), new Vector2(.5f,.5f), 100); }
    GameObject Picture(string name, string resource, Vector3 pos, Vector2 size, int order) {
        var o = new GameObject(name, typeof(SpriteRenderer)); o.transform.SetParent(world.transform); o.transform.position = pos; var r=o.GetComponent<SpriteRenderer>(); r.sprite=Sprite(resource); r.sortingOrder=order;
        if (r.sprite) { var s=r.sprite.bounds.size; o.transform.localScale=new Vector3(size.x/s.x,size.y/s.y,1); } return o;
    }
    void BuildWorld() {
        world = new GameObject("Farm environment");
        if(level==3) BuildNativeFarm(); else for(int i=0;i<3;i++) Picture("Farm backdrop " + i,level==2?"MoonFarm":"Farm",new Vector3(i*19.2f,0,2),new Vector2(19.2f,10.8f),-10);
        var ground = new GameObject("Ground",typeof(BoxCollider2D)); ground.transform.SetParent(world.transform); ground.transform.position=new Vector3(19,-3.0f,0); ground.GetComponent<BoxCollider2D>().size=new Vector2(60,.4f);
        farmer=Picture("Farmer","Farmer",new Vector3(-5,-1.45f,0),new Vector2(2.05f,2.15f),5);
        body=farmer.AddComponent<Rigidbody2D>(); body.freezeRotation=true; body.gravityScale=3; var box=farmer.AddComponent<BoxCollider2D>(); box.size=new Vector2(60,90); box.offset=new Vector2(0,-5);
        // Collider dimensions are in sprite pixels divided by the sprite's pixels-per-unit.
        box.size=new Vector2(6f,10f); box.offset=new Vector2(0,-.5f);
        farmer.AddComponent<Animator>();
        for(int i=0;i<(level==3?7:level==2?6:5);i++) { var o=Picture("Crop " + (i+1),"Crop",new Vector3(i*5f,-1.95f,0),new Vector2(1.9f,1.9f),4); var c=new Crop{obj=o,disease=i%3,order=new[]{0,1,2}}; if(SceneManager.GetActiveScene().name!="MainMenu") {c.question=FarmQuestions.Next(); FarmQuestions.Shuffle(c.order);} crops.Add(c); Tint(c); }
    }
    void Shape(string name, float x,float y,float w,float h,Color color,int order,float angle=0) {
        var o=new GameObject(name,typeof(SpriteRenderer)); o.transform.SetParent(world.transform);o.transform.position=new Vector3(x,y,1);o.transform.localScale=new Vector3(w,h,1);o.transform.rotation=Quaternion.Euler(0,0,angle);var r=o.GetComponent<SpriteRenderer>();r.sprite=UnityEngine.Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);r.color=color;r.sortingOrder=order;
    }
    // Level 3 is deliberately built from simple Unity shapes, not a background image.
    void BuildNativeFarm() {
        Shape("Sky",18,0,70,11,new Color(.51f,.77f,.92f),-20);
        Shape("Sun",-4,3.5f,1.1f,1.1f,new Color(1f,.84f,.18f),-19);
        Shape("Grass",18,-2.78f,70,.42f,new Color(.35f,.68f,.22f),-12);
        Shape("Soil",18,-4.3f,70,2.7f,new Color(.43f,.25f,.14f),-13);
        Shape("Fence top",18,-1.35f,70,.12f,new Color(.66f,.42f,.24f),-16);
        Shape("Fence bottom",18,-1.85f,70,.12f,new Color(.66f,.42f,.24f),-16);
        for(int i=0;i<15;i++) Shape("Fence post",-10+i*4,-1.6f,.18f,1.5f,new Color(.53f,.32f,.17f),-15);
        for(int i=0;i<5;i++) {float x=-8+i*10;Shape("Tree trunk",x,-.5f,.45f,3.5f,new Color(.45f,.27f,.15f),-15);Shape("Tree crown",x,1.5f,2.4f,2.0f,new Color(.24f,.57f,.26f),-14);}
        Shape("Barn",9,-.65f,3,3.8f,new Color(.69f,.25f,.20f),-14);
        Shape("Barn roof",9,1.45f,3.6f,.5f,new Color(.38f,.20f,.13f),-13);
        Shape("Barn door",9,-1.35f,1.1f,2.2f,new Color(.39f,.23f,.14f),-12);
    }
    void Tint(Crop c) { c.obj.GetComponent<SpriteRenderer>().color = c.healed ? Color.white : c.disease==0 ? new Color(.9f,.64f,.25f) : c.disease==1 ? new Color(.7f,.82f,.7f) : new Color(.87f,.47f,.19f); }
    void BuildCanvas() {
        canvasRoot=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); var canvas=canvasRoot.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; var scaler=canvasRoot.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=1f; ui=canvasRoot.transform;
    }
    RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size) { var o=new GameObject(name,typeof(RectTransform)); o.transform.SetParent(parent,false); var r=o.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.anchoredPosition=pos; r.sizeDelta=size; return r; }
    Image Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color) { var r=Rect(parent,name,pos,size); var i=r.gameObject.AddComponent<Image>(); i.color=color; return i; }
    TextMeshProUGUI Label(Transform parent,string text,Vector2 pos,Vector2 size,int fontSize,Color color,FontStyles style=FontStyles.Bold) { var r=Rect(parent,"Text",pos,size); var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=text; t.fontSize=fontSize; t.color=color; t.fontStyle=style; t.alignment=TextAlignmentOptions.Center; t.textWrappingMode=TextWrappingModes.Normal; t.raycastTarget=false; return t; }
    void Button(Transform parent,string text,Vector2 pos,Vector2 size,Color color,UnityEngine.Events.UnityAction action) { var p=Panel(parent,text,pos,size,brown); var inner=Panel(p.transform,"Face",new Vector2(0,5),new Vector2(size.x-8,size.y-9),color); var b=p.gameObject.AddComponent<Button>(); b.targetGraphic=inner; b.onClick.AddListener(action); Label(p.transform,text,Vector2.zero,size,size.x>600?27:37,Color.white); }
    void ClearUI() { for(int i=ui.childCount-1;i>=0;i--) Destroy(ui.GetChild(i).gameObject); }
    void Refresh() {
        ClearUI();
        if(screen==Screen.Menu) { Panel(ui,"shade",Vector2.zero,new Vector2(1920,1080),new Color(0,0,0,.12f)); Label(ui,"CropGuard",new Vector2(0,300),new Vector2(1100,160),115,Color.white); Label(ui,"PLANT DISEASE RESCUE",new Vector2(0,205),new Vector2(950,80),48,new Color(1,.87f,.36f)); Button(ui,"Play",new Vector2(0,65),new Vector2(420,85),green,()=>{FarmQuestions.ResetRun(); SceneManager.LoadScene("GameLevel1");}); Button(ui,"How to Play",new Vector2(0,-35),new Vector2(420,85),brown,()=>{screen=Screen.Instructions;Refresh();}); Button(ui,"Settings",new Vector2(0,-135),new Vector2(420,85),brown,()=>{screen=Screen.Settings;Refresh();}); Button(ui,"Exit",new Vector2(0,-235),new Vector2(420,85),brown,()=>Application.Quit()); return; }
        if(screen==Screen.Instructions || screen==Screen.Settings) { Modal(screen==Screen.Settings?"Settings":"How to Play",screen==Screen.Settings?"Use the system volume controls for sound.\nYour progress resets when you restart.":"A / D or arrow keys: move    •    Space: jump\nE: inspect a nearby crop    •    F: apply\nAnswer each farm question and press F to save the crop. Wrong answers cost a life."); Button(ui,"Back",new Vector2(0,-230),new Vector2(260,70),green,()=>{screen=Screen.Menu;Refresh();}); return; }
        Panel(ui,"HUD background",new Vector2(0,487),new Vector2(1840,74),new Color(.08f,.14f,.15f,.88f)); hud=Label(ui,"",new Vector2(-80,487),new Vector2(1500,70),36,Color.white); UpdateHUD(); Button(ui,"Pause",new Vector2(825,487),new Vector2(145,65),green,()=>{screen=Screen.Paused;Refresh();});
        hint=Label(ui,"",new Vector2(0,-400),new Vector2(1000,70),36,Color.white);
        if(screen==Screen.Identify) { Modal("Farm Challenge", target.question.prompt); for(int i=0;i<3;i++){int n=i; Button(ui,(selected==i?">  ":"")+target.question.options[target.order[i]],new Vector2(0,45-i*80),new Vector2(760,68),selected==i?green:brown,()=>{selected=n;Refresh();});} Button(ui,"Confirm",new Vector2(0,-230),new Vector2(260,70),green,ConfirmDisease); }
        if(screen==Screen.Paused) { Modal("Paused","Take a breath, farmer!"); Button(ui,"Resume",new Vector2(0,0),new Vector2(300,70),green,()=>{screen=Screen.Playing;Refresh();}); Button(ui,"Restart",new Vector2(0,-85),new Vector2(300,70),brown,Restart); Button(ui,"Main Menu",new Vector2(0,-170),new Vector2(300,70),brown,Menu); }
        if(screen==Screen.Complete || screen==Screen.GameOver) { bool win=screen==Screen.Complete; Modal(win?"Level " + level + " Complete!":"Out of lives",win?(lives==3?"3 STARS":lives==2?"2 STARS":"1 STAR") + "\nCrops treated: " + healed + "/" + crops.Count + "     Score: " + score:level==2 && stormTime<=0?"The storm arrived! Try the moonlight rescue again.":"The farm still needs your help. Try again!"); Button(ui,win && level<3?"Next Level":win?"Play Again":"Try Again",new Vector2(0,-30),new Vector2(330,75),green,()=>{if(win && level<3){SceneManager.LoadScene("GameLevel"+(level+1));}else Restart();}); Button(ui,"Main Menu",new Vector2(0,-130),new Vector2(330,75),brown,Menu); }
    }
    void Modal(string title,string detail) { Panel(ui,"Dim",Vector2.zero,new Vector2(1920,1080),new Color(0,0,0,.55f)); Panel(ui,"Frame",Vector2.zero,new Vector2(900,650),brown); Panel(ui,"Card",new Vector2(0,5),new Vector2(880,630),cream); Panel(ui,"Header",new Vector2(0,270),new Vector2(865,90),green); Label(ui,title,new Vector2(0,270),new Vector2(850,85),52,Color.white); Label(ui,detail,new Vector2(0,title=="How to Play"?80:title=="Farm Challenge"?160:170),new Vector2(780,title=="How to Play"?240:title=="Farm Challenge"?145:115),32,ink); }
    void UpdateHUD(){if(hud) hud.text="Lives: " + lives + "                 Level " + level + "                 Score: " + score + "       Crops: " + healed + "/" + crops.Count + (level==2 ? "       Storm: " + Mathf.CeilToInt(stormTime) + "s" : "");}
    void ConfirmDisease(){if(selected<0)return; if(target.order[selected]!=0){Mistake("Not quite. Choose another answer!");return;} selected=-1;screen=Screen.Ready;message="Correct! Press F to apply your solution";messageUntil=Time.time+20;Refresh();}
    void Mistake(string text){lives--;if(lives<=0){screen=Screen.GameOver;Refresh();return;} message=text;messageUntil=Time.time+3;Refresh();}
    void Restart(){ score=0;lives=3;healed=0;selected=-1;target=null;stormTime=60f;foreach(var c in crops){c.healed=false;Tint(c);} farmer.transform.position=new Vector3(-5,-1.45f,0);body.linearVelocity=Vector2.zero;screen=Screen.Playing;Refresh();}
    void Menu(){SceneManager.LoadScene("MainMenu");}
    void Spray(){if(target==null||target.healed)return; target.healed=true;Tint(target); healed++;score+=level==3?200:level==2?150:100; var fx=new GameObject("Healing sparkles",typeof(ParticleSystem));fx.transform.position=target.obj.transform.position;var ps=fx.GetComponent<ParticleSystem>();ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.startColor=Color.yellow;main.startLifetime=.8f;main.startSpeed=2;main.startSize=.15f;main.duration=.8f;main.loop=false;ps.Play();Destroy(fx,2);message=level==3?"Farm solution applied!  +200":level==2?"Moon crop saved!  +150":"Crop saved!  +100";messageUntil=Time.time+3;screen=healed==crops.Count?Screen.Complete:Screen.Playing;target=null;Refresh();}
    void Update(){if(body==null)return; bool playing=screen==Screen.Playing||screen==Screen.Ready; if(level==2 && playing){stormTime=Mathf.Max(0,stormTime-Time.deltaTime); UpdateHUD(); if(stormTime<=0){screen=Screen.GameOver;Refresh();playing=false;}} var kb=Keyboard.current; if(playing&&kb!=null){float x=(kb.dKey.isPressed||kb.rightArrowKey.isPressed?1:0)-(kb.aKey.isPressed||kb.leftArrowKey.isPressed?1:0);body.linearVelocity=new Vector2(x*5,body.linearVelocity.y); if(x!=0){var s=farmer.transform.localScale;s.x=Mathf.Abs(s.x)*Mathf.Sign(x);farmer.transform.localScale=s;} if(kb.spaceKey.wasPressedThisFrame&&Mathf.Abs(body.linearVelocity.y)<.1f)body.linearVelocity=new Vector2(body.linearVelocity.x,9); if(screen==Screen.Playing){Crop nearest=null;float dist=2.1f;foreach(var c in crops) if(!c.healed&&Mathf.Abs(c.obj.transform.position.x-farmer.transform.position.x)<dist){dist=Mathf.Abs(c.obj.transform.position.x-farmer.transform.position.x);nearest=c;} if(nearest!=null){if(hint && Time.time>messageUntil)hint.text="Press E to inspect";if(kb.eKey.wasPressedThisFrame){target=nearest;selected=-1;screen=Screen.Identify;Refresh();}} else if(hint&&Time.time>messageUntil)hint.text="Find crops and solve the farm challenges!";} else if(kb.fKey.wasPressedThisFrame)Spray(); } else body.linearVelocity=new Vector2(0,body.linearVelocity.y); if(hint&&Time.time<messageUntil)hint.text=message; if(playing)cam.transform.position=new Vector3(Mathf.Clamp(farmer.transform.position.x+3,0,28),0,-10); }
}
