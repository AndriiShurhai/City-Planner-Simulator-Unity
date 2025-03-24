using UnityEngine;

public class Doctor : AIResident
{
    private Transform illCitizenTarget;
    private bool isTryingToCure = false;
    protected override void Awake()
    {
        Debug.Log($"Doctor Awake called on {gameObject.name}");
        base.Awake();
        Debug.Log($"Doctor Awake finished on {gameObject.name}, movementController is {(movementController == null ? "NULL" : "initialized")}");
    }

    protected override void Start()
    {
        Debug.Log($"Doctor Start called on {gameObject.name}");
        base.Start();

        if (movementController == null)
        {
            Debug.LogWarning($"Movement controller still null after Start in {gameObject.name}, reinitializing");
            InitializeComponents();
        }

        Debug.Log($"Doctor Start finished on {gameObject.name}, movementController is {(movementController == null ? "NULL" : "initialized")}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AIResident resident = collision.GetComponent<AIResident>();

        if (resident != null && resident.isHavingHeartAttack && isTryingToCure)
        {
            CureCitizen(resident);
        }
    }

    private void CureCitizen(AIResident illResident)
    {
        Debug.Log($"Doctor {gameObject.name} helped citizen");
        isTryingToCure = false;
        illCitizenTarget = null;

        illResident.ResetHealthAttack(true);
        HospitalsManager.Instance.ResetCurrentILLResident();
        movementController.SetChaseSpeed(false);
    }
    protected override void Update()
    {
        if (movementController == null)
        {
            Debug.LogError($"Movement controller is null in {gameObject.name}'s Update");
            return;
        }

        if (isTryingToCure && illCitizenTarget == null)
        {
            isTryingToCure = false;
            movementController.SetChaseSpeed(false);
        }

        movementController.UpdateMovement();
    }

    internal void GoHealCitizen(Transform target, AIResident criminalResident)
    {
        Debug.Log($"GoHealCitizen called on {gameObject.name}");

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

        isTryingToCure = true;
        illCitizenTarget = target;
        Debug.Log($"Calling set destination for the doctor. Target position: {illCitizenTarget.transform.position}  Current Position: {transform.position}");

        movementController.SetChaseSpeed(true);
        movementController.SetDestination(illCitizenTarget.transform.position);
    }

}
