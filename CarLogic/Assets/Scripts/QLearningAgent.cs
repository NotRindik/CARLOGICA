using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class QTableData
{
    public Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
}

public class QLearningAgent : MonoBehaviour
{
    public float learningRate = 0.1f; // �������� ��������
    public float discountFactor = 0.9f; // ����������� ���������������
    public int numActions = 6; // ��������� ���������� �������� ��� ����� ������� ����������
    public float epsilon = 1.0f; // ��������� ����������� ���������� ��������
    public float epsilonDecay = 0.995f; // �������� ���������� 
    public float epsilonMin = 0.1f; // ����������� �������� 

    private QTableData qTableData = new QTableData();
    public bool save;

    private void Start()
    {
        Debug.Log(Application.dataPath + "/Json" + "/qTable.json");
        InitializeQTable();
        LoadQTable();
    }

    private void InitializeQTable()
    {
        for (int speed = 0; speed <= 10; speed++) // ������ ������������� ��������
        {
            for (int angle = -90; angle <= 90; angle += 10) // ������ ������������� ���� ��������
            {
                string stateKey = $"{speed},{angle}";
                qTableData.qTable[stateKey] = new float[numActions];
            }
        }
    }

    public void SaveQTable()
    {
        string json = JsonConvert.SerializeObject(qTableData, Newtonsoft.Json.Formatting.Indented);
        string path = Path.Combine(Application.dataPath, "Json", "qTable.json");

        // ���������, ��� ���������� ����������
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, json);
    }

    public void LoadQTable()
    {
        string path = Path.Combine(Application.dataPath, "Json", "qTable.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            qTableData = JsonConvert.DeserializeObject<QTableData>(json);
        }
    }

    public void UpdateQTable(string state, int action, float reward, string nextState)
    {
        InitializeStateIfNotExists(state);
        InitializeStateIfNotExists(nextState);

        float oldValue = qTableData.qTable[state][action];
        float maxFutureValue = Mathf.Max(qTableData.qTable[nextState]);


        qTableData.qTable[state][action] = oldValue + learningRate * (reward + discountFactor * maxFutureValue - oldValue);

        Debug.Log($"State: {state}, Action: {action}, Q-value: {qTableData.qTable[state][action]}");

        if (save)
        {
            SaveQTable();
            save = false;
        }
    }

    public int SelectAction(string state)
    {
        InitializeStateIfNotExists(state);

        if (epsilon > epsilonMin)
        {
            epsilon *= epsilonDecay;
        }

        if (Random.value < epsilon)
        {
            return Random.Range(0, numActions);
        }
        else
        {
            float[] qValues = qTableData.qTable[state];
            return System.Array.IndexOf(qValues, Mathf.Max(qValues));
        }
    }

    private void InitializeStateIfNotExists(string state)
    {
        if (!qTableData.qTable.ContainsKey(state))
        {
            qTableData.qTable[state] = new float[numActions];
        }
    }
}
