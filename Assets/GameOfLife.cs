using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public ComputeShader computeShader;
    private const int ThreadBlockSize = 8;
    private ComputeBuffer Result,State;
    private int cubeNum;
    public int width = 100;
    public int height = 100;
    private GameObject[] cubes;
    private float[] angles;
    private int[] actives;
    private int[] stateActives;
    private int initKernel, updateKernel,drawKernel;
    // Start is called before the first frame update
    void Start()
    {
        cubeNum = width * height;
        cubes = new GameObject[cubeNum];
        angles = new float[cubeNum];
        actives = new int[cubeNum];
        stateActives = new int[cubeNum];
        for (int i = 0; i < height;i++){
            for (int j = 0; j < height; j++){
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localPosition = new Vector3(j,i,1f);
                cubes[i * width + j] = cube;
            }
        }
        initKernel = computeShader.FindKernel("Init");
        Result = new ComputeBuffer(cubeNum,sizeof(int));
        State = new ComputeBuffer(cubeNum, sizeof(int));
        computeShader.SetBuffer(initKernel, "State", State);
        computeShader.SetInt("_Width", width);
        computeShader.SetInt("_Height", height);
        Result.SetData(actives);
        State.SetData(stateActives);
        int threadGroupX = (cubeNum / ThreadBlockSize) + 1;
        int threadGroupY = (height / ThreadBlockSize) + 1;
        computeShader.Dispatch(initKernel, threadGroupX, threadGroupY, 1);
    }

    // Update is called once per frame
    void Update()
    {
        // GPU並列処理実行

        updateKernel = computeShader.FindKernel("Update");
        drawKernel = computeShader.FindKernel("Draw");
        int threadGroupX = (width / ThreadBlockSize) + 1;
        int threadGroupY = (height / ThreadBlockSize) + 1;


        computeShader.SetInt("_Width", width);
        computeShader.SetInt("_Height", height);

        computeShader.SetBuffer(updateKernel, "State", State);
        computeShader.Dispatch(updateKernel, threadGroupX, threadGroupY, 1);

        computeShader.SetBuffer(drawKernel, "State", State);
        computeShader.SetBuffer(drawKernel, "Result", Result);
        computeShader.Dispatch(drawKernel,threadGroupX, threadGroupY, 1);

        var data = new int[cubeNum];
        // 更新結果を取得
        Result.GetData(data);

        for (int i = 0; i < cubeNum; i++)
        {
            int result = data[i];
            actives[i] = result;
            // キューブをぐるぐるさせる
            cubes[i].SetActive(actives[i] != 0);
        }
    }
    private void OnDestroy()
    {
        Result.Release();
    }
}
