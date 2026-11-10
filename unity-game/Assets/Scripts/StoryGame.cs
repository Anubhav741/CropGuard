using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// A self-contained WebGL story game; all visuals use GUI textures, not scene shaders.
public class StoryGame : MonoBehaviour {
    enum ScreenMode { Title, Explore, Dialogue, Outcome, Ending }
    const string SaveKey="aquaguardian.story.v1";
    StoryState state; ScreenMode screen; string outcome=""; Texture2D background, pixel; GUIStyle title, text, small, action, choice, tag; bool stylesReady;
    Color ink=new Color(.075f,.14f,.19f), cream=new Color(.99f,.97f,.91f), teal=new Color(.07f,.38f,.38f), amber=new Color(.98f,.72f,.30f);
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void AquaGuardianResult(string json);
#endif
    [Serializable] class Result { public string scenario="VillageAdventure"; public int score, groundwaterBefore=50, groundwaterAfter, waterSaved; public string sustainabilityRating; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { if(SceneManager.GetActiveScene().name=="Village") new GameObject("AquaGuardian Story").AddComponent<StoryGame>(); }
    void Awake() {
        state=new StoryState(); screen=ScreenMode.Title;
        background=Resources.Load<Texture2D>("VillageStory"); pixel=Texture2D.whiteTexture;
        var camera=new GameObject("Story Camera").AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.09f,.22f,.23f);
    }
    void Styles() {
        if(stylesReady) return; stylesReady=true;
        title=new GUIStyle(GUI.skin.label){fontSize=38,fontStyle=FontStyle.Bold,wordWrap=true}; title.normal.textColor=ink;
        text=new GUIStyle(GUI.skin.label){fontSize=21,wordWrap=true,richText=true}; text.normal.textColor=ink;
        small=new GUIStyle(text){fontSize=16};
        tag=new GUIStyle(small){fontStyle=FontStyle.Bold}; tag.normal.textColor=teal;
        action=new GUIStyle(GUI.skin.button){fontSize=20,fontStyle=FontStyle.Bold,wordWrap=true,alignment=TextAnchor.MiddleCenter};
        choice=new GUIStyle(action){fontSize=19,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,16,8,8)};
    }
    void RectColor(Rect r,Color color) { var old=GUI.color; GUI.color=color; GUI.DrawTexture(r,pixel); GUI.color=old; }
    void Panel(Rect r) { RectColor(new Rect(r.x+5,r.y+6,r.width,r.height),new Color(0,0,0,.18f)); RectColor(r,cream); }
    void OnGUI() {
        Styles(); float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f); float ox=(Screen.width-1280*scale)*.5f, oy=(Screen.height-720*scale)*.5f;
        GUI.matrix=Matrix4x4.TRS(new Vector3(ox,oy,0),Quaternion.identity,new Vector3(scale,scale,1));
        RectColor(new Rect(0,0,1280,720),new Color(.2f,.45f,.38f));
        if(background!=null) GUI.DrawTexture(new Rect(0,0,1280,720),background,ScaleMode.ScaleAndCrop);
        RectColor(new Rect(0,0,1280,720),new Color(.03f,.12f,.16f,.12f));
        if(screen==ScreenMode.Title) DrawTitle();
        else if(screen==ScreenMode.Ending) DrawEnding();
        else { DrawHud(); if(screen==ScreenMode.Explore) DrawExplore(); else if(screen==ScreenMode.Dialogue) DrawDialogue(); else DrawOutcome(); }
    }
    void DrawTitle() {
        Panel(new Rect(60,90,570,535)); GUI.Label(new Rect(95,120,450,30),"A STORY ABOUT EVERY DROP",tag);
        GUI.Label(new Rect(95,160,495,100),"AQUA\nGUARDIAN",title);
        GUI.Label(new Rect(95,280,475,95),"One village. One shared well. Your choices shape what happens next.",text);
        GUI.Label(new Rect(95,380,470,70),"Meet the villagers, hear their stories and decide how to protect the water beneath their feet.",small);
        if(GUI.Button(new Rect(95,475,220,58),"BEGIN STORY",action)) { state=new StoryState(); PlayerPrefs.DeleteKey(SaveKey); screen=ScreenMode.Explore; }
        if(PlayerPrefs.HasKey(SaveKey) && GUI.Button(new Rect(330,475,220,58),"CONTINUE",action)) Load();
        GUI.Label(new Rect(95,555,470,35),"A choice-driven groundwater adventure",small);
    }
    void DrawHud() {
        RectColor(new Rect(0,0,1280,68),new Color(.025f,.12f,.14f,.93f));
        var light=new GUIStyle(tag); light.normal.textColor=Color.white;
        GUI.Label(new Rect(30,23,250,28),"AQUAGUARDIAN  /  STORY",light);
        GUI.Label(new Rect(450,23,780,28),"WELL  "+state.water+"%       TRUST  "+state.trust+"       COINS  "+state.coins+"       SCORE  "+state.score,light);
    }
    void DrawExplore() {
        var ch=StoryContent.Chapters[state.chapter];
        Panel(new Rect(54,110,485,380)); GUI.Label(new Rect(82,135,410,28),"CHAPTER "+(state.chapter+1)+" OF "+StoryContent.Chapters.Length+"  /  "+ch.location,tag);
        GUI.Label(new Rect(82,175,420,100),ch.title,title);
        GUI.Label(new Rect(82,290,390,75),ch.speaker+" is waiting to speak with you. Listen carefully: your decision will change the village.",text);
        if(GUI.Button(new Rect(82,400,350,58),"TALK TO "+ch.speaker,action)) screen=ScreenMode.Dialogue;
        GUI.Label(new Rect(82,462,380,25),"Click to meet the next villager",small);
        Panel(new Rect(800,495,420,150)); GUI.Label(new Rect(830,520,360,30),"YOUR JOURNEY",tag);
        GUI.Label(new Rect(830,557,350,60),"Listen  >  Choose  >  See what changes",text);
    }
    void DrawDialogue() {
        var ch=StoryContent.Chapters[state.chapter];
        Panel(new Rect(55,95,1170,565)); GUI.Label(new Rect(85,120,900,27),ch.location+"  /  "+ch.speaker,tag);
        GUI.Label(new Rect(85,155,1040,58),ch.title,title);
        GUI.Label(new Rect(85,225,1070,115),ch.dialogue,text);
        GUI.Label(new Rect(85,344,850,28),"WHAT WILL YOU DO?",tag);
        for(int i=0;i<ch.choices.Length;i++) if(GUI.Button(new Rect(85,385+i*76,1080,62),(i+1)+"   "+ch.choices[i].text,choice)) { var selected=ch.choices[i]; state.Choose(i); outcome=selected.outcome; screen=ScreenMode.Outcome; Save(); }
    }
    void DrawOutcome() {
        var ch=StoryContent.Chapters[state.chapter]; Panel(new Rect(80,110,770,490));
        GUI.Label(new Rect(115,140,600,28),"THE STORY CHANGES",tag); GUI.Label(new Rect(115,180,650,70),"Your choice matters.",title);
        GUI.Label(new Rect(115,260,650,100),outcome,text); GUI.Label(new Rect(115,380,650,90),"WATER WISDOM  /  "+ch.lesson,small);
        if(GUI.Button(new Rect(115,505,300,58),state.chapter==StoryContent.Chapters.Length-1?"SEE YOUR ENDING":"CONTINUE STORY",action)) { state.Next(); if(state.finished) { screen=ScreenMode.Ending; Finish(); } else { screen=ScreenMode.Explore; Save(); } }
    }
    void DrawEnding() {
        Panel(new Rect(65,75,810,590)); GUI.Label(new Rect(100,105,700,28),"YOUR VILLAGE  /  END OF SEASON",tag);
        GUI.Label(new Rect(100,145,720,95),state.Ending,title);
        string ending=state.water>=65 && state.trust>=60 ? "The well is healthier and neighbours trust one another. Together, they have a plan for the next dry season." : state.water>=40 && state.trust>=35 ? "The village has made progress, but the next season will test its choices. There is still time to work together." : "The well and community need care. The villagers decide to learn from this season and try a different plan.";
        GUI.Label(new Rect(100,260,700,105),ending,text);
        GUI.Label(new Rect(100,375,700,95),"Well: "+state.water+"%    Trust: "+state.trust+"    Coins: "+state.coins+"\nFinal score: "+state.score+"\n\nGame values are illustrative, not scientific measurements.",text);
        if(GUI.Button(new Rect(100,560,260,58),"PLAY AGAIN",action)) { state=new StoryState(); PlayerPrefs.DeleteKey(SaveKey); screen=ScreenMode.Explore; }
        if(GUI.Button(new Rect(375,560,220,58),"MAIN MENU",action)) screen=ScreenMode.Title;
    }
    void Save() { PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(state)); PlayerPrefs.Save(); }
    void Load() { try { var loaded=JsonUtility.FromJson<StoryState>(PlayerPrefs.GetString(SaveKey)); if(loaded!=null && !loaded.finished && loaded.chapter>=0 && loaded.chapter<StoryContent.Chapters.Length && loaded.choices!=null) { state=loaded; if(state.choices.Count>state.chapter) { outcome=StoryContent.Chapters[state.chapter].choices[state.choices[state.chapter]].outcome; screen=ScreenMode.Outcome; } else screen=ScreenMode.Explore; return; } } catch(Exception) {} state=new StoryState(); screen=ScreenMode.Explore; }
    void Finish() {
        PlayerPrefs.DeleteKey(SaveKey); PlayerPrefs.Save(); var result=new Result { score=state.score,groundwaterAfter=state.water,waterSaved=Math.Max(0,state.water-50),sustainabilityRating=state.water>=65?"Excellent":state.water>=40?"Good":"At risk" }; string json=JsonUtility.ToJson(result); Debug.Log("AquaGuardian result: "+json);
#if UNITY_WEBGL && !UNITY_EDITOR
        AquaGuardianResult(json);
#endif
    }
}
