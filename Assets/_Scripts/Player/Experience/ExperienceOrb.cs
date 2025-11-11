using UnityEngine;

public class ExperienceOrb : OrbBase
{
    [Header("Configuración de Experiencia")]
    [SerializeField] private float minExperience = 5f;
    [SerializeField] private float maxExperience = 15f;

    private ExperienceManager experienceManager;

    protected override void Start()
    {
        base.Start();
        experienceManager = FindObjectOfType<ExperienceManager>();
    }

    protected override void ApplyEffect(GameObject player)
    {
        float xp = Random.Range(minExperience, maxExperience + 1);

        if (experienceManager != null)
            experienceManager.AddExperience(xp);
    }
}
