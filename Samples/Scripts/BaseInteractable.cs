using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VK.Input;

namespace VK.Interaction
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] protected InputHandler _inputHandler;
        [SerializeField] protected Camera _camera;
        [SerializeField] protected LayerMask _interactionMask;
        protected Coroutine _holdCoroutine;

        private bool _isHighlighted = true;  // To track if the object is currently highlighted

        private void OnEnable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnInteractPressed += TryStartInteraction;
                _inputHandler.OnInteractReleased += TryEndInteraction;
            }
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnInteractPressed -= TryStartInteraction;
                _inputHandler.OnInteractReleased -= TryEndInteraction;
            }
        }
        private void Start()
        {
            _holdCoroutine = null;
            UpdateHighlight();
        }
        public virtual void OnHighlight()
        {
        }

        public virtual void OnInteractStart()
        {
            // Prevent starting interaction twice
            if (_holdCoroutine == null && IsMouseOverInteractable())
            {
                _holdCoroutine = StartCoroutine(OnInteractHold());
            }
        }

        public virtual IEnumerator OnInteractHold()
        {
            while (IsMouseOverInteractable())
            {

                yield return null;
            }
            // Automatically stop interaction if the mouse moves away
            TryEndInteraction();
        }

        public virtual void OnInteractEnd()
        {

            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

        }

        public virtual void OnUnhighlight()
        {
        }

        private bool IsMouseOverInteractable()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, _interactionMask);

            if (hit.collider != null)
            {
                return hit.collider.gameObject == gameObject;
            }

            return false;
        }

        private void TryStartInteraction()
        {
            if (IsMouseOverInteractable())
            {
                OnInteractStart();
            }
        }

        private void TryEndInteraction()
        {
            if (_holdCoroutine != null)  // Only end interaction if it was started
            {
                OnInteractEnd();
            }
        }

        protected virtual void Update()
        {
            UpdateHighlight();
        }

        private void UpdateHighlight()
        {
            bool isCurrentlyHighlighted = IsMouseOverInteractable();

            // Only call OnHighlight or OnUnhighlight if the highlight state changes
            if (isCurrentlyHighlighted && !_isHighlighted)
            {
                OnHighlight();
                _isHighlighted = true;
            }
            else if (!isCurrentlyHighlighted && _isHighlighted)
            {
                OnUnhighlight();
                _isHighlighted = false;
            }
        }

    }
}
