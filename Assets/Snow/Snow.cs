using UnityEngine;

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snow : MonoBehaviour
{
    #region Field

    // # Unity のドローコールについて
    // 
    // Unity では 1 回のドローコールに付き 65000 頂点まで扱えるようです。
    // 雪を表現するメッシュは 1 つあたり 4 頂点使うので、65,000 / 4 = 1,6250
    // 程度の雪の数を一度に描画することができます。

    /// <summary>
    /// 降らせる雪の数。
    /// </summary>
    const int SNOW_NUM = 16000;

    /// <summary>
    /// 雪のメッシュの頂点。
    /// </summary>
    private Vector3[] snowMeshVertices;

    /// <summary>
    /// 雪のメッシュの三角形。
    /// </summary>
    private int[] snowMeshTriangleIndexes;

    /// <summary>
    /// 雪のメッシュの UV.
    /// </summary>
    private Vector2[] snowMeshUvs;

    /// <summary>
    /// 雪を降らす範囲。
    /// </summary>
    private float range;

    /// <summary>
    /// 雪を降らす範囲の逆数。
    /// </summary>
    private float rangeR;

    /// <summary>
    /// 
    /// </summary>
    private Vector3 move = Vector3.zero;

    private MeshRenderer meshRenderer;

    #endregion Field

    /// <summary>
    /// 開始時に呼び出されます。
    /// </summary>
    protected virtual void Start ()
    {
        this.range    = 16f;
        this.rangeR   = 1.0f / range;
        this.snowMeshVertices = new Vector3[SNOW_NUM * 4];

        #region Generate Vertices

        // 所定の空間にランダムな座標を生成し、頂点情報として保存します。

        for (var i = 0; i < SNOW_NUM; i++)
        {
            float x = Random.Range (-this.range, this.range);
            float y = Random.Range (-this.range, this.range);
            float z = Random.Range (-this.range, this.range);

            Vector3 point = new Vector3(Random.Range(-this.range, this.range),
                                        Random.Range(-this.range, this.range),
                                        Random.Range(-this.range, this.range));

            this.snowMeshVertices [i * 4 + 0] = point;
            this.snowMeshVertices [i * 4 + 1] = point;
            this.snowMeshVertices [i * 4 + 2] = point;
            this.snowMeshVertices [i * 4 + 3] = point;
        }

        #endregion Generate Vertices

        #region Generate Indexes

        this.snowMeshTriangleIndexes = new int[SNOW_NUM * 6];

        for (int i = 0; i < SNOW_NUM; i++)
        {
            this.snowMeshTriangleIndexes[i * 6 + 0] = i * 4 + 0;
            this.snowMeshTriangleIndexes[i * 6 + 1] = i * 4 + 1;
            this.snowMeshTriangleIndexes[i * 6 + 2] = i * 4 + 2;

            this.snowMeshTriangleIndexes[i * 6 + 3] = i * 4 + 2;
            this.snowMeshTriangleIndexes[i * 6 + 4] = i * 4 + 1;
            this.snowMeshTriangleIndexes[i * 6 + 5] = i * 4 + 3;
        }

        #endregion Generate Indexes

        #region Generate UVs

        this.snowMeshUvs = new Vector2[SNOW_NUM * 4];

        for (var i = 0; i < SNOW_NUM; i++)
        {
            snowMeshUvs [i * 4 + 0] = new Vector2 (0f, 0f);
            snowMeshUvs [i * 4 + 1] = new Vector2 (1f, 0f);
            snowMeshUvs [i * 4 + 2] = new Vector2 (0f, 1f);
            snowMeshUvs [i * 4 + 3] = new Vector2 (1f, 1f);
        }

        #endregion Generate UVs

        // # MeshBounds について
        // 
        // MeshBounds は対象のメッシュが描画される矩形領域です。
        // この矩形領域がカメラの外にあるとき、メッシュは描画されなくなります。

        Mesh mesh      = new Mesh();
        mesh.name      = "Snow Frake Meshes";
        mesh.vertices  = this.snowMeshVertices;
        mesh.triangles = this.snowMeshTriangleIndexes;
        mesh.uv        = this.snowMeshUvs;
        mesh.bounds    = new Bounds(Vector3.zero, Vector3.one * 99999999);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        this.meshRenderer = GetComponent<MeshRenderer>();
    }
    
    /// <summary>
    /// 更新の最後に呼び出されます。
    /// </summary>
    protected virtual void LateUpdate ()
    {
        // 常にカメラの前方に来るようにします。

        Vector3 target_position = Camera.main.transform.TransformPoint(Vector3.forward * this.range);

        this.meshRenderer.material.SetFloat("_Range",  this.range);
        this.meshRenderer.material.SetFloat("_RangeR", this.rangeR);
        this.meshRenderer.material.SetFloat("_Size",   0.1f);

        this.meshRenderer.material.SetVector("_MoveTotal",      this.move);
        this.meshRenderer.material.SetVector("_CamUp",          Camera.main.transform.up);
        this.meshRenderer.material.SetVector("_TargetPosition", target_position);

        float x = (Mathf.PerlinNoise(0f, Time.time * 0.1f) - 0.5f) * 10f;
        float y = -2f;
        float z = (Mathf.PerlinNoise(Time.time * 0.1f, 0f) - 0.5f) * 10f;

        move += new Vector3(x, y, z) * Time.deltaTime;
        move.x = Mathf.Repeat(move.x, range * 2f);
        move.y = Mathf.Repeat(move.y, range * 2f);
        move.z = Mathf.Repeat(move.z, range * 2f);
    }
}