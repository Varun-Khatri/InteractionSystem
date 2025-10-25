using System.Collections;
using UnityEngine;

namespace VK.Interaction.Samples
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        // No more input handling here! Just interaction logic.

        public virtual void OnHighlight()
        {
            // Visual feedback (change color, show outline, etc.)
            Debug.Log($"{gameObject.name} highlighted");
        }

        public virtual void OnInteractStart()
        {
            // Start interaction logic
            Debug.Log($"{gameObject.name} interaction started");
        }

        public virtual IEnumerator OnInteractHold()
        {
            // Hold interaction logic - called every frame while interacting
            Debug.Log($"{gameObject.name} holding interaction");
            yield return null; // Continue next frame
        }

        public virtual void OnInteractEnd()
        {
            // End interaction logic
            Debug.Log($"{gameObject.name} interaction ended");
        }

        public virtual void OnUnhighlight()
        {
            // Remove visual feedback
            Debug.Log($"{gameObject.name} unhighlighted");
        }
    }
}