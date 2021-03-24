/*

Set this on an empty game object positioned at (0,0,0) and attach your active camera.
The script only runs on mobile devices or the remote app.

*/

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class InputController : MonoBehaviour
{ 
    private Camera camera;
    private Plane plane;
    public bool rotate;

    private float cameraDefaultZ;

    private void Awake()
    {
        camera = Camera.main;

        var position = transform.position;
        cameraDefaultZ = position.y;
        transform.position = position;

        plane.SetNormalAndPosition(Vector3.up, Vector3.zero);

        Globals.inputController = this;
    }

    public LayerMask ignoreLayerForSelect;
    private Vector3 startPos;
    private float startTime;
    public const float MAX_SWIPE_TIME = 0.5f; 
    public const float MIN_SWIPE_DISTANCE = 0.17f;
    public const float MIN_TAP_DISTANCE_DETECT = 0.03f;
    public float cameraMoveFactor = 25f;
    //public float cameraMoveImpulseFactor = 220.0f;
    //public float cameraDragFactor = 10.0f;
    public Vector2 minMaxZoom = new Vector2(12,50);

    private bool lockInputCamera = false;
    public bool setCameraLock { get { return lockInputCamera; } set { lockInputCamera = value; } }

    private void Update()
    { 
        var delta1 = Vector3.zero;
        var Delta2 = Vector3.zero;

        //Scroll
        if (Input.touchCount == 1 && !lockInputCamera)
        {
			Touch t = Input.GetTouch(0);

            if(TouchPhase.Moved == t.phase)
            {
                delta1 = PlanePositionDelta(t) * cameraMoveFactor * Time.deltaTime; 
                camera.transform.Translate(delta1, Space.World);
            }
            if (TouchPhase.Began == t.phase)
            {
                //rb.velocity = Vector3.zero;
                //rb.angularVelocity = Vector3.zero;

                startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
                startTime = Time.time;
            }
            if (TouchPhase.Ended == t.phase)
            {
                if (Time.time - startTime > MAX_SWIPE_TIME) // press too long
                    return;
                
                Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width); 
                Vector3 swipe = new Vector3(endPos.x - startPos.x, 0, endPos.y - startPos.y);

                //float distance = Vector2.Distance(startPos, endPos) * cameraMoveImpulseFactor;
                 
                if (swipe.magnitude < MIN_SWIPE_DISTANCE)
                {
                    if (swipe.magnitude < MIN_TAP_DISTANCE_DETECT)
                    { 
                        var ray = Camera.main.ScreenPointToRay(t.position); 

                        if (Physics.Raycast(ray, out RaycastHit hit, 150f , ~ignoreLayerForSelect.value))
                        {
                            GroundController ground = hit.transform.GetComponent<GroundController>();

                            //Debug.Log(hit.transform.gameObject.name);
                            if (ground != null)
                            {
                                Globals.userInterface.SetCellSelect(ground);
                            }
                        }
                    }
                    return;
                }

                /*Rigidbody rb = camera.GetComponent<Rigidbody>();
                rb.AddForce(camera.transform.rotation * swipe * distance * -1, ForceMode.Impulse);
                rb.drag = cameraDragFactor;*/
            }

            //if (Camera.transform.position.x < -1 * tilemap.mapSizeXY[1] * 3f) Camera.transform.position = new Vector3(-1 * tilemap.mapSizeXY[1] * 3f, Camera.transform.position.y, Camera.transform.position.z);
            //if (Camera.transform.position.x > tilemap.mapSizeXY[1] * 3f) Camera.transform.position = new Vector3(tilemap.mapSizeXY[1] * 3f, Camera.transform.position.y, Camera.transform.position.z);
            //if (Camera.transform.position.z < -1 * tilemap.mapSizeXY[0] * 3f) Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, -1 * tilemap.mapSizeXY[0] * 3f);
            //if (Camera.transform.position.z > tilemap.mapSizeXY[0] * 3f) Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, tilemap.mapSizeXY[0] * 3f);
             
            /*bool inZone = true; 

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit HitInfo, 100.0f))
            {
                //Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 100.0f, Color.yellow);
                if (HitInfo.transform.tag == "Background")
                {
                    inZone = false;
                }
            } 
       
            if(!inZone)
            { 
                gameHandler.LookAtMainCastleOfLocalPlayer();
            }*/
        }
        else
        {
            
        }

        //Pinch
        if (Input.touchCount >= 2 && !lockInputCamera)
        {  
            var pos1  = PlanePosition(Input.GetTouch(0).position);
            var pos2  = PlanePosition(Input.GetTouch(1).position);
            var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
            var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

            //calc zoom
            var zoom = Vector3.Distance(pos1, pos2) /
                       Vector3.Distance(pos1b, pos2b);

            var tempPosition = camera.transform.position; 

            if (Input.touchCount == 3 && rotate && pos2b != pos2)
            { 
                camera.transform.RotateAround(pos1, plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, plane.normal));
            }
            else
            {
                if (Input.touchCount == 2)
                {
                    camera.transform.position = Vector3.LerpUnclamped((pos1 + pos2) / 2, camera.transform.position, 1 / zoom);

                    if (camera.orthographic)
                    {
                        camera.orthographicSize = camera.transform.position.y;
                    } 
                }
                else
                {
                    return;
                }
            }

            if (camera.transform.position.y > minMaxZoom[1])
            {
                camera.transform.position = new Vector3(tempPosition.x, minMaxZoom[1], tempPosition.z);

                if (camera.orthographic)
                {
                    camera.orthographicSize = minMaxZoom[1];
                } 
            }
             
            if (camera.transform.position.y < minMaxZoom[0])
            {
                camera.transform.position = new Vector3(tempPosition.x, minMaxZoom[0], tempPosition.z);

                if (camera.orthographic)
                {
                    camera.orthographicSize = minMaxZoom[0];
                }
            }

            //if (Camera.transform.position.y < minMaxZoom[0]) Camera.transform.position = new Vector3(Camera.transform.position.x, minMaxZoom[0], Camera.transform.position.z);
            //if (Camera.transform.position.x < -1 * tilemap.mapSizeXY[1] * 3f) Camera.transform.position = new Vector3(-1 * tilemap.mapSizeXY[1] * 3f, Camera.transform.position.y, Camera.transform.position.z);
            //if (Camera.transform.position.x > tilemap.mapSizeXY[1] * 3f) Camera.transform.position = new Vector3(tilemap.mapSizeXY[1] * 3f, Camera.transform.position.y, Camera.transform.position.z);
            //if (Camera.transform.position.z < -1 * tilemap.mapSizeXY[0] * 3f) Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, -1 * tilemap.mapSizeXY[0] * 3f);
            //if (Camera.transform.position.z > tilemap.mapSizeXY[0] * 3f) Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, tilemap.mapSizeXY[0] * 3f);
        }
         
    }
     
    public void LookAt(Transform obj, float animationTime = 10f)
    {
        lockInputCamera = true;

        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (plane.Raycast(ray, out var enterNow))
        {
            Vector3 rayPosition = ray.GetPoint(enterNow);
            Vector3 diffrence = new Vector3(obj.position.x - rayPosition.x, 0, (obj.position.z - rayPosition.z));
            Vector3 toVector = camera.transform.position + diffrence;

            Debug.DrawLine(camera.transform.position, rayPosition, Color.green, 15);
            Debug.DrawLine(camera.transform.position, toVector, Color.red, 15);
            Debug.DrawLine(toVector, obj.position, Color.red, 15);

            IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
            {
                for (float t = 0f; t < duration; t += Time.deltaTime)
                {
                    camera.transform.position = Vector3.Lerp(pos1, pos2, t / duration);
                    yield return 0;
                }
                camera.transform.position = pos2; 
                lockInputCamera = false;
            }
            StartCoroutine(LerpFromTo(camera.transform.position, toVector, animationTime));
        }
        else
        {
            Debug.LogError("ErrorRaycast~LookAt!!!");
            lockInputCamera = false;
        }
    }
    public void LookAt(GameObject _obj, float animationTime = 10f)
    { 
        LookAt(_obj.transform, animationTime);
    }
    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = camera.ScreenPointToRay(touch.position);

        if (plane.Raycast(rayBefore, out var enterBefore) && plane.Raycast(rayNow, out var enterNow))
        {
            //Debug.DrawLine(camera.transform.position, rayBefore.GetPoint(enterBefore), Color.green, 5);
            //Debug.DrawLine(camera.transform.position, rayNow.GetPoint(enterNow), Color.red, 5);
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
        }

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = camera.ScreenPointToRay(screenPos);
        if (plane.Raycast(rayNow, out var enterNow))
        {
            return rayNow.GetPoint(enterNow);
        }

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    } 
}