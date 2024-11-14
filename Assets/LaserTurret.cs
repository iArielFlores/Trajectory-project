using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3f;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    [SerializeField] int maxBounces = 3;  // Limit the number of bounces

    List<Vector3> laserPoints = new List<Vector3>();

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        DrawLaserWithReflection();
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void DrawLaserWithReflection()
    {
        // Clear previous points and set initial point at the barrel's end
        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        Vector3 laserDirection = barrelEnd.forward;  // Initial direction from the barrel end
        Vector3 currentPosition = barrelEnd.position;

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(currentPosition, laserDirection, out RaycastHit hit, 1000f, targetLayer))
            {
                // Add the hit point to the line and calculate reflection
                laserPoints.Add(hit.point);

                // Calculate reflection direction based on the surface normal
                laserDirection = Vector3.Reflect(laserDirection, hit.normal);

                // Set the current position to the hit point for the next bounce
                currentPosition = hit.point;
            }
            else
            {
                // If no more collisions, extend the laser to a far point
                laserPoints.Add(currentPosition + laserDirection * 1000f);
                break;
            }
        }

        // Update the LineRenderer to display the laser path
        line.positionCount = laserPoints.Count;
        for (int i = 0; i < laserPoints.Count; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }
}
