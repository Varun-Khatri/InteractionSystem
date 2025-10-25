using System.Collections;

namespace VK.Interaction
{
    public interface IInteractable
    {
        /// <summary>
        ///     Called when the player starts interacting with the object.
        /// </summary>
        void OnInteractStart();

        /// <summary>
        ///     Called each frame while the player holds interaction.
        ///     Yield return null to continue next frame.
        /// </summary>
        IEnumerator OnInteractHold();

        /// <summary>
        ///     Called when the player stops interacting with the object.
        /// </summary>
        void OnInteractEnd();

        /// <summary>
        ///     Called when the object becomes the current target for interaction.
        /// </summary>
        void OnHighlight();

        /// <summary>
        ///     Called when the object is no longer the current target for interaction.
        /// </summary>
        void OnUnhighlight();
    }
}