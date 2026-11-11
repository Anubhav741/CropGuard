using UnityEngine;
public class PipeSpawner : MonoBehaviour {
 public GameObject pipePrefab;
 public ScoreManager score;
 public bool Playing;
 float nextSpawn;
 public void Restart(){foreach(Transform child in transform)Destroy(child.gameObject);nextSpawn=Time.time+1;Playing=true;}
 void Update(){if(!Playing||Time.time<nextSpawn)return;float difficulty=Mathf.Min(score.Score/20f,1.5f);var pipe=Instantiate(pipePrefab,new Vector3(10,Random.Range(-1.5f,1.5f),0),Quaternion.identity,transform);pipe.GetComponent<PipeMovement>().speed=2.5f+difficulty;pipe.GetComponentInChildren<ScoreGate>().score=score;nextSpawn=Time.time+Mathf.Max(1.5f,2.5f-difficulty*.4f);}
 public void Stop(){Playing=false;foreach(var pipe in GetComponentsInChildren<PipeMovement>())pipe.Moving=false;}
}
