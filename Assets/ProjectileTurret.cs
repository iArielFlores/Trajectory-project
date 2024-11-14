using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0); // Gravity
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle = true;

    [SerializeField] int trajectoryPointCount = 30; // Number of points in the trajectory
    [SerializeField] float timeBetweenPoints = 0.1f; // Time step between each point

    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();
        UpdateTrajectoryPreview();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        // Create a projectile and apply physics
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, barrelEnd.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = projectileSpeed * barrelEnd.forward; // Apply initial velocity to the projectile
    }

    void TrackMouse()
    {
        // Cast a ray to track the mouse position and update the crosshair
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        // Calculate the direction to the target (crosshair)
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;

        // Set the rotation of the turret base to face the target horizontally (ignoring vertical angle)
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        // Get the direction from the barrel end to the crosshair
        Vector3 targetDir = crosshair.transform.position - barrelEnd.position;

        // Calculate the yaw (horizontal rotation) based on the target's position
        targetDir.y = 0;  // Ignore vertical component for horizontal rotation
        float yaw = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;

        // Calculate the pitch (vertical rotation) based on the target's position
        float pitch = Mathf.Atan2(targetDir.y, targetDir.magnitude) * Mathf.Rad2Deg;

        // Apply the calculated rotation to the turret base (horizontal) and gun (vertical)
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, Quaternion.Euler(0, yaw, 0), Time.deltaTime * baseTurnSpeed);
        gun.transform.localRotation = Quaternion.Euler(pitch, 0, 0); // Vertical rotation for the gun
    }

    float? CalculateTrajectoryAngle(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;
        float v = projectileSpeed;
        float g = gravity.y;

        float underRoot = v * v * v * v - g * (g * x * x + 2 * y * v * v);

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = Mathf.Atan2(v * v + root, g * x) * Mathf.Rad2Deg;
            float lowAngle = Mathf.Atan2(v * v - root, g * x) * Mathf.Rad2Deg;
            return useLow ? lowAngle : highAngle;
        }

        return null;
    }

    void UpdateTrajectoryPreview()
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 startPosition = barrelEnd.position;
        Vector3 startVelocity = projectileSpeed * barrelEnd.forward;

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 position = CalculatePositionAtTime(startPosition, startVelocity, time);
            points.Add(position);

            // Check if the trajectory collides with an object
            if (i > 0 && Physics.Raycast(points[i - 1], position - points[i - 1], out RaycastHit hit, (position - points[i - 1]).magnitude, targetLayer))
            {
                points[i] = hit.point; // Adjust the last point to the hit point
                break;
            }
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    Vector3 CalculatePositionAtTime(Vector3 startPosition, Vector3 startVelocity, float time)
    {
        return startPosition + startVelocity * time + 0.5f * gravity * time * time; // Gravity applies here
    }
}
