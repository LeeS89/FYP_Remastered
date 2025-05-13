using UnityEngine;
using UnityEngine.EventSystems;

namespace mitaywalle.SparksVFXMobile._Demo.Scripts
{
    public class MouseOrbitImproved : MonoBehaviour
    {
        public Transform target = default;
        public float distance = 5.0f;
        public float touchSpeed = .1f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float scrollSpeed = 10.0f;
        public float touchZoomSpeed = .02f;

        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        public float distanceMin = .5f;
        public float distanceMax = 15f;

        float x = 0.0f;
        float y = 0.0f;
        float beginDistance = 0.0f;

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        void LateUpdate()
        {
            if (target)
            {
                var uiclicked = EventSystem.current != null && EventSystem.current.currentSelectedGameObject;
                float zoomDelta = 0;

                if (Input.mousePresent)
                {
                    if (Input.GetMouseButton(0) && !uiclicked)
                    {
                        x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    }

                    zoomDelta = Input.GetAxis("Mouse ScrollWheel");
                }

                if (Input.touchSupported && Input.touchCount > 0 && !uiclicked)
                {
                    if (Input.touchCount == 1)
                    {
                        var touch = Input.GetTouch(0);
                        x += touch.deltaPosition.x * xSpeed * touchSpeed * distance * 0.02f;
                        y -= touch.deltaPosition.y * ySpeed * touchSpeed * 0.02f;    
                    }
                    

                    if (Input.touchCount >= 2)
                    {
                        Touch touch0 = Input.GetTouch(0);
                        Touch touch1 = Input.GetTouch(1);
                        
                        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                        {
                            beginDistance = (touch0.position - touch1.position).magnitude;
                        }
                        
                        if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                        {
                            zoomDelta = ((touch0.position - touch1.position).magnitude - beginDistance) * touchZoomSpeed;
                        }


                        
                    }
                }

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                distance = Mathf.Clamp(distance - zoomDelta * scrollSpeed, distanceMin,
                    distanceMax);

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}