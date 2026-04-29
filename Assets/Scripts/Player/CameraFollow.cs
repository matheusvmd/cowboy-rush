using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform alvo;
    [SerializeField] private float suavidade = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private float limiteEsquerdoX = -100f;
    [SerializeField] private float limiteDireitoX = 100f;

    private void LateUpdate()
    {
        if (alvo == null) return;

        Vector3 posAlvo = alvo.position + offset;
        posAlvo.x = Mathf.Clamp(posAlvo.x, limiteEsquerdoX, limiteDireitoX);

        transform.position = Vector3.Lerp(transform.position, posAlvo, suavidade * Time.deltaTime);
    }
}
