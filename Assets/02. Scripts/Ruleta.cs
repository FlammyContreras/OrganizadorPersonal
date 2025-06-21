using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Ruleta : MonoBehaviour
{
    [Header("Opciones de la ruleta (editables)")]
    public List<string> opciones = new List<string>();

    [Header("Referencias UI")]
    public TextMeshProUGUI resultadoTexto;
    public TMP_InputField inputOpcion;
    public Transform listaOpcionesParent;
    public GameObject opcionPrefab;
    public Transform ruletaVisualParent; // Nuevo: el centro visual de la ruleta
    public GameObject segmentoPrefab;    // Nuevo: prefab de un segmento (Image + TextMeshProUGUI)

    private bool girando = false;
    private const int LIMITE_OPCIONES = 50;

    private void Awake()
    {
        resultadoTexto.text = "";
        resultadoTexto.GetComponentInParent <Image>().color = new Color(0, 0, 0, 0f); // Fondo semitransparente
    }
    public void AgregarOpcion()
    {
        string nuevaOpcion = inputOpcion.text.Trim();
        if (string.IsNullOrEmpty(nuevaOpcion) || opciones.Count >= LIMITE_OPCIONES) return;
        opciones.Add(nuevaOpcion);
        inputOpcion.text = "";
        ActualizarListaOpciones();
        ActualizarRuletaVisual();
    }

    public void EliminarOpcion(int indice)
    {
        if (indice < 0 || indice >= opciones.Count) return;
        opciones.RemoveAt(indice);
        ActualizarListaOpciones();
        ActualizarRuletaVisual();
    }

    public void ActualizarListaOpciones()
    {
        foreach (Transform child in listaOpcionesParent)
            Destroy(child.gameObject);

        for (int i = 0; i < opciones.Count; i++)
        {
            GameObject go = Instantiate(opcionPrefab, listaOpcionesParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = opciones[i];
            int index = i;
            go.GetComponentInChildren<Button>().onClick.AddListener(() => EliminarOpcion(index));
        }
    }

    public void ActualizarRuletaVisual()
    {
        // Limpia los segmentos actuales
        foreach (Transform child in ruletaVisualParent)
            Destroy(child.gameObject);

        int n = opciones.Count;
        if (n == 0) return;

        float anguloPorOpcion = 360f / n;

        for (int i = 0; i < n; i++)
        {
            GameObject segmento = Instantiate(segmentoPrefab, ruletaVisualParent);
            segmento.transform.localRotation = Quaternion.Euler(0, 0, -i * anguloPorOpcion);

            // Ajusta el color para diferenciar segmentos
            Image img = segmento.GetComponent<Image>();
            if (img != null)
                img.color = Color.HSVToRGB(i / (float)n, 0.6f, 1f);

            // Asigna el texto de la opción
            TextMeshProUGUI txt = segmento.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = opciones[i];
        }
    }

    public void GirarRuleta()
    {
        if (girando) return;
        if (opciones == null || opciones.Count == 0)
        {
            resultadoTexto.text = "No hay opciones.";
            return;
        }
        StartCoroutine(GirarAnimacion());
    }

    private IEnumerator GirarAnimacion()
    {
        girando = true;
        float duracion = 2f;
        float tiempo = 0f;
        float anguloFinal = 360f * Random.Range(3, 6) + (360f / opciones.Count) * Random.Range(0, opciones.Count);

        float anguloInicial = ruletaVisualParent.eulerAngles.z;
        float anguloObjetivo = anguloInicial + anguloFinal;

        while (tiempo < duracion)
        {
            float anguloActual = Mathf.Lerp(anguloInicial, anguloObjetivo, tiempo / duracion);
            ruletaVisualParent.eulerAngles = new Vector3(0, 0, anguloActual);
            tiempo += Time.deltaTime;
            yield return null;
        }

        ruletaVisualParent.eulerAngles = new Vector3(0, 0, anguloObjetivo);

        // Determinar la opción ganadora
        float angulo = anguloObjetivo % 360f;
        int indice = opciones.Count - 1 - Mathf.FloorToInt(angulo / (360f / opciones.Count));
        if (indice < 0) indice += opciones.Count;
        string resultado = opciones[indice];
        resultadoTexto.text = $"Resultado: {resultado}";
        resultadoTexto.GetComponentInParent<Image>().color = new Color(0, 0, 0, 0.5f); // Fondo semitransparente
        girando = false;
    }
}
