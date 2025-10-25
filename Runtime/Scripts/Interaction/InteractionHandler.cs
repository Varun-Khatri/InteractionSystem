using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VK.Input;

namespace VK.Interaction
{
    public class InteractionHandler : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private LayerMask _interactionMask;
        [SerializeField] private float _interactionRange = 2f;

        private IInteractable _currentInteractable;
        private Coroutine _holdCoroutine;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;

            if (_inputHandler != null)
            {
                _inputHandler.OnInteractPressed += TryStartInteraction;
                _inputHandler.OnInteractReleased += TryEndInteraction;
            }
        }

        private void Update()
        {
            UpdateCurrentInteractable();
        }

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

            // Clean up any ongoing interaction
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }
        }

        private void UpdateCurrentInteractable()
        {
            var newInteractable = FindInteractableAtMousePosition();

            // If interactable changed
            if (newInteractable != _currentInteractable)
            {
                // Unhighlight previous
                if (_currentInteractable != null) _currentInteractable.OnUnhighlight();

                _currentInteractable = newInteractable;

                // Highlight new
                if (_currentInteractable != null) _currentInteractable.OnHighlight();
            }
        }

        private IInteractable FindInteractableAtMousePosition()
        {
            if (_mainCamera == null) return null;

            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, _interactionMask);

            if (hit.collider != null)
            {
                // Check if the hit object has an IInteractable component
                IInteractable interactable;
                hit.collider.TryGetComponent(out interactable);
                if (interactable != null)
                {
                    // Optional: Check if within interaction range
                    var distance = Vector2.Distance(transform.position, hit.point);
                    if (distance <= _interactionRange) return interactable;
                }
            }

            return null;
        }

        private void TryStartInteraction()
        {
            if (_currentInteractable != null && _holdCoroutine == null)
            {
                _currentInteractable.OnInteractStart();
                _holdCoroutine = StartCoroutine(HandleInteractionHold());
            }
        }

        private void TryEndInteraction()
        {
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;

                if (_currentInteractable != null) _currentInteractable.OnInteractEnd();
            }
        }

        private IEnumerator HandleInteractionHold()
        {
            while (_currentInteractable != null)
            {
                yield return _currentInteractable.OnInteractHold();
                yield return null; // Wait one frame between hold calls
            }

            // If we get here, the interactable became null during hold
            TryEndInteraction();
        }
    }
}