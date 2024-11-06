using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountingEnemy : MonoBehaviour
{
    int EnenmyAmount;
    int totalEnenmyAmount;
    [SerializeField] Text enemyCount;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnenmyAmount = enemies.Length;
        enemyCount = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnenmyAmount = enemies.Length;
        enemyCount.text = $"Remaining Enemy: {EnenmyAmount}/{totalEnenmyAmount}";
    }
}
