using UnityEngine;
using Random = System.Random;

namespace Bubbel_Shot
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PoppingBubbelParticle : MonoBehaviour
    {
        public float lifeSpan = 0.15f;
        public float currentLife;
        public Color color;
        public int particleScore;
        Random r;

        public void InitializePoppingBubbelParticle(Color color, int particleScore)
        {
            r = new Random();
            this.color = color;
            transform.Rotate(0,0, (float)r.NextDouble()*360);
            this.particleScore = particleScore;
        }
    }
}