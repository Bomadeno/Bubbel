using UnityEngine;

namespace Bubbel_Shot
{
    public delegate void VectorAction(Vector2 clickSpot);

    class MouseHandlerOld
    {
        private VectorAction leftClickAction;
        private VectorAction updateTarget;
        public bool mouseEnabled = true;
        private Rect activeArea;

        public MouseHandlerOld(Rect activeArea)
        {
            this.activeArea = activeArea;
        }

        public void SetTargetUpdater(VectorAction updateTarget)
        {
            this.updateTarget = updateTarget;
        }

        public void SetLeftClickAction(VectorAction leftClickAction)
        {
            this.leftClickAction = leftClickAction;
        }

        public Vector2 GetMouseLocation()
        {
            return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        public void Update()
        {

            if (mouseEnabled && IsInActiveArea())
            {
                updateTarget(GetMouseLocation());
                if (Input.GetMouseButtonDown(0))
                {
                    mouseEnabled = true;
                    leftClickAction(GetMouseLocation());
                }
            }
        }

        private bool IsInActiveArea()
        {
            if (Input.mousePosition.x > activeArea.x && Input.mousePosition.x < activeArea.x + activeArea.width
                                                                    && Input.mousePosition.y > activeArea.y && Input.mousePosition.y < activeArea.y + activeArea.height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
