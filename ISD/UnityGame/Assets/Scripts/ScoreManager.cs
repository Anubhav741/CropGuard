using UnityEngine;
public class ScoreManager : MonoBehaviour {
 public int Score{get;private set;}
 public int Best=>PlayerPrefs.GetInt("FlappyBird.Best",0);
 public System.Action<int> Changed;
 public void ResetScore(){Score=0;Changed?.Invoke(Score);}
 public void AddPoint(){Score++;Changed?.Invoke(Score);}
 public void SaveBest(){if(Score>Best){PlayerPrefs.SetInt("FlappyBird.Best",Score);PlayerPrefs.Save();}}
}
