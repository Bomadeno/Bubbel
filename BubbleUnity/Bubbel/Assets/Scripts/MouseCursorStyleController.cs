using UnityEngine;

namespace Bubbel_Shot
{
    public class MouseCursorStyleController : MonoBehaviour
    {
        [SerializeField] private Texture2D crosshair;

        private Rect crosshairRegion;
        private bool isCrosshairMode;

        private void Update()
        {
            if (crosshairRegion.Contains(Input.mousePosition) && !isCrosshairMode)
            {
                Cursor.SetCursor(crosshair, new Vector2(10, 10), CursorMode.Auto);
                isCrosshairMode = true;
            }
            else if (!crosshairRegion.Contains(Input.mousePosition) && isCrosshairMode)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                isCrosshairMode = false;
            }
        }

        public void SetCrosshairRegion(Rect region)
        {
            crosshairRegion = region;
        }
    }
}