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
        private IInteractable _currentInteractable; // Currently highlighted
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

        // Visualize interaction range in Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
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
                if (_currentInteractable != null)
                {
                    _currentInteractable.OnUnhighlight();
                    Debug.Log($"Unhighlighted: {_currentInteractable}");
                }

                _currentInteractable = newInteractable;

                // Highlight new if it's in range
                if (_currentInteractable != null && IsInteractableInRange(_currentInteractable))
                {
                    _currentInteractable.OnHighlight();
                    Debug.Log($"Highlighted: {_currentInteractable}");
                }
                else if (_currentInteractable != null)
                {
                    // If out of range, don't highlight and clear the reference
                    _currentInteractable = null;
                }
            }

            // Check if current interactable went out of range
            if (_currentInteractable != null && !IsInteractableInRange(_currentInteractable))
            {
                _currentInteractable.OnUnhighlight();
                Debug.Log($"Out of range - Unhighlighted: {_currentInteractable}");
                _currentInteractable = null;
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
                if (interactable != null && IsInteractableInRange(interactable)) return interactable;
            }

            return null;
        }

        private bool IsInteractableInRange(IInteractable interactable)
        {
            if (interactable == null) return false;

            // Get the MonoBehaviour to access transform
            var interactableMono = interactable as MonoBehaviour;
            if (interactableMono == null) return false;

            // Calculate distance from player to interactable
            var distance = Vector2.Distance(transform.position, interactableMono.transform.position);
            var inRange = distance <= _interactionRange;

            Debug.Log($"Distance to {interactable}: {distance:F2} / {_interactionRange} - In Range: {inRange}");
            return inRange;
        }

        private void TryStartInteraction()
        {
            // Only start if we have a valid interactable, not already interacting, and in range
            if (_currentInteractable != null && !_isInteracting && IsInteractableInRange(_currentInteractable))
                StartInteraction(_currentInteractable);
            else
                Debug.Log(
                    $"Cannot start interaction - Interactable: {_currentInteractable != null}, Interacting: {_isInteracting}, In Range: {_currentInteractable != null && IsInteractableInRange(_currentInteractable)}");
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
            if (_isInteracting && _activeInteractable != null)
            {
                _activeInteractable.OnInteractEnd();
                Debug.Log($"Cleanup - Ended interaction with {_activeInteractable}");
            }

            _isInteracting = false;
            _activeInteractable = null;

            // Clear highlight on cleanup
            if (_currentInteractable != null)
            {
                _currentInteractable.OnUnhighlight();
                _currentInteractable = null;
            }
        }

        private IEnumerator HandleInteractionHold()
        {
            while (_isInteracting && _activeInteractable != null)
            {
                // Check if still in range during hold
                if (!IsInteractableInRange(_activeInteractable))
                {
                    Debug.Log("Interactable moved out of range during hold - Ending interaction");
                    EndInteraction();
                    yield break;
                }

                // Call the interactable's hold method and wait for it to complete
                yield return _activeInteractable.OnInteractHold();

                // Check if we're still over the same interactable and in range
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
            // Check if the active interactable is still the current one, valid, and in range
            return _activeInteractable != null &&
                   _activeInteractable == _currentInteractable &&
                   IsInteractableInRange(_activeInteractable);
        }
    }
}