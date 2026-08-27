namespace AliceInCradleHack.utils.animation
{
    /// <summary>
    /// Damped spring animation driven by Hooke's law, integrated with the
    /// semi-implicit (symplectic) Euler method for unconditional stability.
    /// </summary>
    public class SpringAnimation
    {
        private readonly float _stiffness;
        private readonly float _mass;
        private readonly float _damping;
        private float _velocity;

        public float TargetValue { get; set; }
        public float CurrentValue { get; private set; }

        public SpringAnimation(float stiffness, float mass, float damping, float initialValue)
        {
            _stiffness = stiffness;
            _mass = mass;
            _damping = damping;
            CurrentValue = initialValue;
            TargetValue = initialValue;
        }

        public void Reset(float value)
        {
            CurrentValue = value;
            TargetValue = value;
            _velocity = 0f;
        }

        public void Update(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            float force = -_stiffness * (CurrentValue - TargetValue) - _damping * _velocity;
            float acceleration = force / _mass;
            _velocity += acceleration * deltaTime;
            CurrentValue += _velocity * deltaTime;
        }
    }
}
