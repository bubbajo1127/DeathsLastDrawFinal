using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hovl
{
    // Optional impact backend. Kept in this file so HS_Laser compiles on its own.
    internal static class HS_LaserImpactBackend
    {
        internal delegate bool SpawnEffect(GameObject prefab, GameObject target, Vector3 point, Vector3 normal,
            float interval, float lifetime, int capacity, bool follow, bool ensureEveryTarget);
        internal static SpawnEffect Spawn;
        internal static System.Action<GameObject> Release;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Spawn = null; Release = null; }
    }

    // Optional integration point. This file has no dependency on the demo enemy scripts.
    public interface IHS_LaserDamageable
    {
        void TakeDamage(float damage);
    }

    [System.Serializable]
    public class HS_LaserHitEvent : UnityEvent<GameObject, float> { }

    /// <summary>Timed or continuous laser for the pack's LineRenderer and particle-mesh shaders.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public class HS_Laser : MonoBehaviour
    {
        public enum LaserType { Auto, LineRenderer, ParticleMesh }
        public LaserType laserType = LaserType.Auto;
        [Header("Beam")]
        [Min(0f)] public float MaxLength = 12f;
        [Min(0.001f)] public float laserSize = 1f;
        [Min(0.001f)] public float laserScale = 1f;
        public Color laserColor = Color.white;
        public LayerMask collisionMask = ~0;
        [Header("Optional chain bounces")]
        [Tooltip("Jump from the first enemy to nearby unvisited enemies. Pierce Enemies independently controls whether the main beam continues through enemies. Walls still block every jump.")]
        public bool chainBounces;
        [Tooltip("Additional targets after the first hit. Zero stops at the first target without extra jumps.")]
        [Range(0, 100)] public int maxChainBounces = 3;
        [Min(0f)] public float chainBounceRange = 5f;
        [Tooltip("Seconds to extend each chain link. Zero makes jumps immediate, so brief hits from rotating lasers can chain. Independent of the primary beam's Growth Duration.")]
        [Min(0f)] public float chainGrowthDuration;
        [Header("Width flicker")]
        [Tooltip("Animate only the visual beam width. Length, collision and damage are unchanged.")]
        public bool flickerWidth;
        [Tooltip("Average width changes per second. Zero freezes the current width.")]
        [Min(0f)] public float flickerSpeed = 12f;
        [Tooltip("Random variation in each change interval. 0.5 means intervals range from 50% to 150% of 1 / Flicker Speed.")]
        [Range(0f,0.95f)] public float flickerIntervalVariation = 0.5f;
        [Min(0f)] public float minWidthMultiplier = 0.75f;
        [Min(0f)] public float maxWidthMultiplier = 1.25f;
        [Header("Optional damage integration")]
        [Min(0f)] public float damageOverTime = 30f;
        [Tooltip("Apply the same damage per second to enemies inside a sphere at the beam origin, including colliders containing the origin. A target hit by both the sphere and beam is damaged only once per frame by this laser.")]
        public bool damageNearOrigin = true;
        [Tooltip("Radius in world metres. Shares Collision Mask and enemy detection with the beam. Set to zero or disable Damage Near Origin to turn it off.")]
        [Min(0f)] public float originDamageRadius = 0.75f;
        [Tooltip("Pass through targets implementing IHS_LaserDamageable or in Enemy Layers. Walls still block the beam.")]
        public bool pierceEnemies = true;
        [Tooltip("Optional enemy classification for projects using On Hit instead of IHS_LaserDamageable. Leave empty for interface-based detection.")]
        public LayerMask enemyLayers;
        [Tooltip("Called once per hit target per emitting frame with target and damage for that frame. Optional; do not also apply the same damage via the interface.")]
        public HS_LaserHitEvent onHit = new HS_LaserHitEvent();
        [Header("Optional enemy hit effects (shared pool)")]
        public GameObject enemyHitEffectPrefab;
        [Tooltip("Minimum seconds between effects on the same target, shared by lasers using this prefab.")]
        [Min(0.01f)] public float enemyHitEffectInterval = 0.1f;
        [Tooltip("Safety timeout for looping/non-particle effects. One-shot particles return to the pool when finished.")]
        [Min(0.02f)] public float enemyHitEffectMaxLifetime = 2f;
        [Tooltip("Shared active budget per effect prefab. With Ensure Every Hit Target, the pool expands for new targets and refreshes existing effects when full.")]
        [Range(1,256)] public int maxActiveEnemyHitEffects = 32;
        [Tooltip("Guarantee an effect for each hit target: allow pool growth beyond the budget for new targets. Disable for a strict limit that can skip effects.")]
        public bool ensureEveryHitTarget = true;
        public bool followEnemyHitTarget = true;
        [Min(0f)] public float enemyHitEffectOffset = 0.01f;
        [Header("Shot: grow, hold, fade")]
        [Tooltip("Keep the beam active after growing until DisablePrepare is called, including via SendMessage. Ignores Hold Duration; shutdown still uses Fade Duration.")]
        public bool continuous;
        [Min(0f)] public float growthDuration = 0.2f;
        [Min(0f)] public float holdDuration = 1f;
        [Min(0f)] public float fadeDuration = 0.2f;
        public bool playOnEnable = true;
        public bool deactivateWhenFinished = true;
        [Tooltip("Clear Hit/Flash particles at shutdown. Disable to stop emission but let existing particles finish naturally, including the LineRenderer flash. Automatic deactivation waits for these particles; external disabling/destruction still stops them.")]
        public bool clearHitAndFlashOnFadeEnd = true;
        [Header("Particle and material parameters")]
        public GameObject HitEffect;
        public GameObject FlashEffect;
        public float HitOffset;
        public bool useLaserRotation;
        public float MainTextureLength = 1f;
        public bool RandomizeMainOffset = false;
        public bool tileNoise = true;
        public string NoiseTextureName = "_Noise";
        public float NoiseTextureLength = 1f;
        public bool RandomizeNoiseOffset = false;
        public bool RandomizeEveryFrame = false;
        private float MainOffset = 0f;
        private float NoiseOffset = 0f;
        private Vector4 mainST;
        private Vector4 noiseST;
        private string noiseSTName;

        public float TotalDuration => continuous ? float.PositiveInfinity : Mathf.Max(0f, growthDuration) + Mathf.Max(0f, holdDuration) + Mathf.Max(0f, fadeDuration);
        public float CurrentLength { get; private set; }
        public float CurrentWidthMultiplier { get; private set; } = 1f;
        public bool IsBusy { get; private set; }
        public bool IsEmitting => IsBusy && !fading;
        LineRenderer line;
        ParticleSystem mesh;
        ParticleSystemRenderer meshRenderer;
        ParticleSystem[] effects;
        ParticleSystem[] hitEffects;
        MaterialPropertyBlock properties;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];
        Transform owner;
        Vector3 originalScale;
        float originalWidth, age, fadeAge, shotGrowth, shotHold, shotFade;
        float flickerElapsed, flickerDuration, flickerFrom, flickerTo;
        bool flickerWasEnabled;
        bool initialized, meshMode, fading, waitingForEffectTails;
        RaycastHit[] hitBuffer = new RaycastHit[64];
        Collider[] originHitBuffer = new Collider[64];
        readonly List<BeamHit> beamHits = new List<BeamHit>(64);
        readonly HashSet<GameObject> damagedTargets = new HashSet<GameObject>();
        readonly HashSet<GameObject> chainTargets = new HashSet<GameObject>();
        readonly List<ChainLink> chainLinks = new List<ChainLink>();
        readonly HS_LaserTargetQuery chainQuery = new HS_LaserTargetQuery();
        Transform chainPool;
        GameObject chainSeed;
        bool chainVisual;
        sealed class ChainLink
        {
            public HS_Laser beam;
            public GameObject target;
            public float startedAt;
        }
        struct BeamHit
        {
            public RaycastHit hit;
            public MonoBehaviour receiver;
            public bool enemy;
        }

        void Awake() { Initialize(); }
        void OnEnable() { Initialize(); if (playOnEnable) Fire(); }

        void Initialize()
        {
            if (initialized) return;
            initialized = true;
            line = GetComponent<LineRenderer>();
            mesh = GetComponent<ParticleSystem>();
            meshRenderer = GetComponent<ParticleSystemRenderer>();
            meshMode = laserType == LaserType.ParticleMesh || (laserType == LaserType.Auto && !line && mesh);
            effects = GetComponentsInChildren<ParticleSystem>(true);
            hitEffects = HitEffect ? HitEffect.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
            properties = new MaterialPropertyBlock();

            if (properties.HasProperty(Shader.PropertyToID("_MainTex_ST")))
                mainST = properties.GetVector("_MainTex_ST");
            else if (line && line.sharedMaterial && line.sharedMaterial.HasProperty("_MainTex_ST"))
                mainST = line.sharedMaterial.GetVector("_MainTex_ST");

            noiseSTName = NoiseTextureName + "_ST";
            if (properties.HasProperty(Shader.PropertyToID(noiseSTName)))
                noiseST = properties.GetVector(noiseSTName);
            else if (line && line.sharedMaterial && line.sharedMaterial.HasProperty(noiseSTName))
                noiseST = line.sharedMaterial.GetVector(noiseSTName);

            originalScale = transform.localScale;
            if (RandomizeMainOffset) MainOffset = Random.value * 1f;
            if (RandomizeNoiseOffset) NoiseOffset = Random.value * 1f;
            if (line) { originalWidth = line.widthMultiplier; line.useWorldSpace = true; line.positionCount = 2; line.enabled = false; }
            if (meshMode && mesh)
            {
                var main = mesh.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.playOnAwake = false;
                var emission = mesh.emission; emission.enabled = false;
            }
            StopParticles(true);
        }

        public void Fire(Transform damageOwner = null)
        {
            Initialize();
            owner = damageOwner;
            shotGrowth = Mathf.Max(0f, growthDuration);
            shotHold = Mathf.Max(0f, holdDuration);
            shotFade = Mathf.Max(0f, fadeDuration);
            age = fadeAge = 0f; fading = false; waitingForEffectTails = false; IsBusy = true; CurrentLength = 0f;
            ClearChain(false);
            ResetFlicker();
            StopParticles(true);
            transform.localScale = originalScale * Mathf.Max(0.001f, laserSize);
            if (!meshMode && FlashEffect && FlashEffect != gameObject) FlashEffect.SetActive(true);
            foreach (var ps in effects)
                if (ps && !IsHit(ps) && ps != mesh) ps.Play(false);
            if (meshMode && mesh) mesh.Play(false);
            if (line && !meshMode) { line.enabled = true; line.widthMultiplier = originalWidth * Mathf.Max(0.001f, laserSize); }
            RenderBeam(0f, 0f, false);
        }

        void LateUpdate()
        {
            if (waitingForEffectTails)
            {
                if (HasEffectTails()) return;
                waitingForEffectTails = false;
                if (deactivateWhenFinished) gameObject.SetActive(false);
            }
            if (!IsBusy) return;
            AdvanceFlicker(Time.deltaTime);
            if (!fading)
            {
                age += Time.deltaTime;
                float length = Mathf.Max(0f, MaxLength) * (shotGrowth <= 0f ? 1f : Mathf.Clamp01(age / shotGrowth));
                RenderBeam(length, 0f, true);
                if (!continuous && age >= shotGrowth + shotHold) DisablePrepare();
            }
            else
            {
                fadeAge += Time.deltaTime;
                float dissolve = shotFade <= 0f ? 1f : Mathf.Clamp01(fadeAge / shotFade);
                if (meshMode) RenderBeam(CurrentLength, dissolve, false);
                foreach (var link in chainLinks)
                    if (link.beam && link.beam.IsBusy) link.beam.RenderBeam(link.beam.CurrentLength, dissolve, false);
                if (fadeAge >= shotFade) Finish();
            }
        }

        bool IsHit(ParticleSystem ps) => HitEffect && (ps.transform == HitEffect.transform || ps.transform.IsChildOf(HitEffect.transform));

        bool Cast(float distance, out RaycastHit closest)
        {
            closest = default;
            beamHits.Clear();
            if (distance <= 0f) return false;
            int count;
            while ((count = Physics.RaycastNonAlloc(transform.position, transform.forward, hitBuffer, distance, collisionMask, QueryTriggerInteraction.Ignore)) == hitBuffer.Length)
                System.Array.Resize(ref hitBuffer, hitBuffer.Length * 2);
            RaycastHit[] hits = hitBuffer;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (IsIgnored(hits[i].collider)) continue;
                MonoBehaviour receiver = FindDamageReceiver(hits[i].collider);
                bool enemy = receiver || (enemyLayers.value & (1 << hits[i].collider.gameObject.layer)) != 0;
                beamHits.Add(new BeamHit { hit = hits[i], receiver = receiver, enemy = enemy });
                if (pierceEnemies && enemy) continue;
                if (hits[i].distance >= nearest) continue;
                nearest = hits[i].distance; closest = hits[i];
            }
            return nearest < float.PositiveInfinity;
        }

        void RenderBeam(float requestedLength, float dissolve, bool applyDamage)
        {
            CurrentWidthMultiplier = WidthMultiplier();
            RaycastHit hit = default;
            bool hasHit = !chainVisual && !fading && Cast(requestedLength, out hit);
            CurrentLength = hasHit ? hit.distance : requestedLength;
            Vector3 end = transform.position + transform.forward * CurrentLength;
            if (line && !meshMode)
            {
                line.widthMultiplier = originalWidth * Mathf.Max(0.001f, laserSize) * CurrentWidthMultiplier;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, end);
                line.GetPropertyBlock(properties);
                float size = Mathf.Max(0.001f, laserSize);
                
                if (RandomizeMainOffset && RandomizeEveryFrame) MainOffset = Random.value * 1f;
                if (RandomizeNoiseOffset && RandomizeEveryFrame) NoiseOffset = Random.value * 1f;
                mainST.x = MainTextureLength * CurrentLength / size;
                mainST.z = MainOffset;
                properties.SetVector("_MainTex_ST", mainST);

                if (tileNoise)
                {          
                    noiseST.x = NoiseTextureLength * CurrentLength / size;
                    noiseST.z = NoiseOffset;
                    properties.SetVector(noiseSTName, noiseST);
                }
                line.SetPropertyBlock(properties);
            }
            if (meshMode && mesh && meshRenderer)
            {
                float scale = Mathf.Max(0.001f, laserScale * laserSize);
                float segment = 2f * scale;
                int count = Mathf.Min(10000, Mathf.CeilToInt(CurrentLength / segment));
                if (particles.Length < count) particles = new ParticleSystem.Particle[Mathf.NextPowerOfTwo(count)];
                var main = mesh.main; if (main.maxParticles < count) main.maxParticles = count;
                for (int i = 0; i < count; i++)
                {
                    particles[i].position = transform.InverseTransformPoint(transform.position + transform.forward * (i * segment));
                    particles[i].startSize3D = new Vector3(0.001f, 0.001f, 2f * laserScale);
                    particles[i].startColor = laserColor;
                    particles[i].startLifetime = particles[i].remainingLifetime = 1000f;
                }
                mesh.SetParticles(particles, count);
                meshRenderer.GetPropertyBlock(properties);
                properties.SetVector("_StartPoint", transform.position);
                properties.SetVector("_EndPoint", end);
                properties.SetFloat("_Distance", CurrentLength);
                // The shader expands vertices along their normals with _Scale.
                // Keep particle spacing and endpoints at the base scale: only width flickers.
                properties.SetFloat("_Scale", scale * CurrentWidthMultiplier);
                properties.SetFloat("_Dissolve", dissolve);
                meshRenderer.SetPropertyBlock(properties);
            }
            if (HitEffect)
            {
                HitEffect.transform.position = hasHit ? hit.point + hit.normal * HitOffset : end;
                if (hasHit) HitEffect.transform.rotation = useLaserRotation ? transform.rotation : Quaternion.LookRotation(hit.normal);
                foreach (var ps in hitEffects)
                {
                    if (hasHit && !ps.isPlaying) ps.Play(false);
                    else if (!hasHit && ps.isPlaying) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            if (applyDamage && !fading)
            {
                // The nearest enemy seeds the chain even when the main beam pierces it.
                // Choose before damage, since a lethal hit may return a target to its pool.
                Collider seedCollider = null;
                Vector3 seedPoint = end;
                float seedDistance = float.PositiveInfinity;
                if (chainBounces && maxChainBounces > 0 && chainBounceRange > 0f)
                foreach (var entry in beamHits)
                    if (entry.enemy && entry.hit.collider && entry.hit.distance <= CurrentLength + 0.0001f && entry.hit.distance < seedDistance)
                    { seedCollider = entry.hit.collider; seedPoint = entry.hit.point; seedDistance = entry.hit.distance; }
                ApplyDamage(CurrentLength);
                UpdateChain(seedCollider, seedPoint);
            }
        }

        void UpdateChain(Collider firstCollider, Vector3 firstPoint)
        {
            if (chainVisual) return;
            if (!chainBounces || maxChainBounces <= 0 || chainBounceRange <= 0f || !firstCollider)
            {
                ClearChain(false);
                return;
            }
            var firstReceiver = FindDamageReceiver(firstCollider);
            if (!firstReceiver && (enemyLayers.value & (1 << firstCollider.gameObject.layer)) == 0)
            {
                ClearChain(false);
                return;
            }
            GameObject seed = HS_LaserTargetQuery.Identity(firstCollider, firstReceiver);
            if (chainSeed != seed) ClearChain(false);
            chainSeed = seed;
            chainTargets.Clear();
            chainTargets.Add(seed);
            Vector3 from = firstPoint;
            int used = 0;
            for (int i = 0; i < Mathf.Clamp(maxChainBounces, 0, 100); i++)
            {
                if (!chainQuery.FindNearest(from, chainBounceRange, owner, transform, collisionMask,
                    enemyLayers, chainTargets, out var target)) break;
                ChainLink link = GetChainLink(i);
                if (link.target != target.gameObject)
                {
                    link.target = target.gameObject;
                    link.startedAt = age;
                    link.beam.gameObject.SetActive(true);
                    link.beam.Fire(owner);
                    if (link.beam.FlashEffect) link.beam.FlashEffect.SetActive(false);
                }
                Vector3 delta = target.point - from;
                float distance = delta.magnitude;
                link.beam.transform.SetPositionAndRotation(from,
                    distance > 0.0001f ? Quaternion.LookRotation(delta) : transform.rotation);
                float progress = chainGrowthDuration <= 0f ? 1f : Mathf.Clamp01((age - link.startedAt) / chainGrowthDuration);
                link.beam.AdvanceFlicker(Time.deltaTime);
                link.beam.RenderBeam(distance * progress, 0f, false);
                used++;
                chainTargets.Add(target.gameObject);
                if (progress < 1f) break;
                Vector3 normal = distance > 0.0001f ? -delta / distance : -transform.forward;
                DamageTarget(target.collider, target.receiver, true, target.collider.ClosestPoint(from), normal,
                    Mathf.Max(0f, damageOverTime) * Time.deltaTime);
                from = target.point;
            }
            for (int i = used; i < chainLinks.Count; i++) HideChainLink(chainLinks[i]);
        }

        ChainLink GetChainLink(int index)
        {
            while (chainLinks.Count <= index)
            {
                if (!chainPool)
                {
                    // Keep clones outside the source hierarchy so cloning never copies previous links.
                    chainPool = new GameObject("HS Chain Visual Pool").transform;
                    chainPool.gameObject.SetActive(false);
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(chainPool.gameObject, gameObject.scene);
                }
                var clone = Instantiate(gameObject, chainPool).GetComponent<HS_Laser>();
                clone.name = name + " — chain segment";
                clone.chainVisual = true;
                clone.chainBounces = false;
                clone.damageNearOrigin = false;
                clone.damageOverTime = 0f;
                clone.playOnEnable = false;
                clone.deactivateWhenFinished = false;
                clone.enabled = false;
                // Initializers must capture the prefab scale, not an already size-adjusted shot scale.
                clone.transform.localScale = originalScale;
                clone.Initialize();
                clone.gameObject.SetActive(false);
                clone.transform.SetParent(null, false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone.gameObject, gameObject.scene);
                chainLinks.Add(new ChainLink { beam = clone });
            }
            return chainLinks[index];
        }

        void HideChainLink(ChainLink link)
        {
            link.target = null;
            if (link.beam && link.beam.gameObject.activeSelf) link.beam.gameObject.SetActive(false);
        }

        void ClearChain(bool destroy)
        {
            chainSeed = null;
            foreach (var link in chainLinks)
            {
                HideChainLink(link);
                if (destroy && link.beam) Destroy(link.beam.gameObject);
            }
            if (!destroy) return;
            chainLinks.Clear();
            if (chainPool) Destroy(chainPool.gameObject);
            chainPool = null;
        }

        void ApplyDamage(float visibleLength)
        {
            damagedTargets.Clear();
            float damage = Mathf.Max(0f, damageOverTime) * Time.deltaTime;
            foreach (var entry in beamHits)
            {
                if (!entry.hit.collider || entry.hit.distance > visibleLength + 0.0001f) continue;
                DamageTarget(entry.hit.collider, entry.receiver, entry.enemy, entry.hit.point, entry.hit.normal, damage);
            }
            if (!damageNearOrigin || originDamageRadius <= 0f) return;
            Vector3 origin = transform.position;
            int count;
            while ((count = Physics.OverlapSphereNonAlloc(origin, originDamageRadius, originHitBuffer,
                collisionMask, QueryTriggerInteraction.Ignore)) == originHitBuffer.Length)
                System.Array.Resize(ref originHitBuffer, originHitBuffer.Length * 2);
            Collider[] overlaps = originHitBuffer;
            for (int i = 0; i < count; i++)
            {
                Collider collider = overlaps[i];
                if (!collider || IsIgnored(collider)) continue;
                MonoBehaviour receiver = FindDamageReceiver(collider);
                bool enemy = receiver || (enemyLayers.value & (1 << collider.gameObject.layer)) != 0;
                if (!enemy) continue;
                Vector3 point = collider.ClosestPoint(origin);
                Vector3 normal = origin - point;
                // ClosestPoint returns the origin when it lies inside the collider.
                if (normal.sqrMagnitude < 0.000001f) normal = origin - collider.bounds.center;
                normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : -transform.forward;
                DamageTarget(collider, receiver, true, point, normal, damage);
            }
        }

        bool IsIgnored(Collider collider)
        {
            Transform target = collider.transform;
            return target == transform || target.IsChildOf(transform) ||
                (owner && (target == owner || target.IsChildOf(owner)));
        }

        MonoBehaviour FindDamageReceiver(Collider collider)
        {
            return HS_LaserReceiverCache.Get(collider);
        }

        /// <summary>Call after changing a target's health components or hierarchy during a laser hit callback.</summary>
        public static void InvalidateDamageReceiverCache() { HS_LaserReceiverCache.Clear(); }

        void DamageTarget(Collider collider, MonoBehaviour receiver, bool enemy, Vector3 point, Vector3 normal, float damage)
        {
            if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy) return;
            GameObject target = receiver ? receiver.gameObject :
                (collider.attachedRigidbody ? collider.attachedRigidbody.gameObject : collider.gameObject);
            // Shared across beam and origin sphere, including compound colliders.
            if (!damagedTargets.Add(target)) return;
            if (enemy && enemyHitEffectPrefab)
                HS_LaserImpactBackend.Spawn?.Invoke(enemyHitEffectPrefab, target,
                    point + normal * Mathf.Max(0f, enemyHitEffectOffset), normal,
                    enemyHitEffectInterval, enemyHitEffectMaxLifetime, maxActiveEnemyHitEffects, followEnemyHitTarget, ensureEveryHitTarget);
            if (damage <= 0f) return;
            if (receiver) ((IHS_LaserDamageable)receiver).TakeDamage(damage);
            onHit?.Invoke(target, damage);
        }

        void OnDrawGizmosSelected()
        {
            if (!damageNearOrigin || originDamageRadius <= 0f) return;
            Gizmos.color = new Color(1f, 0.65f, 0.15f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, originDamageRadius);
        }

        float WidthMultiplier()
        {
            if (!flickerWidth) return 1f;
            float min = Mathf.Max(0f, Mathf.Min(minWidthMultiplier, maxWidthMultiplier));
            float max = Mathf.Max(min, Mathf.Max(minWidthMultiplier, maxWidthMultiplier));
            float progress = Mathf.SmoothStep(0f,1f,flickerElapsed / Mathf.Max(0.05f,flickerDuration));
            return Mathf.Clamp(Mathf.Lerp(flickerFrom,flickerTo,progress),min,max);
        }

        float RandomWidth()
        {
            float min = Mathf.Max(0f, Mathf.Min(minWidthMultiplier,maxWidthMultiplier));
            return Random.Range(min,Mathf.Max(min,Mathf.Max(minWidthMultiplier,maxWidthMultiplier)));
        }

        float RandomFlickerDuration()
        {
            float variation = Mathf.Clamp(flickerIntervalVariation,0f,0.95f);
            return Random.Range(1f-variation,1f+variation);
        }

        void ResetFlicker()
        {
            flickerWasEnabled=flickerWidth;
            flickerFrom=flickerWidth?RandomWidth():1f;
            flickerTo=flickerWidth?RandomWidth():1f;
            flickerDuration=flickerWidth?RandomFlickerDuration():1f;
            // Each instance starts at its own point in an independently timed transition.
            flickerElapsed=flickerWidth?Random.value*flickerDuration:0f;
        }

        void AdvanceFlicker(float deltaTime)
        {
            if(!flickerWidth) { flickerWasEnabled=false; return; }
            if(!flickerWasEnabled) ResetFlicker();
            flickerElapsed+=Mathf.Max(0f,deltaTime)*Mathf.Max(0f,flickerSpeed);
            int steps=0;
            while(flickerElapsed>=flickerDuration && steps++<8)
            {
                flickerElapsed-=flickerDuration;
                flickerFrom=flickerTo;
                flickerTo=RandomWidth();
                flickerDuration=RandomFlickerDuration();
            }
            // Keep work bounded after a long frame or an extreme speed setting.
            if(flickerElapsed>=flickerDuration)flickerElapsed=Mathf.Repeat(flickerElapsed,flickerDuration);
        }

        // Kept for callers that previously used Hovl_Laser/Hovl_Laser2.
        public void DisablePrepare()
        {
            if (!IsBusy || fading) return;
            fading = true; fadeAge = 0f;
            foreach (var link in chainLinks) if (link.beam && link.beam.IsBusy) link.beam.DisablePrepare();
            if (line) line.enabled = false;
            StopParticles(false);
            if (clearHitAndFlashOnFadeEnd && !meshMode && FlashEffect && FlashEffect != gameObject)
            {
                // Clear live flash particles too: StopEmitting alone leaves a visible tail.
                foreach (var ps in effects)
                    if (ps && (ps.transform == FlashEffect.transform || ps.transform.IsChildOf(FlashEffect.transform)))
                        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                FlashEffect.SetActive(false);
            }
            if (shotFade <= 0f) Finish();
        }

        void StopParticles(bool clear)
        {
            if (effects == null) return;
            foreach (var ps in effects)
                if (ps && (clear || ps != mesh)) ps.Stop(false, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }

        void Finish()
        {
            IsBusy = false; CurrentLength = 0f;
            ClearChain(false);
            if (line) line.enabled = false;
            if (clearHitAndFlashOnFadeEnd) StopParticles(true);
            else if (effects != null)
                foreach (var ps in effects)
                    if (ps) ps.Stop(false, IsTailEffect(ps) && ps != mesh
                        ? ParticleSystemStopBehavior.StopEmitting : ParticleSystemStopBehavior.StopEmittingAndClear);
            waitingForEffectTails = !clearHitAndFlashOnFadeEnd && deactivateWhenFinished && HasEffectTails();
            if (deactivateWhenFinished && !waitingForEffectTails) gameObject.SetActive(false);
        }

        bool IsTailEffect(ParticleSystem ps) => IsHit(ps) ||
            (FlashEffect && (ps.transform == FlashEffect.transform || ps.transform.IsChildOf(FlashEffect.transform)));

        bool HasEffectTails()
        {
            if (effects != null)
                foreach (var ps in effects)
                    if (ps && ps != mesh && ps.gameObject.activeInHierarchy && IsTailEffect(ps) && ps.IsAlive(false)) return true;
            return false;
        }

        void OnDisable() { waitingForEffectTails = false; IsBusy = false; CurrentLength = 0f; if (line) line.enabled = false; StopParticles(true); ClearChain(false); }
        void OnDestroy() { ClearChain(true); }
    }

    // Shared only within a frame: no persistent references to pooled/deleted enemies.
    // All beams, origin spheres and chain queries reuse one hierarchy lookup per collider.
    internal static class HS_LaserReceiverCache
    {
        static readonly Dictionary<Collider, MonoBehaviour> entries = new Dictionary<Collider, MonoBehaviour>();
        static readonly List<MonoBehaviour> components = new List<MonoBehaviour>();
        static int frame = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Clear() { entries.Clear(); components.Clear(); frame = -1; }

        internal static MonoBehaviour Get(Collider collider)
        {
            if (!collider || !collider.gameObject.activeInHierarchy) return null;
            if (frame != Time.frameCount) { entries.Clear(); frame = Time.frameCount; }
            if (entries.TryGetValue(collider, out var receiver))
            {
                if (ReferenceEquals(receiver, null)) return null;
                if (receiver && receiver.gameObject.activeInHierarchy &&
                    (collider.transform == receiver.transform || collider.transform.IsChildOf(receiver.transform))) return receiver;
            }
            components.Clear();
            collider.GetComponentsInParent(false, components);
            receiver = null;
            foreach (var component in components)
                if (component is IHS_LaserDamageable) { receiver = component; break; }
            entries[collider] = receiver;
            return receiver;
        }
    }

    // Shared target selection for chaining and the optional spawner mode; no demo-enemy dependency.
    internal sealed class HS_LaserTargetQuery
    {
        internal struct Target
        {
            public Collider collider;
            public MonoBehaviour receiver;
            public GameObject gameObject;
            public Vector3 point;
        }
        Collider[] colliders = new Collider[64];
        RaycastHit[] hits = new RaycastHit[64];

        internal static GameObject Identity(Collider collider, MonoBehaviour receiver) => receiver ? receiver.gameObject :
            (collider.attachedRigidbody ? collider.attachedRigidbody.gameObject : collider.gameObject);

        internal static Vector3 AimPoint(Collider collider)
        {
            // Transform local centres directly so aiming follows this frame's movement,
            // even before the next fixed physics step updates the collider's world bounds.
            if (collider is SphereCollider sphere) return sphere.transform.TransformPoint(sphere.center);
            if (collider is BoxCollider box) return box.transform.TransformPoint(box.center);
            if (collider is CapsuleCollider capsule) return capsule.transform.TransformPoint(capsule.center);
            if (collider is CharacterController character) return character.transform.TransformPoint(character.center);
            if (collider is MeshCollider mesh && mesh.sharedMesh) return mesh.transform.TransformPoint(mesh.sharedMesh.bounds.center);
            return collider.bounds.center;
        }

        MonoBehaviour Receiver(Collider collider)
        {
            return HS_LaserReceiverCache.Get(collider);
        }

        static bool Ignored(Collider collider, Transform owner, Transform self)
        {
            Transform t = collider.transform;
            return (owner && (t == owner || t.IsChildOf(owner))) || (self && (t == self || t.IsChildOf(self)));
        }

        internal bool FindNearest(Vector3 origin, float range, Transform owner, Transform self,
            LayerMask collisionMask, LayerMask enemyLayers, HashSet<GameObject> excluded, out Target result)
        {
            result = default;
            if (range <= 0f) return false;
            int count;
            while ((count = Physics.OverlapSphereNonAlloc(origin, range, colliders, collisionMask,
                QueryTriggerInteraction.Ignore)) == colliders.Length)
                System.Array.Resize(ref colliders, colliders.Length * 2);
            float best = range * range;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                Collider collider = colliders[i];
                if (!collider || Ignored(collider, owner, self)) continue;
                MonoBehaviour receiver = Receiver(collider);
                if (!receiver && (enemyLayers.value & (1 << collider.gameObject.layer)) == 0) continue;
                GameObject identity = Identity(collider, receiver);
                if (excluded != null && excluded.Contains(identity)) continue;
                Vector3 point = AimPoint(collider);
                float distance = (point - origin).sqrMagnitude;
                if (distance > best || (found && Mathf.Approximately(distance, best) && CompareIdentity(identity, result.gameObject) >= 0)) continue;
                if (!ClearPath(origin, point, owner, self, collisionMask, enemyLayers)) continue;
                best = distance;
                found = true;
                result = new Target { collider = collider, receiver = receiver, gameObject = identity, point = point };
            }
            return found;
        }

        // Only used to break equal-distance ties, not to infer creation order.
        static int CompareIdentity(GameObject a, GameObject b)
        {
#if UNITY_6000_5_OR_NEWER
            return a.GetEntityId().CompareTo(b.GetEntityId());
#else
            return a.GetInstanceID().CompareTo(b.GetInstanceID());
#endif
        }

        bool ClearPath(Vector3 origin, Vector3 end, Transform owner, Transform self, LayerMask mask, LayerMask enemyLayers)
        {
            Vector3 delta = end - origin;
            float distance = delta.magnitude;
            if (distance <= 0.0001f) return true;
            int count;
            while ((count = Physics.RaycastNonAlloc(origin, delta / distance, hits, distance, mask,
                QueryTriggerInteraction.Ignore)) == hits.Length)
                System.Array.Resize(ref hits, hits.Length * 2);
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (!collider || Ignored(collider, owner, self)) continue;
                if (Receiver(collider) || (enemyLayers.value & (1 << collider.gameObject.layer)) != 0) continue;
                return false;
            }
            return true;
        }
    }
}
