using UnityEngine;
public class ScoreGate : MonoBehaviour {
 public ScoreManager score;
 bool counted;
 void OnTriggerEnter2D(Collider2D other){var bird=other.GetComponent<BirdController>();if(!counted&&bird!=null&&bird.Playing){counted=true;score.AddPoint();}}
}
