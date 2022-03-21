using UnityEngine;
using Mirror;

public class PlayerPlantManager : NetworkBehaviour
{
    public bool isInPlantableArea;

    public GameManager gameManager;

    void Update()
    {
        plant();
    }

    void plant()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("PlantableArea")) isInPlantableArea = true;
    }

}
