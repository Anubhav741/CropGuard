using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// An online bridge is optional: the original offline game remains playable.
public class CropGuardOnline : MonoBehaviour {
    [Serializable] class Config { public string apiUrl; public string token; }
    [Serializable] class StartBody { public int levelId; }
    [Serializable] class StartResult { public string sessionId; }
    [Serializable] class FinishBody { public string sessionId; public int levelId,score,cropsTreated,livesRemaining,duration; public bool completed; }
    public void VoiceCommand(string command){var game=UnityEngine.Object.FindFirstObjectByType<CropGuardGame>();if(game!=null)game.VoiceCommand(command);}
    public static CropGuardOnline Instance { get; private set; }
    string api,token,sessionId; int activeLevel; float started; bool submitting,starting,saving; int generation;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void CropGuardStatus(string message);
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Init(){if(Instance!=null)return; var o=new GameObject("CropGuardOnline");DontDestroyOnLoad(o);Instance=o.AddComponent<CropGuardOnline>();}
    void Status(string value){Debug.Log("Online: "+value);
#if UNITY_WEBGL && !UNITY_EDITOR
        CropGuardStatus(value);
#endif
    }
    public void Configure(string json){try {var c=JsonUtility.FromJson<Config>(json); if(c==null||string.IsNullOrWhiteSpace(c.apiUrl)||string.IsNullOrWhiteSpace(c.token)){Status("Offline: sign in to save progress.");return;} var url=new Uri(c.apiUrl); if(url.Scheme!="https" && !(url.Scheme=="http" && (url.Host=="localhost"||url.Host=="127.0.0.1"))){Status("Online save requires HTTPS.");return;}api=c.apiUrl.TrimEnd('/');token=c.token;Status("Online save ready.");if(activeLevel>0&&!starting&&!submitting&&string.IsNullOrEmpty(sessionId))StartLevel(activeLevel);}catch{Status("Online configuration invalid; playing offline.");}}
    public void StartLevel(int level){generation++;activeLevel=level;sessionId=null;submitting=false;started=Time.realtimeSinceStartup;if(string.IsNullOrEmpty(token)){Status("Offline mode: this game will not be saved.");return;}starting=true;StartCoroutine(StartSession(level,generation));}
    IEnumerator StartSession(int level,int current){while(saving)yield return null;if(current!=generation)yield break;using(var req=Request("/api/game/start",JsonUtility.ToJson(new StartBody{levelId=level}))){yield return req.SendWebRequest();if(current!=generation)yield break;starting=false;if(req.result!=UnityWebRequest.Result.Success){Status("Could not start online session. Play continues offline.");yield break;}var result=JsonUtility.FromJson<StartResult>(req.downloadHandler.text);sessionId=result?.sessionId;if(string.IsNullOrEmpty(sessionId))Status("Invalid session response. Play continues offline.");else Status("Online session started.");}}
    public void FinishLevel(int level,int score,int crops,int lives,bool completed){if(submitting)return;submitting=true;int duration=Mathf.FloorToInt(Time.realtimeSinceStartup-started);StartCoroutine(WaitAndFinish(level,score,crops,lives,duration,completed,generation));} IEnumerator WaitAndFinish(int level,int score,int crops,int lives,int duration,bool completed,int current){while(starting && current==generation)yield return null;if(level!=activeLevel||current!=generation||string.IsNullOrEmpty(sessionId)){Status("Result not saved: no online session. Play continues offline.");yield break;}var data=new FinishBody{sessionId=sessionId,levelId=level,score=score,cropsTreated=crops,livesRemaining=lives,duration=duration,completed=completed};sessionId=null;saving=true;yield return SendResult(data);saving=false;}
    IEnumerator SendResult(FinishBody data){using(var req=Request("/api/game/complete",JsonUtility.ToJson(data))){yield return req.SendWebRequest();if(req.result==UnityWebRequest.Result.Success)Status("Result saved to your account.");else if(req.responseCode==401)Status("Session expired. Log in again to save future games.");else Status("Could not save this result. Play continues offline.");}}
    UnityWebRequest Request(string path,string json){var req=new UnityWebRequest(api+path,"POST");req.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));req.downloadHandler=new DownloadHandlerBuffer();req.SetRequestHeader("Content-Type","application/json");req.SetRequestHeader("Authorization","Bearer "+token);req.timeout=12;return req;}
}
