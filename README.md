# Interaction System

A flexible and extensible interaction system for Unity games. Provides a clean interface for implementing various interactable objects with support for both instant and hold-based interactions.

## 📦 Installation & Setup

### Package Structure

```text
Assets/Packages/[Package Name]/
├── Runtime/                 # Core system files
│   ├── [MainSystemFiles].cs
│   └── ...
└── Samples/                 # Sample implementations
    ├── ExampleComponent1.cs
    ├── ExampleComponent2.cs
    └── ExampleScene.unity   (if included)
```

### Installation Methods
**Method 1: Unity Package Manager (Recommended)**

- Open Window → Package Manager
- Click + → Add package from git URL
- Enter your repository URL:

```text
https://github.com/[username]/[repository-name].git
The system will be installed in Assets/Packages/[System Name]/
```

**Method 2: Manual Installation**

- Download the repository or clone it
- Copy the entire package folder to:

```text
Assets/Packages/[System Name]/
The system is ready to use
```

### Accessing Samples

After installation, access samples at Assets/Packages/[System Name]/Samples/

## 🎯 Features

- **Interface-Based Design** - Clean `IInteractable` interface for easy implementation
- **Multiple Interaction Types** - Support for instant, hold, and continuous interactions
- **Automatic Highlighting** - Built-in mouse-over detection and highlighting
- **Input-Agnostic** - Works with any input system (includes Unity Input System samples)
- **Extensible Base Class** - `BaseInteractable` provides common functionality
- **Coroutine Support** - Built-in support for long-press and continuous interactions

## 🏗️ Architecture

### Core Components

- **`IInteractable`** - Main interface that all interactable objects implement
- **`BaseInteractable`** - Sample Abstract base class handling input and mouse detection
- **`InteractableObject`** - Sample implementation showing basic usage

## 🚀 Quick Start

### 1. Basic Implementation

Create a simple interactable object:

```csharp
using UnityEngine;
using VK.Interaction;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    public void OnInteractStart()
    {
        Debug.Log("Interaction started!");
        // Add your interaction logic here
    }

    public System.Collections.IEnumerator OnInteractHold()
    {
        Debug.Log("Hold interaction in progress...");
        // Continuous interaction logic
        yield return null;
    }

    public void OnInteractEnd()
    {
        Debug.Log("Interaction ended!");
        // Cleanup logic
    }

    public void OnHighlight()
    {
        Debug.Log("Object highlighted");
        // Visual feedback when mouse is over
    }

    public void OnUnhighlight()
    {
        Debug.Log("Object unhighlighted");
        // Remove visual feedback
    }
}
```
### 2. Using the Base Class (Recommended)
Extend BaseInteractable for automatic input handling:

``` csharp
public class DoorInteractable : BaseInteractable
{
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private bool _isOpen = false;

    public override void OnInteractStart()
    {
        base.OnInteractStart();
        
        ToggleDoor();
    }

    public override System.Collections.IEnumerator OnInteractHold()
    {
        // Example: Hold to slowly open door
        float holdTime = 0f;
        float requiredHoldTime = 2f;
        
        while (IsMouseOverInteractable() && holdTime < requiredHoldTime)
        {
            holdTime += Time.deltaTime;
            Debug.Log($"Holding... {holdTime:F1}/{requiredHoldTime:F1}");
            yield return null;
        }
        
        if (holdTime >= requiredHoldTime)
        {
            ToggleDoor();
        }
    }

    public override void OnHighlight()
    {
        // Show outline or change color
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public override void OnUnhighlight()
    {
        // Restore original appearance
        GetComponent<Renderer>().material.color = Color.white;
    }

    private void ToggleDoor()
    {
        _isOpen = !_isOpen;
        _doorAnimator.SetBool("IsOpen", _isOpen);
        Debug.Log(_isOpen ? "Door opened" : "Door closed");
    }
}
```

### 3. Setup Instructions

- Add the component to your interactable GameObject
- Configure the Input Handler in the inspector
- Set up Layer Mask for interaction detection
- Configure Camera reference (will use Main Camera by default)

```csharp
// Inspector configuration example:
[SerializeField] private InputHandler _inputHandler;  // Drag your input handler here
[SerializeField] private LayerMask _interactionMask;  // Set to your interactable layers
[SerializeField] private Camera _camera;              // Will auto-detect Main Camera
```

## 📖 API Reference

**IInteractable Interface**

| Method	| Description |
|-----------|-------------|
| 'OnInteractStart()' |	Called when interaction begins |
| 'OnInteractHold()' |		Coroutine for continuous/hold interactions |
| 'OnInteractEnd()' |		Called when interaction ends |
| 'OnHighlight()' |		Called when object is mouse-over highlighted |
| 'OnUnhighlight()' |		Called when highlight is removed |

**BaseInteractable Protected Methods**

| Method |	Description |
|-----------|-------------|
| 'IsMouseOverInteractable()' |		Checks if mouse is currently over the object |
| 'UpdateHighlight()' |		Automatically manages highlight state |

## 🔧 Configuration

### Input Handler Setup

The sample implementation requires an InputHandler with these events:

```csharp
public event Action OnInteractPressed;
public event Action OnInteractReleased;
```

### Layer Mask Configuration

Set up your interaction layers in the Inspector:
- Create a layer for interactable objects (e.g., "Interactable")
- Assign interactable objects to this layer
- Set the _interactionMask to only include this layer

## 💡 Usage Examples

### Instant Interaction (Button)

```csharp
public override void OnInteractStart()
{
    base.OnInteractStart();
    // Immediate action - no hold required
    GetComponent<Button>().onClick.Invoke();
}
```

### Hold Interaction (Charging)

```csharp
public override System.Collections.IEnumerator OnInteractHold()
{
    float charge = 0f;
    float maxCharge = 3f;
    
    while (IsMouseOverInteractable() && charge < maxCharge)
    {
        charge += Time.deltaTime;
        UpdateChargeVisual(charge / maxCharge);
        yield return null;
    }
    
    if (charge >= maxCharge)
    {
        ExecuteFullyChargedAction();
    }
}
```

### Continuous Interaction (Dragging)

```csharp
public override System.Collections.IEnumerator OnInteractHold()
{
    while (IsMouseOverInteractable())
    {
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;
        yield return null;
    }
}
```

## 🛡️ Best Practices

- Always call base methods when overriding in derived classes
- Use Layer Masks for efficient raycasting
- Implement proper cleanup in OnInteractEnd()
- Provide visual feedback in OnHighlight()/OnUnhighlight()
- Use coroutines wisely for long-running interactions

### Proper Base Class Usage
```csharp
public override void OnInteractStart()
{
    base.OnInteractStart(); // Important: call base first
    // Your custom logic here
}

public override void OnInteractEnd()
{
    // Your cleanup logic here
    base.OnInteractEnd(); // Important: call base last
}
```

## 🎯 Use Cases

- Doors & Switches - Instant activation
- Charging Weapons - Hold-to-charge mechanics
- Draggable Objects - Continuous movement while held
- Contextual Actions - Different interactions based on object type
- Puzzle Elements - Complex multi-step interactions
- UI Elements - Enhanced button interactions

## 🤝 Contributing

This system is part of my professional portfolio. Feel free to:

- Use in your personal or commercial projects
- Extend with additional interaction types
- Adapt for your specific game needs


