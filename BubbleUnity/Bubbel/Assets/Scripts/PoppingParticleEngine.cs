using System.Collections.Generic;
using UnityEngine;

namespace Bubbel_Shot
{
    public class PoppingParticleEngine : MonoBehaviour
    {
        public List<PoppingBubbelParticle> bubbelParticles;

        [SerializeField] private PoppingBubbelParticle poppingBubbelPrefab;


        private void Awake()
        {
            bubbelParticles = new List<PoppingBubbelParticle>();
        }

        /// <summary>
        /// Call to pause the updating and drawing of the particles
        /// </summary>
        public void Pause()
        {
            enabled = false;
        }

        /// <summary>
        /// Call to resume the updating and drawing of
        /// the particles
        /// </summary>
        public void Resume()
        {
            enabled = true;
        }

        public void AddBubbel(Vector2 position, Color color, int score)
        {
            var newOne = Instantiate(poppingBubbelPrefab, position, Quaternion.identity, transform);
            newOne.InitializePoppingBubbelParticle(color, score);
            bubbelParticles.Add(newOne);
        }

        public void Update()
        {
            if (enabled)
            {
                for (int i = bubbelParticles.Count - 1; i >= 0; i--)
                {
                    //age all particles
                    bubbelParticles[i].currentLife+= Time.deltaTime;
                    //remove any dead particles
                    if (bubbelParticles[i].currentLife > bubbelParticles[i].lifeSpan)
                    {
                        Destroy(bubbelParticles[i].gameObject);
                        bubbelParticles.RemoveAt(i);
                    }
                }
                //TODO maybe vary location slightly?
            }
        }
    }
}
