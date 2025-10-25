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
        private IInteractable _activeInteractable; // Currently being interacted with

        private IInteractable _currentInteractable;
        private Coroutine _holdCoroutine;
        private bool _isInteracting;
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

            CleanupInteraction();
        }

        private void UpdateCurrentInteractable()
        {
            // Don't change highlight during active interaction
            if (_isInteracting) return;

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
                var interactable = hit.collider.GetComponent<IInteractable>();
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
            // Only start if we have a valid interactable and not already interacting
            if (_currentInteractable != null && !_isInteracting) StartInteraction(_currentInteractable);
        }

        private void StartInteraction(IInteractable interactable)
        {
            _isInteracting = true;
            _activeInteractable = interactable;

            // Start the interaction
            interactable.OnInteractStart();

            // Start the hold coroutine
            _holdCoroutine = StartCoroutine(HandleInteractionHold());

            Debug.Log($"Started interaction with {interactable}");
        }

        private void TryEndInteraction()
        {
            // Only end if we're currently interacting
            if (_isInteracting) EndInteraction();
        }

        private void EndInteraction()
        {
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            if (_activeInteractable != null)
            {
                _activeInteractable.OnInteractEnd();
                Debug.Log($"Ended interaction with {_activeInteractable}");
            }

            _isInteracting = false;
            _activeInteractable = null;
        }

        private void CleanupInteraction()
        {
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            // If we were interacting during disable, end it properly
            if (_isInteracting && _activeInteractable != null) _activeInteractable.OnInteractEnd();

            _isInteracting = false;
            _activeInteractable = null;
        }

        private IEnumerator HandleInteractionHold()
        {
            while (_isInteracting && _activeInteractable != null)
            {
                // Call the interactable's hold method and wait for it to complete
                yield return _activeInteractable.OnInteractHold();

                // Check if we're still over the same interactable
                if (!IsInteractableStillValid())
                {
                    EndInteraction();
                    yield break;
                }
            }

            // If we exit the loop, end the interaction
            EndInteraction();
        }

        private bool IsInteractableStillValid()
        {
            // Check if the active interactable is still the current one and valid
            return _activeInteractable != null &&
                   _activeInteractable == _currentInteractable;
        }
    }
}