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
        public float forwardBaseSpeed = 8f; // no se usa aún, reservado
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
            rb.isKinematic = true; // usamos integración manual para ser deterministas
        }

        private void Start()
        {
            TryPlaceOnFirstBranch();
        }

        private void Update()
        {
            if (GameFlow.I == null || GameFlow.I.state != GameState.Playing) return;

            // En suelo: abrir ventana y leer input
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
                    Debug.Log("Presion detectada por PlayerJumper");


                    if (dt <= perfectWindow)
                    {
                        JumpTo(lastBranch != null ? lastBranch.next : null);
                        if (AudioTac.I != null) AudioTac.I.Tac(1f, 1.06f);
                        if (GameFlow.I != null) GameFlow.I.AddScore(1);
                    }
                    else if (dt <= goodWindow)
                    {
                        JumpTo(lastBranch != null ? lastBranch.next : null);
                        if (AudioTac.I != null) AudioTac.I.Tac(0.9f, 1f);
                        if (GameFlow.I != null) GameFlow.I.AddScore(1);
                    }
                    else
                    {
                        TriggerFall();
                    }

                    waitingInput = false;
                }

                // si se pasó la ventana sin presionar → caída
                if (waitingInput && Time.time - landedTime > goodWindow)
                {
                    waitingInput = false;
                    TriggerFall();
                }
            }

            // Integración manual del salto
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
            else
            {
                // En suelo, orientar hacia adelante suave (opcional)
                if (model)
                {
                    var targetRot = Quaternion.LookRotation(Vector3.forward, Vector3.up);
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
                TriggerFall();
                return;
            }

            airborne = true;
            waitingInput = false;

            Vector3 start = transform.position;
            Vector3 end = target.LandPosition;
            Vector3 planar = new Vector3(end.x - start.x, 0f, end.z - start.z);
            float horizDist = Mathf.Max(0.1f, planar.magnitude);

            float t = Mathf.Clamp(horizDist / horizMaxSpeed, minAirTime, maxAirTime);

            // v = (d - 0.5 g t^2) / t
            Vector3 disp = end - start;
            Vector3 vel = new Vector3(
                disp.x / t,
                (disp.y + 0.5f * gravity * t * t) / t,
                disp.z / t
            );

            velocity = vel;
            lastBranch = target;

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
                // cerca y descendiendo
                if (Vector3.Distance(transform.position, goal) < 0.35f && velocity.y <= 0f)
                    break;

                yield return null;
            }

            // Snap suave al punto de aterrizaje
            transform.position = goal;
            airborne = false;
            velocity = Vector3.zero;
        }

        private void TriggerFall()
        {
            if (airborne) return; // ya en el aire, no forzar

            airborne = true;
            waitingInput = false;

            if (GameFlow.I != null) GameFlow.I.Miss();

            // Caída vertical
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
