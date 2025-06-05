using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Color textColor; // Use a Color bc you cannot change textMesh.color.a value
    private float lifeTime;
    private static int sortingOrder; // Sorting order so that the newest always stay up top on rendering

    void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
    }

    void Update()
    {
        float moveYSpeed = 0.2f;
        transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;

        lifeTime -= Time.deltaTime;

        if (lifeTime > 0.7f) // Text animation
        {
            transform.localScale += 1.5f * Time.deltaTime * Vector3.one; // Inflate the text
        } else
        {
            transform.localScale -= 1f * Time.deltaTime * Vector3.one; // Deflate the text
        }

        if (lifeTime <= 0) // if life time ends then start disappearing
        {
            float disappearSpeed = 5f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Reset();
                ObjectPools.EnqueueObject(this, "DamagePopUp");
            }
        }
    }

    // Create a damage text pop up object
    public static void Create(Vector3 position, int damageAmount)
    {
        DamagePopUp damagePopUp = ObjectPools.DequeueObject<DamagePopUp>("DamagePopUp");
        damagePopUp.gameObject.transform.position = position;
        damagePopUp.gameObject.SetActive(true);
        
        //damagePopUp.textMesh.ForceMeshUpdate(); // Force refresh
        damagePopUp.textMesh.SetText(damageAmount.ToString());
        damagePopUp.lifeTime = 1f;
        damagePopUp.textColor = damagePopUp.textMesh.color;

        sortingOrder += 1;
        damagePopUp.textMesh.sortingOrder = sortingOrder;

    }

    // Reset values for the next activation
    private void Reset()
    {
        textColor.a = 1f;
        textMesh.color = textColor;
        transform.localScale = Vector3.one;
        textMesh.SetText("");
    }
}
