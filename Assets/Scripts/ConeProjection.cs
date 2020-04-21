using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeProjection : MonoBehaviour {
    public float radius;
    public Material mat;
    public int shadowCastLayer;
    public int meshLayer;

    [Range(0, 360)] public int angleStart;
    [Range(0, 360)] public int angleEnd;

    public int angleStartRotate;
    public int angleEndRotate;

    List<Vector2> contactPoints = new List<Vector2>();
    public GameObject lightGameObject;
    Mesh lightMesh;

    void Update() {
        angleStartRotate = angleStart + (int) transform.rotation.eulerAngles.z;
        angleEndRotate = angleEnd + (int) transform.rotation.eulerAngles.z;

        CheckForContactPoints();
        Object.Destroy(lightGameObject);
        CreateLightMesh();
        DrawLightMesh();
    }

    void CheckForContactPoints() {
        contactPoints.Clear();

        Collider2D[] objectsInRadius = Physics2D.OverlapCircleAll(transform.position, radius, 1 << shadowCastLayer);
        foreach (Collider2D obj in objectsInRadius) {
            PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
            for (int i = 0; i < col.pathCount; i++) {
                foreach (Vector2 point in col.GetPath(i)) {
                    Vector2 pointInSpace = ((point * obj.transform.localScale) + (Vector2) obj.transform.position);
                    if (Vector2.Distance(pointInSpace, transform.position) > radius)
                        continue;

                    float angle = Mathf.Atan2(pointInSpace.y - transform.position.y, pointInSpace.x - transform.position.x) * Mathf.Rad2Deg;
                    if (angle < 0)
                        angle = 360 + angle;

                    if (!(angle >= angleStartRotate && angle <= angleEndRotate))
                        continue;

                    Vector2 pos_0 = pointInSpace;
                    Vector2 dir_1 = new Vector2(Mathf.Cos((angle + 1) * Mathf.Deg2Rad), Mathf.Sin((angle + 1) * Mathf.Deg2Rad));
                    Vector2 dir_2 = new Vector2(Mathf.Cos((angle - 1) * Mathf.Deg2Rad), Mathf.Sin((angle - 1) * Mathf.Deg2Rad));

                    RaycastHit2D hit_0 = Physics2D.Linecast(transform.position, pos_0, 1 << shadowCastLayer);
                    RaycastHit2D hit_1 = Physics2D.Raycast(transform.position, dir_1, radius, 1 << shadowCastLayer);
                    RaycastHit2D hit_2 = Physics2D.Raycast(transform.position, dir_2, radius, 1 << shadowCastLayer);

                    if (hit_0)
                        contactPoints.Add(hit_0.point);
                    else
                        contactPoints.Add(pos_0);

                    if (hit_1)
                        contactPoints.Add(hit_1.point);
                    else
                        contactPoints.Add((Vector2) transform.position + dir_1 * radius);

                    if (hit_2)
                        contactPoints.Add(hit_2.point);
                    else
                        contactPoints.Add((Vector2)transform.position + dir_2 * radius);
                }
            }
        }

        for (int i = angleStartRotate; i <= angleEndRotate; i += 12) {
            Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * i), Mathf.Sin(Mathf.Deg2Rad * i)) * radius;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, radius, 1 << shadowCastLayer);

            if (hit)
                contactPoints.Add(hit.point);
            else
                contactPoints.Add((Vector2) transform.position + (direction));
        }

        contactPoints.Sort(new SortClockwise(transform.position));
    }

    void CreateLightMesh() {
        Vector3[] vertices = new Vector3[contactPoints.Count + 1];
        Vector2[] uv = new Vector2[contactPoints.Count + 1];
        int[] triangles = new int[3 * (contactPoints.Count)];

        vertices[0] = transform.position;
        uv[0] = new Vector2(0.5f, 0.5f);
        for (int i = 1; i < vertices.Length; i++) {
            vertices[i] = contactPoints[i - 1];
            Vector2 distanceFromCenter = contactPoints[i - 1] - (Vector2)transform.position;
            uv[i] = distanceFromCenter.normalized * (Vector2.Distance(contactPoints[i - 1], transform.position) / radius) + (new Vector2(0.5f, 0.5f));
        }

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        for (int i = 3; i < triangles.Length - 3; i += 3) {
            triangles[i] = 0;
            triangles[i + 1] = triangles[i - 1];
            triangles[i + 2] = triangles[i - 1] + 1;
        }
            triangles[triangles.Length - 3] = 0;
            triangles[triangles.Length - 2] = contactPoints.Count;
            triangles[triangles.Length - 1] = 1;

        lightMesh = new Mesh();
        lightMesh.vertices = vertices;
        lightMesh.uv = uv;
        lightMesh.triangles = triangles;
    }

    void DrawLightMesh() {
        lightGameObject = new GameObject("shadow", typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D));
        lightGameObject.transform.SetParent(gameObject.transform);
        lightGameObject.layer = meshLayer;
        lightGameObject.GetComponent<MeshFilter>().mesh = lightMesh;
        lightGameObject.GetComponent<MeshRenderer>().material = mat;
        lightGameObject.GetComponent<MeshRenderer>().sortingLayerName = "light";

        Vector2[] points = new Vector2[lightMesh.vertices.Length];
        for (int i = 0; i < points.Length; i++)
            points[i] = lightMesh.vertices[i];
        lightGameObject.GetComponent<PolygonCollider2D>().points = points;
        lightGameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    class SortClockwise : Comparer<Vector2> {
        Vector2 origin;

        public SortClockwise(Vector2 origin) {
            this.origin = origin;
        }

        public override int Compare(Vector2 a, Vector2 b) {
            Vector2 distanceFromCenterA = a - origin;
            Vector2 distanceFromCenterB = b - origin;

            float angleA = Mathf.Atan2(distanceFromCenterA.y, distanceFromCenterA.x) * Mathf.Rad2Deg;
            float angleB = Mathf.Atan2(distanceFromCenterB.y, distanceFromCenterB.x) * Mathf.Rad2Deg;

            if (angleA < 0)
                angleA = 360 + angleA;

            if (angleB < 0)
                angleB = 360 + angleB;

            return angleB.CompareTo(angleA);
        }

    }

}
