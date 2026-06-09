using UnityEngine;

public class PreserveProducts : MonoBehaviour
{
    private void Awake()
    {
        // Protects this object and its network loop from being cleared 
        // when Horizon OS forces a deep suspension. So no obejects that are grabbable dissapear if you long-press Meta button
        DontDestroyOnLoad(gameObject);
    }
}