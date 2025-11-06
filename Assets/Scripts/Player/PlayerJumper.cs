// Assets/Scripts/Player/PlayerJumper.cs
using System.Collections;
using UnityEngine;
using Cachu.Core;
using Cachu.World;
using Cachu.Game; // 👈 Importante: añadimos el namespace del ScoreManager

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

        private Rigidbody rb;
        private bool waitingInput;
        private float landedTime;
        private BranchTarget lastBranch;
        private bool airborne;
        private Vector3 velocity;
        private bool graceActive = false;
        private static bool pendingGrace = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = true;
        }

        private void Start()
        {
            TryPlaceOnFirstBranch();

            if (pendingGrace)
            {
                pendingGrace = false;
                graceActive = true;
                waitingInput = true;
                landedTime = Time.time;
                Debug.Log("🌿 Tiempo de gracia ACTIVADO tras respawn (persistente)");
            }
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

                if (graceActive)
                {
                    if (InputReader.I != null && InputReader.I.Pressed)
                    {
                        AudioTac.I?.Tac(1f, 1f);
                        graceActive = false;
                        Debug.Log("✨ Primer salto tras respawn — se reactivan tiempos normales");
                        JumpTo(ResolveNextTarget());
                        waitingInput = false;
                    }
                    return;
                }

                if (InputReader.I != null && InputReader.I.Pressed && Time.time > landedTime)
                {
                    float dt = Time.time - landedTime;

                    if (dt < 0.03f)
                    {
                        Debug.Log("⛔ Presionó demasiado pronto, se castiga");
                        waitingInput = false;
                        TriggerFall();
                        return;
                    }

                    if (dt <= perfectWindow)
                    {
                        AudioTac.I?.Tac(1f, 1.15f);
                        JumpTo(ResolveNextTarget());
                    }
                    else if (dt <= goodWindow)
                    {
                        AudioTac.I?.Tac(1f, 1f);
                        JumpTo(ResolveNextTarget());
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

        private BranchTarget ResolveNextTarget()
        {
            if (lastBranch != null && lastBranch.next != null)
                return lastBranch.next;

            if (sensor != null && sensor.currentBranch != null && sensor.currentBranch.next != null)
                return sensor.currentBranch.next;

            BranchTarget best = null;
            float bestDz = float.PositiveInfinity;
            float zNow = transform.position.z + 0.05f;
            var all = GameObject.FindObjectsOfType<BranchTarget>();

            foreach (var b in all)
            {
                if (!b) continue;
                float dz = b.transform.position.z - zNow;
                if (dz > 0f && dz < bestDz)
                {
                    best = b;
                    bestDz = dz;
                }
            }

            if (best != null)
                Debug.Log($"🔍 Rama encontrada automáticamente: {best.name}");
            else
                Debug.LogWarning("⚠️ No se encontró rama válida por delante");

            return best;
        }

        private void JumpTo(BranchTarget target)
        {
            if (target == null)
                target = ResolveNextTarget();

            if (target == null)
            {
                Debug.LogWarning("❌ JumpTo falló: target nulo (tras resolver)");
                TriggerFall();
                return;
            }

            airborne = true;
            waitingInput = false;

            Vector3 start = transform.position;
            Vector3 end = target.LandPosition;

            float tTotal = Mathf.Lerp(minAirTime, maxAirTime, 0.5f);
            float jumpArcHeight = 1f;

            StopAllCoroutines();
            StartCoroutine(JumpArcCoroutine(start, end, tTotal, jumpArcHeight));

            lastBranch = target;
            AudioTac.I?.Tac(1f, 1.05f);

            Debug.Log($"🌿 JumpTo (forzado) de {start} → {end}");
        }

        private IEnumerator JumpArcCoroutine(Vector3 start, Vector3 end, float duration, float height)
        {
            float time = 0f;
            airborne = true;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float yOffset = 4 * height * t * (1 - t);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y = Mathf.Lerp(start.y, end.y, t) + yOffset;
                transform.position = pos;

                yield return null;
            }

            transform.position = end;
            airborne = false;
            velocity = Vector3.zero;

            // ✅ Aquí registramos el salto exitoso:
            var score = FindObjectOfType<ScoreManager>();
            if (score != null)
            {
                score.AddJump();
                Debug.Log("✅ Salto registrado por ScoreManager");
            }
        }

        private void TriggerFall()
        {
            if (airborne) return;

            airborne = true;
            waitingInput = false;
            if (GameFlow.I != null) GameFlow.I.Miss();

            pendingGrace = true;
            AudioTac.I?.Tac(0.8f, 0.7f);

            velocity = Vector3.down * 0.1f;
            gravity = fallGravity;
            StartCoroutine(RespawnAfterFall());
        }

        private IEnumerator RespawnAfterFall()
        {
            // Fade con DOTween
            if (Cachu.UI.ScreenFader.I != null)
            {
                Cachu.UI.ScreenFader.I.FadeOutThenIn(() =>
                {
                    if (GameFlow.I != null && GameFlow.I.state == GameState.Playing)
                        GameFlow.I.Restart();
                });
            }
            else
            {
                // Si no hay ScreenFader, usa el comportamiento normal
                yield return new WaitForSecondsRealtime(0.6f);
                if (GameFlow.I != null && GameFlow.I.state == GameState.Playing)
                    GameFlow.I.Restart();
            }

            yield break;
        }

    }
}
