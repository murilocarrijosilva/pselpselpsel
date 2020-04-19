using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp2D : MonoBehaviour {
    public float radius;
    [Range(4, 360)] public int circleVertices = 4;
    List<ContactPoint> contactPoints = new List<ContactPoint>();
    GameObject lightGameObject;
    public Material mat;
    public int shadowLayer;
    public int angleStart;
    public int angleEnd;

    void Update() {
        CheckForContactPoints();
        Object.Destroy(lightGameObject);
        contactPoints.Sort(new SortClockwise());

        if (contactPoints.Count > 0)
            DrawLightMesh();
    }

    void CheckForContactPoints() {
        contactPoints.Clear();
        CheckForShadowObjects();

        for (int i = 0; i < contactPoints.Count; i++) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, contactPoints[i].distanceFromCenter.normalized, radius, 1 << shadowLayer);
            if (hit)
                contactPoints[i] = new ContactPoint(hit.point, transform.position);
            else
                contactPoints[i] = new ContactPoint(transform.position + contactPoints[i].distanceFromCenter.normalized * radius, transform.position);
        }

        for (int i = angleStart; i <= angleEnd; i += 2) {
            Vector2 edge = transform.position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * i), Mathf.Sin(Mathf.Deg2Rad * i)) * radius;
            RaycastHit2D hit = Physics2D.Linecast(transform.position, edge, 1 << shadowLayer);

            if (hit)
                contactPoints.Add(new ContactPoint(hit.point, transform.position));
            else
                contactPoints.Add(new ContactPoint(edge, transform.position));
        }

        foreach (ContactPoint point in contactPoints)
            Debug.DrawLine(transform.position, point.point, Color.magenta);

    }

    void CheckForShadowObjects() {
        Collider2D[] objectsInRadius = Physics2D.OverlapCircleAll(transform.position, radius, 1 << shadowLayer);
        foreach (Collider2D obj in objectsInRadius) {
            PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
            for (int i = 0; i < col.pathCount; i++) {
                foreach(Vector2 point in col.GetPath(i)) {
                    Vector2 relativePos = new Vector2(point.x - transform.position.x, point.y - transform.position.y);
                    if (relativePos.magnitude > radius)
                        continue;
                    
                    float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
                    if (angle < 0)
                        angle = 360 + angle;
                    
                    if (!(angle >= angleStart && angle <= angleEnd))
                        continue;

                    Vector3 relativePosition = new Vector2(point.x * obj.transform.localScale.x, point.y * obj.transform.localScale.y);
                    contactPoints.Add(new ContactPoint(relativePosition + obj.transform.position, transform.position));
                    contactPoints.Add(new ContactPoint(relativePosition + obj.transform.position + Vector3.one * 0.001f, transform.position));
                    contactPoints.Add(new ContactPoint(relativePosition + obj.transform.position - Vector3.one * 0.001f, transform.position));
                }
            }
        }
    }

    void DrawLightMesh() {
        Vector3[] vertices = new Vector3[contactPoints.Count + 1];
        Vector2[] uv = new Vector2[contactPoints.Count + 1];
        int[] triangles = new int[3 * (contactPoints.Count)];

        vertices[0] = transform.position;
        uv[0] = new Vector2(0.5f, 0.5f);
        for (int i = 1; i < vertices.Length; i++) {
            vertices[i] = new Vector2(contactPoints[i - 1].point.x, contactPoints[i - 1].point.y);
            uv[i] = contactPoints[i - 1].distanceFromCenter.normalized * (Vector2.Distance(contactPoints[i - 1].point, transform.position) / radius) + (new Vector3(0.5f, 0.5f));
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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        lightGameObject = new GameObject("shadow", typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D));
        lightGameObject.transform.SetParent(gameObject.transform);
        lightGameObject.layer = 9;
        lightGameObject.GetComponent<MeshFilter>().mesh = mesh;
        lightGameObject.GetComponent<MeshRenderer>().material = mat;
        lightGameObject.GetComponent<MeshRenderer>().sortingLayerName = "light";

        Vector2[] points = new Vector2[contactPoints.Count];
        for (int i = 0; i < contactPoints.Count; i++)
            points[i] = contactPoints[i].point;
        lightGameObject.GetComponent<PolygonCollider2D>().points = points;
        lightGameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    class ContactPoint {
        public Vector3 point;
        public Vector3 distanceFromCenter;

        public ContactPoint(Vector2 point, Vector2 origin) {
            this.point = point;
            distanceFromCenter = point - origin; //new Vector2(point.x - origin.x, point.y - origin.y)
        }
    }

    class SortClockwise : Comparer<ContactPoint> {
        public Vector2
        
        public override int Compare(ContactPoint x, ContactPoint y) {
            if (x.distanceFromCenter.x >= 0 && y.distanceFromCenter.x < 0)
                return 1;
            if (x.distanceFromCenter.x < 0 && y.distanceFromCenter.x >= 0)
                return -1;
            if (x.distanceFromCenter.x == 0 && y.distanceFromCenter.x == 0) {
                if (x.distanceFromCenter.y >= 0 || y.distanceFromCenter.y >= 0)
                    return x.point.y.CompareTo(y.point.y);
                return y.point.y.CompareTo(x.point.y);
            }

            float det = (x.distanceFromCenter.x) * (y.distanceFromCenter.y) - (y.distanceFromCenter.x) * (x.distanceFromCenter.y);
            if (det < 0)
                return 1;
            if (det > 0)
                return -1;

            float d1 = (x.distanceFromCenter.x) * (x.distanceFromCenter.x) + (x.distanceFromCenter.y) * (x.distanceFromCenter.y);
            float d2 = (y.distanceFromCenter.x) * (y.distanceFromCenter.x) + (y.distanceFromCenter.y) * (y.distanceFromCenter.y);
            return d1.CompareTo(d2);
        }

        public int CompareRelative() {

        }

    }

}
