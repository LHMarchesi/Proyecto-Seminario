public interface IDamageable
{
    void TakeDamage(float damage, DamageFeedbackType feedbackType = DamageFeedbackType.Normal);
}
