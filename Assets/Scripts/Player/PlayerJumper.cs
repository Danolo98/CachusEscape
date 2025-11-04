// Assets/Scripts/Player/PlayerJumper.cs
using System.Collections;
using UnityEngine;
using Cachu.Core;
using Cachu.World;

namespace Cachu.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerJumper : MonoBehaviour
    {
        [Header("Refs")]
        public LandingSensor sensor;
        public Transform model;

        [Header("Motion")]
        public float forwardBaseSpeed = 8f;
        public float horizMaxSpeed = 10f;
        public float minAirTime = 0.35f;
        public float maxAirTime = 0.65f;
        public float gravity = 25f;
        public float rotateLerp = 12f;

        [Header("Timing Window (s)")]
        public float perfectWindow = 0.08f;
        public float goodWindow = 0.16f;

        [Header("Fail")]
        public float fallGravity = 40f;

        // --- Estado interno ---
        private Rigidbody rb;
        private bool waitingInput;
        private float landedTime;
        private BranchTarget lastBranch;
        private bool airborne;
        private Vector3 velocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = true;
        }

        private void Start()
        {
            TryPlaceOnFirstBranch();
        }

        private void Update()
        {
            if (GameFlow.I == null || GameFlow.I.state != GameState.Playing) return;

            if (sensor != null && sensor.IsGrounded && !airborne)
            {
                if (!waitingInput)
                {
                    waitingInput = true;
                    landedTime = Time.time;
                    lastBranch = sensor.currentBranch;
                }

                if (InputReader.I != null && InputReader.I.Pressed)
                {
                    float dt = Time.time - landedTime;
                    Debug.Log("🟢 Espacio detectado por PlayerJumper");

                    if (dt <= perfectWindow)
                    {
                        JumpTo(lastBranch != null ? lastBranch.next : null);
                    }
                    else if (dt <= goodWindow)
                    {
                        JumpTo(lastBranch != null ? lastBranch.next : null);
                    }
                    else
                    {
                        TriggerFall();
                    }

                    waitingInput = false;
                }

                if (waitingInput && Time.time - landedTime > goodWindow)
                {
                    waitingInput = false;
                    TriggerFall();
                }
            }

            // movimiento manual en el aire
            if (airborne)
            {
                velocity += Vector3.down * gravity * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;

                if (model && velocity.sqrMagnitude > 0.0001f)
                {
                    var dir = new Vector3(velocity.x, 0f, Mathf.Max(0.01f, velocity.z));
                    var targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotateLerp * Time.deltaTime);
                }
            }
        }

        private void TryPlaceOnFirstBranch()
        {
            if (sensor != null && sensor.IsGrounded)
            {
                transform.position = sensor.currentBranch.LandPosition;
                lastBranch = sensor.currentBranch;
                waitingInput = true;
                landedTime = Time.time;
            }
        }

        private void JumpTo(BranchTarget target)
        {
            if (target == null)
            {
                Debug.LogWarning("❌ JumpTo falló: target nulo");
                TriggerFall();
                return;
            }

            airborne = true;
            waitingInput = false;

            Vector3 start = transform.position;
            Vector3 end = target.LandPosition;
            Vector3 disp = end - start;
            Vector3 planar = new Vector3(disp.x, 0f, disp.z);
            float horizDist = Mathf.Max(0.1f, planar.magnitude);
            float t = Mathf.Clamp(horizDist / horizMaxSpeed, minAirTime, maxAirTime);

            // Fórmula de velocidad inicial
            Vector3 vel = new Vector3(
                disp.x / t,
                (disp.y + 0.5f * gravity * t * t) / t,
                disp.z / t
            );

            Debug.Log($"🌿 JumpTo {target.name} | start={start} end={end} disp={disp} horizDist={horizDist:F2} t={t:F2} vel={vel}");

            velocity = vel;
            lastBranch = target;

            // --- TEST TEMPORAL ---
            // fuerza un salto visible si el cálculo da valores bajos
            if (velocity.magnitude < 0.1f)
            {
                velocity = new Vector3(0, 10f, 10f);
                Debug.Log("🚀 Salto forzado (debug)");
            }

            StartCoroutine(WaitForLanding(target));
        }

        private IEnumerator WaitForLanding(BranchTarget target)
        {
            float maxT = 2f;
            float t = 0f;
            var goal = target.LandPosition;

            while (t < maxT)
            {
                t += Time.deltaTime;
                if (Vector3.Distance(transform.position, goal) < 0.35f && velocity.y <= 0f)
                    break;
                yield return null;
            }

            transform.position = goal;
            airborne = false;
            velocity = Vector3.zero;
        }

        private void TriggerFall()
        {
            if (airborne) return;

            airborne = true;
            waitingInput = false;
            if (GameFlow.I != null) GameFlow.I.Miss();

            velocity = Vector3.down * 0.1f;
            gravity = fallGravity;
            StartCoroutine(RespawnAfterFall());
        }

        private IEnumerator RespawnAfterFall()
        {
            yield return new WaitForSecondsRealtime(0.6f);
            if (GameFlow.I != null && GameFlow.I.state == GameState.Playing)
                GameFlow.I.Restart();
        }
    }
}
