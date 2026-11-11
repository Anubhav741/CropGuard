using UnityEngine;
public class PipeMovement : MonoBehaviour {
 public float speed=2.5f;
 public bool Moving=true;
 void Update(){if(!Moving)return;transform.Translate(Vector3.left*speed*Time.deltaTime);if(transform.position.x < -12)Destroy(gameObject);}
}
