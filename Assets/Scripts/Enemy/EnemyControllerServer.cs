﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerServer : NetworkBehaviour
{
    private const string ENEMY_ID_PREFIX = "Enemy ";

    public bool IsWalking { get; set; }
    private float _currentHealth;
    [SerializeField] private float _maxHealth = 50f;

    public NavMeshAgent Agent { get; set; }

    public Vector3 Dest { get; set; }
    
    private void Start()
    {
        transform.name = ENEMY_ID_PREFIX + GameManager.EnemyIdCounter++;
        if (isServer)
        {
            if (!GameManager.Enemies.ContainsKey(transform.name)) GameManager.Enemies.Add(transform.name, this);
            Agent = GetComponent<NavMeshAgent>();
            StartCoroutine(SetClosestPlayerStart());
            IsWalking = true;
            _currentHealth = _maxHealth;
        }
        else enabled = false;
    }

    private void Update()
    {
        if (Dest != null && IsWalking) Agent.SetDestination(Dest);
        else SetClosestPlayer();
    }

    private IEnumerator SetClosestPlayerStart()
    {
        yield return new WaitForSeconds(1f);

        List<Transform> players = new List<Transform>();
        
        foreach (PlayerManager player in GameManager.ActivePlayers.Values)
            players.Add(player.transform);
        
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in players)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        Dest =  tMin.position;
        RpcSendDest(Dest);
    }

    public void SetClosestPlayer()
    {
        List<Transform> players = new List<Transform>();

        foreach (PlayerManager player in GameManager.ActivePlayers.Values)
            players.Add(player.transform);

        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in players)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        Dest = tMin.position;
        RpcSendDest(Dest);
    }


    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            GameManager.Enemies.Remove(transform.name);
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    void RpcSendDest(Vector3 dest)
    {
        
        if (!isServer)
        {
            EnemyControllerClient enemyControllerClient = GetComponent<EnemyControllerClient>();
            enemyControllerClient.Dest = dest;
        }
    }
}