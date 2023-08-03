using System.Collections.Generic;

namespace Bubbel_Shot
{
    public class PoppingParticleEngine : UnityEngine.MonoBehaviour
    {
        public List<PoppingBubbelParticle> bubbelParticles;

        [UnityEngine.SerializeField] private PoppingBubbelParticle poppingBubbelPrefab;


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

        public void AddBubbel(UnityEngine.Vector2 position, UnityEngine.Color color, int score)
        {
            //todo pop in the right place!
            var newOne = Instantiate(poppingBubbelPrefab);
            newOne.InitializePoppingBubbelParticle(color, score);
            bubbelParticles.Add(newOne);
        }

        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space))
            {
                AddBubbel(UnityEngine.Vector2.zero, UnityEngine.Color.magenta, 0);
            }
            
            if (enabled)
            {
                for (int i = bubbelParticles.Count - 1; i >= 0; i--)
                {
                    //age all particles
                    bubbelParticles[i].currentLife+= UnityEngine.Time.deltaTime;
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
