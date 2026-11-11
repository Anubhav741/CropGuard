using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BirdController : MonoBehaviour {
 public float flapVelocity=5.5f;
 Rigidbody2D body;
 public System.Action HitObstacle;
 public bool Playing {get;set;}
 void Awake(){body=GetComponent<Rigidbody2D>();body.gravityScale=1.5f;body.freezeRotation=true;}
 void Update(){if(Playing&&(Input.GetKeyDown(KeyCode.Space)||Input.GetMouseButtonDown(0)))body.linearVelocity=Vector2.up*flapVelocity;}
 void OnCollisionEnter2D(Collision2D collision){if(Playing){Playing=false;HitObstacle?.Invoke();}}
 public void ResetBird(){body.simulated=false;transform.position=Vector3.zero;body.position=Vector2.zero;Physics2D.SyncTransforms();body.simulated=true;body.linearVelocity=Vector2.up*flapVelocity;Playing=true;}
 public void Stop(){Playing=false;body.simulated=false;}
}
