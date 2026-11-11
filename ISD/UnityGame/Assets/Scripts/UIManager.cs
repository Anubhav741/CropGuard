using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {
 public GameObject panel; public Text title,details,scoreLabel; public Button play,menu; public GameManager game;
 void Awake(){play.onClick.AddListener(()=>game.Play());menu.onClick.AddListener(()=>game.Menu());}
 public void UpdateScore(int score){scoreLabel.text=score.ToString();}
 public void ShowPlaying(){panel.SetActive(false);scoreLabel.gameObject.SetActive(true);}
 public void ShowMenu(int best){panel.SetActive(true);scoreLabel.gameObject.SetActive(false);title.text="SKY HOP";details.text="A little bird. A big adventure.\n\nSpace or click to flap\nFly through the gaps\n\nBest score: "+best;play.GetComponentInChildren<Text>().text="LET'S FLY";menu.gameObject.SetActive(false);}
 public void ShowGameOver(int score,int best){panel.SetActive(true);title.text="NICE FLIGHT!";details.text="Score: "+score+"\nBest: "+best+"\n\nReady for another adventure?";play.GetComponentInChildren<Text>().text="TRY AGAIN";menu.gameObject.SetActive(true);}
}
