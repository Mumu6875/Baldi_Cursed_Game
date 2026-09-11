using UnityEngine;

public class NearExitTriggerScript : MonoBehaviour
{
    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached || gc == null || !gc.finaleMode || !other.CompareTag("Player"))
        {
            return;
        }

        // The first three exits keep the original Baldi behavior.
        if (gc.exitsReached < 3)
        {
            reached = true;
            gc.ExitReached();
            if (es != null) es.Lower();
            if (gc.baldiScrpt != null && gc.baldiScrpt.isActiveAndEnabled)
            {
                gc.baldiScrpt.Hear(transform.position, 8f);
            }
            return;
        }

        // IMPORTANT: Start the cursed final-exit sequence here, on the SCHOOL
        // side of the exit. Waiting for ExitTriggerScript means the player has
        // already crossed the boundary and can end up outside the map.
        if (CursedFinalExitSequence.TryStartFromNearExit(this, other, gc, es))
        {
            reached = true;
        }
    }

    public GameControllerScript gc;
    public EntranceScript es;
}
