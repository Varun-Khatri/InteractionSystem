using System.Collections;

namespace VK.Interaction
{
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player starts interacting with the object.
        /// </summary>
        void OnInteractStart();

        /// <summary>
        /// Called when the player continues interacting with the object.
        /// </summary>
        IEnumerator OnInteractHold();

        /// <summary>
        /// Called when the player stops interacting with the object.
        /// </summary>
        void OnInteractEnd();

        /// <summary>
        /// Optional: Called when the object is highlighted or selected for interaction.
        /// </summary>
        void OnHighlight();

        /// <summary>
        /// Optional: Called when the object is no longer highlighted or selected.
        /// </summary>
        void OnUnhighlight();
    }
}
