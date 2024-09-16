using UnityEngine;


namespace VK.Interaction.Samples
{
    public class InteractableObject : BaseInteractable
    {

        public override void OnInteractStart()
        {
            base.OnInteractStart();

            Debug.Log("OnInteractStart");
        }

        public override void OnInteractEnd()
        {
            base.OnInteractEnd();

            Debug.Log("OnInteractEnd");
        }

    }
}
