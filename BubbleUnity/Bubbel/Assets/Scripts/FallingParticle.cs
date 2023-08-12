using UnityEngine;

namespace Bubbel_Shot
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FallingParticle : MonoBehaviour
    {
        private Vector3 speed;
        public Vector3 acceleration = new Vector3(0, 0.5f);
        //public Vector2 location;
        public Color color;
        //public float orientation;
        //public int particleScore;
        [SerializeField] private float terminalVelocity = 15;
        //todo add randomness to drop
        private SpriteRenderer mySprite;

        private void Awake()
        {
            mySprite = GetComponent<SpriteRenderer>();
        }

        public void InitializeFallingParticle(Vector3 location, Color ballColor)
        {
            transform.localPosition = location;
            speed = new Vector3(0, 0);

            Color blackedOutColor = ballColor;

            blackedOutColor.r *= 0.3f;
            blackedOutColor.g *= 0.3f;
            blackedOutColor.b *= 0.3f;

            mySprite.color = blackedOutColor;
            
        }

        public void Update()
        {
            //update location (move)
            transform.localPosition += speed * Time.deltaTime;

            //update speed (accelerate)
            if (speed.magnitude < terminalVelocity)
            {
                speed += acceleration * Time.deltaTime;
            }
        }
    }
}