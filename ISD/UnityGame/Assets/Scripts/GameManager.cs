using UnityEngine;
public class GameManager : MonoBehaviour {
 public BirdController bird; public PipeSpawner spawner; public ScoreManager score; public UIManager ui;
 void Update(){if(!bird.Playing && Input.GetKeyDown(KeyCode.Space))Play();if(Input.GetKeyDown(KeyCode.Escape))Menu();}
 void Start(){bird.HitObstacle=GameOver;score.Changed=ui.UpdateScore;bird.Stop();ui.ShowMenu(score.Best);}
 public void Play(){score.ResetScore();bird.ResetBird();spawner.Restart();ui.ShowPlaying();}
 void GameOver(){bird.Stop();spawner.Stop();score.SaveBest();ui.ShowGameOver(score.Score,score.Best);}
 public void Menu(){bird.Stop();spawner.Stop();ui.ShowMenu(score.Best);}
}
