using System;
using UnityEngine;

public class Policeman : AIResident
{
    private Transform criminalTarget;
    private float chasingRecalculationCooldown = 3f;

    private int pathIndex;

    protected override void Awake()
    {
        Debug.Log($"Policeman Awake called on {gameObject.name}");
        base.Awake();
        Debug.Log($"Policeman Awake finished on {gameObject.name}, movementController is {(movementController == null ? "NULL" : "initialized")}");
    }

    protected override void Start()
    {
        Debug.Log($"Policeman Start called on {gameObject.name}");
        base.Start();

        if (movementController == null)
        {
            Debug.LogWarning($"Movement controller still null after Start in {gameObject.name}, reinitializing");
            InitializeComponents();
        }

        Debug.Log($"Policeman Start finished on {gameObject.name}, movementController is {(movementController == null ? "NULL" : "initialized")}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AIResident resident = collision.GetComponent<AIResident>();
        
        if (resident != null && resident.isCommitingCrime && isChasing)
        {
            CatchCriminal(resident);
        }
    }

    private void CatchCriminal(AIResident criminal)
    {
        Debug.Log($"Policeman {gameObject.name} caught criminal");
        isChasing = false;
        criminalTarget = null;

        criminal.ResetCriminal(true);
        PoliceStationManager.Instance.ResetCriminal();
    }
    protected override void Update()
    {
        if (movementController == null)
        {
            Debug.LogError($"Movement controller is null in {gameObject.name}'s Update");
            return; 
        }

        if (isChasing && criminalTarget != null)
        {          
            chasingRecalculationCooldown -= Time.deltaTime;

            if (chasingRecalculationCooldown <= 0)
            {
                movementController.SetDestination(criminalTarget.position);
                chasingRecalculationCooldown = 1f;
            }
        }

        else if (isChasing)
        {
            isChasing = false;
            movementController.SetChaseSpeed(false);
        }

        movementController.UpdateMovement();
    }

    internal void StartChasingCriminal(Transform target, AIResident criminalResident)
    {
        Debug.Log($"StartChasingCriminal called on {gameObject.name}");

        if (target == null)
        {
            Debug.LogError("Target is null");
            return;
        }

        if (movementController == null)
        {
            Debug.LogError($"Movement Controller is null in {gameObject.name}");

            if (groundTilemap != null && roadTilemap != null && animator != null)
            {
                InitializeComponents();

                if (movementController == null)
                {
                    Debug.LogError("Failed to initialize movement controller");
                    return;
                }
            }
            else
            {
                Debug.LogError("Cannot initialize - missing tilemap or animator references");
                return;
            }
        }

        isChasing = true;
        criminalTarget = target;
        Debug.Log($"Calling set destination for the policeman. Target position: {criminalTarget.transform.position}  Current Position: {transform.position}");

        movementController.SetDestination(criminalTarget.transform.position);
    }
}
