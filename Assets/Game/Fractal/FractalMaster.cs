using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class FractalMaster : MonoBehaviour
{

    public ComputeShader fractalShader;
    public RenderTexture rt;

    [Range(1, 20)]
    public float fractalPower = 10f;
    public float darkness = 70f;
    public float drawDistance = 2f;

    [Header("Colour mixing")]
    [Range(0, 1)] public float alpha;
    [Range(0, 1)] public float blackAndWhite;
    [Range(0, 1)] public float redA;
    [Range(0, 1)] public float greenA;
    [Range(0, 1)] public float blueA = 1;
    [Range(0, 1)] public float redB = 1;
    [Range(0, 1)] public float greenB;
    [Range(0, 1)] public float blueB;
    [Range(-1, 1)] public float angle;
    public Vector3 positionOffset;

    RenderTexture target;
    Camera cam;
    Light directionalLight;

    [Header("Animation Settings")]
    public float powerIncreaseRate = 0.2f;
    public float oscillationRate = 0.2f;
    public float oscillationRange = 5f;
    private float t = 0;

    private int handleCSMain;

    public float[] groupMinData;
    public int groupMin;

    private ComputeBuffer groupMinBuffer;

    private int threadGroupsX;
    private int threadGroupsY;

    public float minDist;

    private int maxStepCount = 250;

    public int maxIterations;

    public bool LODChangeWithDist;

    Matrix4x4 cameraToWorldMatrix;
    Matrix4x4 projectionMatrixInverse;


    void Start()
    {
        Application.targetFrameRate = 60;

        if (null == fractalShader)
        {
            Debug.Log("Shader missing.");
            return;
        }
    }

    void Init()
    {
        cam = Camera.main;

        cameraToWorldMatrix = cam.cameraToWorldMatrix;
        projectionMatrixInverse = cam.projectionMatrix.inverse;

        directionalLight = FindObjectOfType<Light>();

        threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 64.0f);     //CREATING A THREAD FOR EACH PIXEL (/8 AS IT'S *8 IN THE SHADER)
        threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 1.0f);
    }

    void InitBuffer()
    {
        groupMinBuffer = new ComputeBuffer(threadGroupsX, (sizeof(uint) * 2) + (sizeof(float) * 1));
        groupMinData = new float[threadGroupsX * 3];
    }

    // Animate properties
    void Update()
    {
        if (Application.isPlaying)
        {
            if (powerIncreaseRate != 0)
            {
                fractalPower += powerIncreaseRate * Time.deltaTime;
            }

            else if (oscillationRate != 0)
            {
                t = (t + (Time.deltaTime * oscillationRate)) % (2 * Mathf.PI);

                fractalPower = 1f + oscillationRange * (1f + (Mathf.Cos(t + Mathf.PI)));
            }

            fractalPower = Mathf.Lerp(2.5f, 9f, (Mathf.Sin(Time.time * .25f) + 1) / 2);

            redA = Mathf.Cos((Time.time +0f) *0.3f)/2 + .5f;
            greenA = Mathf.Sin((Time.time - 0.6f) * 0.3f) /2 + .5f;
            blueA = Mathf.Sin((Time.time + .9f) * 0.3f) /2 + .5f;
            redB = Mathf.Cos((Time.time + .8f) * 0.3f) /2 + .5f;
            greenB = Mathf.Cos((Time.time - 0f) * 0.3f) /2 + .5f;
            blueB = Mathf.Sin((Time.time+ .0f) * 0.3f) /2 + .5f;

        }
        UpdateTexture();

    }

    void UpdateTexture()
    {
        handleCSMain = fractalShader.FindKernel("CSMain");
        Init();
        InitRenderTexture();

        InitBuffer();

        SetParameters();

        fractalShader.Dispatch(handleCSMain, threadGroupsX, threadGroupsY, 1);

        // get minima of groups
        groupMinBuffer.GetData(groupMinData);

        // find minimum of all groups
        groupMin = 0;
        for (int group = 1; group < threadGroupsX; group++)
        {
            if (groupMinData[3 * group + 2] < groupMinData[3 * groupMin + 2])
            {
                groupMin = group;
            }
        }

        // At the end, the relative luminance of the brightest pixel is at groupMinData[3 * groupMin + 2].
        // Its x coordinate is at groupMinData[3 * groupMin + 0] and 
        // its y coordinate is at groupMinData[3 * groupMin + 1]
        minDist = groupMinData[3 * groupMin + 2];

        //Graphics.Blit(target, destination);
        Graphics.Blit(target, rt);

        OnDestroy();
    }

    void SetParameters()
    {
        //source.enableRandomWrite = true;
        //RenderTexture src = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB2101010);
        //src.enableRandomWrite = true;
        //src.Create();

        //Graphics.Blit(source, src);
        //Graphics.CopyTexture(source, src);

        fractalShader.SetTexture(0, "Destination", target);
        //fractalShader.SetTexture(0, "Source", src);
        fractalShader.SetFloat("alpha", alpha);
        fractalShader.SetFloat("power", Mathf.Max(fractalPower, 1.01f));
        fractalShader.SetFloat("darkness", darkness);
        fractalShader.SetFloat("blackAndWhite", blackAndWhite);
        fractalShader.SetFloat("maxDst", drawDistance);
        fractalShader.SetVector("colourAMix", new Vector3(redA, greenA, blueA));
        fractalShader.SetVector("colourBMix", new Vector3(redB, greenB, blueB));
        fractalShader.SetFloat("angle", angle);
        fractalShader.SetVector("positionOffset", positionOffset);

        fractalShader.SetInt("maxStepCount", maxStepCount);

        if (LODChangeWithDist)
        {
            maxIterations = Mathf.FloorToInt(5f / minDist);
        }
        else maxIterations = 15;

        fractalShader.SetInt("maxIterations", maxIterations);

        fractalShader.SetMatrix("_CameraToWorld", cameraToWorldMatrix);
        fractalShader.SetMatrix("_CameraInverseProjection", projectionMatrixInverse);
        fractalShader.SetVector("_LightDirection", directionalLight.transform.forward);

        fractalShader.SetBuffer(handleCSMain, "GroupMinBuffer", groupMinBuffer);
    }

    void InitRenderTexture()
    {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }

            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    void OnDestroy()
    {
        if (null != groupMinBuffer)
        {
            groupMinBuffer.Release();
        }
    }
}

