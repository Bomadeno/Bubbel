using System.Collections.Generic;
using UnityEngine;

namespace Bubbel_Shot
{
    class FallingParticleEngine : MonoBehaviour
    {
        List<FallingParticle> fallingParticles;
        
        [SerializeField] private FallingParticle fallingBallPrefab;


        private void Awake()
        {
            fallingParticles = new List<FallingParticle>();
        }

        /// <summary>
        /// Call to pause the updating and drawing of the particles
        /// </summary>
        public void Pause()
        {
            enabled = false;
        }

        /// <summary>
        /// Call to resume the updating and drawing of the particles
        /// </summary>
        public void Resume()
        {
            enabled = true;
        }

        public void AddBubbel(Vector2 location, Color color)
        {
            var newOne = Instantiate(fallingBallPrefab);
            newOne.InitializeFallingParticle(location, color);
            fallingParticles.Add(newOne);
        }

        public void Update()
        {
            //todo should pool them, and dispose of them properly https://github.com/Bomadeno/Bubbel/issues/3
            for (int i = fallingParticles.Count - 1; i >= 0; i--)
            {
                //remove any dead particles
                if (fallingParticles[i].transform.localPosition.y > 700)
                {
                    fallingParticles[i].gameObject.SetActive(false);
                    fallingParticles.RemoveAt(i);
                }
            }
        }
    }
}
