using System.Collections;
using UnityEngine;

namespace VK.Interaction.Samples
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")] [SerializeField]
        private float _interactionRange = 2f;

        // IInteractable implementation
        public float InteractionRange => _interactionRange;

        public virtual void OnHighlight()
        {
            // Visual feedback
            Debug.Log($"{gameObject.name} highlighted (Range: {InteractionRange})");
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