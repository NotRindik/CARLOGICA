using System.Collections.Generic;
using UnityEngine;

public class Car_Control_AI : MonoBehaviour
{
    private QLearningAgent agent;
    private Car car;

    public LayerMask DieGround;
    public LayerMask AwardGround;
    public Transform[] rayPoses = new Transform[6];
    public Vector3 GroundCheckerSize;
    public Transform StartPos;

    public bool isGround;
    public float reward;
    public bool checkPointCompleted;
    public int checkPointCountCompleted;
    public bool[] sensors;

    public CheackPoint[] checkPoints;
    public float epsilon = 1.0f; // Вероятность случайного действия
    public float epsilonDecay = 0.995f; // Уменьшение ε
    public float epsilonMin = 0.1f; // Минимальная вероятность случайного действия

    public Vector3 lastPosition;

    public bool IsUpdateQTableByFps;

    private void Start()
    {
        agent = GetComponent<QLearningAgent>();
        car = GetComponent<Car>();
        transform.position = StartPos.position;
        transform.eulerAngles = StartPos.eulerAngles;
        lastPosition = car.transform.position;
    }

    private void Update()
    {
        reward = GetReward();
        string currentState = GetCurrentState();
        int action = agent.SelectAction(currentState);
        PerformAction(action);
        if(IsUpdateQTableByFps) 
            UpdateQTable(reward);

        // Уменьшение ε для уменьшения случайности по мере обучения
        if (epsilon > epsilonMin)
        {
            epsilon *= epsilonDecay;
        }
    }

    private string GetCurrentState()
    {
        sensors = new bool[6];

        for (int i = 0; i < sensors.Length; i++)
        {
            // Использование Physics.Raycast для определения препятствий
            RaycastHit hit;
            sensors[i] = Physics.Raycast(rayPoses[i].position, rayPoses[i].forward, out hit, 2000, AwardGround);
        }

        // Дискретизация скорости для упрощения состояния
        int speedLevel = Mathf.FloorToInt(car.speedKmh / 10.0f);
        float carAngle = car.transform.eulerAngles.y;
        return $"{speedLevel},{carAngle},{sensors[0]},{sensors[1]},{sensors[2]},{sensors[3]},{sensors[4]},{sensors[5]}";
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, GroundCheckerSize);
        if(Application.isEditor)
            for (int i = 0;i < rayPoses.Length;i++) 
                Gizmos.DrawRay(rayPoses[i].position, rayPoses[i].forward);
    }

    private float GetReward()
    {
        isGround = true;
        bool noRoad = true;

        foreach (Collider touch in Physics.OverlapBox(transform.position, GroundCheckerSize, Quaternion.identity, AwardGround))
        {
            noRoad = false;
        }

        // Штраф за остановку или выезд с трассы
        if (noRoad || car.speedKmh < 0.5f)
        {
            isGround = false;
            UpdateQTable(-5);
            transform.position = StartPos.position;
            transform.eulerAngles = StartPos.eulerAngles;
            lastPosition = StartPos.position;
            checkPointCountCompleted = 0;
            foreach (var item in checkPoints)
            {
                item.cheacked.Remove(this);
            }
            car.rigidbody.velocity = Vector3.one;
            return -5; // Увеличенный штраф за ошибку
        }
        for (int i = 0; i < sensors.Length; i++)
        {
            if (!sensors[i])
            {
                return -0.2f;
            }
        }

        // Награда за успешное прохождение чекпоинта
        if (checkPointCompleted)
        {
            checkPointCountCompleted++;
            checkPointCompleted = false;
            UpdateQTable(1 + checkPointCountCompleted);
            return 1+ checkPointCountCompleted; // Увеличенная награда за прогресс
        }
        //if (!IsMovingForward())
        //{
        //    return -0.1f; // Штраф за движение назад
        //}

        if (car.speedKmh > 10.0f) // Награда за поддержание скорости
        {
            return 0.5f;
        }

        // Маленькая награда за движение вперёд
        return 0;
    }

    private void UpdateQTable(float reward)
    {
        string currentState = GetCurrentState();
        string nextState = GetCurrentState();
        int action = agent.SelectAction(currentState);
        agent.UpdateQTable(currentState, action, reward, nextState);
    }

    private bool IsMovingForward()
    {
        // Вычисляем вектор скорости машины (ее текущее движение в пространстве)
        Vector3 velocity = (car.transform.position - lastPosition).normalized;

        // Сравниваем направление машины (transform.forward) с направлением её движения (velocity)
        bool movingForward = Vector3.Dot(car.transform.forward, velocity) > 0;

        // Обновляем последнее положение
        lastPosition = car.transform.position;

        return movingForward;
    }

    private void PerformAction(int action)
    {
        switch (action)
        {
            case 0:
                car.forward = 0.5f; // Вперёд
                break;
            case 1:
                car.forward = 0; // Остановка
                break;
            case 2:
                car.turn = 0; // Прямо
                break;
            case 3:
                car.turn = -0.1f; // Остановка
                break;
            case 4:
                car.turn = 0.1f; // Остановка
                break;
        }
    }
}
