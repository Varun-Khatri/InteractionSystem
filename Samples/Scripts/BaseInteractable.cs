using System.Collections;
using UnityEngine;

namespace VK.Interaction.Samples
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        // No coroutines, no state management - just pure interaction logic

        public virtual void OnHighlight()
        {
            // Visual feedback
            Debug.Log($"{gameObject.name} highlighted");
        }

        public virtual void OnInteractStart()
        {
            // One-time interaction start logic
            Debug.Log($"{gameObject.name} interaction STARTED");
        }

        public virtual IEnumerator OnInteractHold()
        {
            // This is called repeatedly while holding
            // Put continuous interaction logic here
            Debug.Log($"{gameObject.name} holding...");

            // Yield return null to be called again next frame
            yield return null;

            // Or you can yield for specific time
            // yield return new WaitForSeconds(0.1f);
        }

        public virtual void OnInteractEnd()
        {
            // Cleanup logic
            Debug.Log($"{gameObject.name} interaction ENDED");
        }

        public virtual void OnUnhighlight()
        {
            // Remove visual feedback
            Debug.Log($"{gameObject.name} unhighlighted");
        }
    }
}