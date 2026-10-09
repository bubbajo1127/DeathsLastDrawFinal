using UnityEngine;

namespace Hovl
{
    // Optional pool contract lives with the enemy, so no spawner file is required.
    public interface IHS_EnemyPool
    {
        void ReturnToPool(HS_Enemy enemy);
        void NotifyDisabled(HS_Enemy enemy);
        void Forget(HS_Enemy enemy);
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class HS_Enemy : MonoBehaviour, IHS_LaserDamageable
    {
        public Transform player;
        [Min(0f)] public float moveSpeed = 1.5f;
        [Min(1f)] public float maxHealth = 100f;
        [Min(0f)] public float stoppingDistance = 0.45f;
        public Camera healthCamera;
        public RectTransform healthBar;
        public RectTransform healthFill;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        Rigidbody body;
        internal IHS_EnemyPool poolOwner;
        internal HS_Enemy poolPrefab;
        // Interfaces do not use Unity's destroyed-object null comparison.
        bool HasPoolOwner => poolOwner != null &&
            (!(poolOwner is Object unityOwner) || unityOwner != null);

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (!healthBar || !healthFill) CreateHealthBar();
        }

        void OnEnable()
        {
            CurrentHealth = Mathf.Max(1f, maxHealth);
            RefreshHealth();
        }

        void Start()
        {
            if (!player)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged) player = tagged.transform;
            }
            if (!healthCamera) healthCamera = Camera.main;
        }

        void FixedUpdate()
        {
            if (!player || IsDead) return;
            Vector3 delta = player.position - body.position;
            delta.y = 0f;
            float travel = Mathf.Min(Mathf.Max(0f, moveSpeed) * Time.fixedDeltaTime,
                Mathf.Max(0f, delta.magnitude - stoppingDistance));
            body.MovePosition(body.position + delta.normalized * travel);
        }

        void LateUpdate()
        {
            if (!healthCamera) healthCamera = Camera.main;
            if (healthBar && healthCamera) healthBar.rotation = healthCamera.transform.rotation;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f || float.IsNaN(damage)) return;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            RefreshHealth();
            if (IsDead)
            {
                if (HasPoolOwner) poolOwner.ReturnToPool(this);
                else Destroy(gameObject);
            }
        }

        internal void PrepareForSpawn(Transform target, Camera camera, Vector3 position, Quaternion rotation)
        {
            player = target;
            healthCamera = camera;
            if (!body) body = GetComponent<Rigidbody>();
            transform.SetPositionAndRotation(position, rotation);
            if (body)
            {
                body.position = position;
                body.rotation = rotation;
                if (!body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
            }
        }

        void OnDisable()
        {
            HS_LaserImpactBackend.Release?.Invoke(gameObject);
            if (HasPoolOwner) poolOwner.NotifyDisabled(this);
        }

        void OnDestroy() { if (HasPoolOwner) poolOwner.Forget(this); }

        void RefreshHealth()
        {
            if (healthFill) healthFill.anchorMax = new Vector2(CurrentHealth / Mathf.Max(1f, maxHealth), 1f);
        }

        // Also callable by editor setup, so the bar is visible in the saved prefab.
        public void CreateHealthBar()
        {
            if (healthBar && healthFill) return;
            var canvasObject = new GameObject("Health Bar", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            healthBar = (RectTransform)canvasObject.transform;
            healthBar.localPosition = new Vector3(0f, 0.8f, 0f);
            healthBar.localScale = Vector3.one * 0.01f;
            healthBar.sizeDelta = new Vector2(120f, 14f);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var background = new GameObject("Background", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            background.transform.SetParent(healthBar, false);
            var bg = (RectTransform)background.transform;
            bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one; bg.sizeDelta = Vector2.zero;
            var image = background.GetComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.08f, 0.09f, 0.12f, 0.95f); image.raycastTarget = false;
            var fill = new GameObject("Health", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            fill.transform.SetParent(bg, false);
            healthFill = (RectTransform)fill.transform;
            healthFill.anchorMin = Vector2.zero; healthFill.anchorMax = Vector2.one;
            healthFill.offsetMin = new Vector2(2f, 2f); healthFill.offsetMax = new Vector2(-2f, -2f);
            image = fill.GetComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.25f, 1f, 0.4f); image.raycastTarget = false;
        }
    }
}
