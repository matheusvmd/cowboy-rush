using UnityEngine;

public static class EfeitosVisuais
{
    public static void SpawnBurst(Vector3 posicao, Color cor, int quantidade = 8, float velocidade = 3f, float duracao = 0.5f)
    {
        var go = new GameObject("Burst_Particulas");
        go.transform.position = posicao;

        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor = cor;
        main.startSpeed = velocidade;
        main.startLifetime = duracao;
        main.startSize = 0.13f;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)quantidade) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;

        ps.Play();
    }

    public static void SpawnBurstPortal(Vector3 posicao)
    {
        SpawnBurst(posicao, new Color(0.2f, 0.9f, 1f), 20, 5f, 0.7f);
        SpawnBurst(posicao, new Color(1f, 0.85f, 0.1f), 12, 3.5f, 0.6f);
    }

    public static void SpawnMuzzleFlash(Vector3 posicao, Vector2 direcao)
    {
        SpawnBurst(posicao + (Vector3)(direcao.normalized * 0.18f), new Color(1f, 0.78f, 0.18f), 10, 5.5f, 0.16f);
        SpawnBurst(posicao + (Vector3)(direcao.normalized * 0.12f), new Color(1f, 0.32f, 0.08f), 5, 3.2f, 0.12f);
    }

    public static void SpawnImpactoBala(Vector3 posicao)
    {
        SpawnBurst(posicao, new Color(0.96f, 0.72f, 0.32f), 7, 3.2f, 0.22f);
        SpawnBurst(posicao, new Color(0.2f, 0.14f, 0.09f), 4, 2.1f, 0.28f);
    }
}
