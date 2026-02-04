using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 5f;        // How far you can interact
    public LayerMask interactableLayer;        // Optional: limit to specific layers

    [Header("UI")]
    public bool showCrosshair = true;          // Show a crosshair in center
    private Texture2D crosshairTexture;
    private GUIStyle labelStyle;

    [Header("Highlight")]
    public bool enableHighlight = true;
    public Color highlightColor = Color.white;
    public float highlightIntensity = 2f;      // Emission intensity

    private Camera cam;
    private GameObject currentLookTarget;      // What we're currently looking at
    private GameObject lastLookTarget;         // Previously looked at object
    private Material[] originalMaterials;      // Store original materials
    private Renderer targetRenderer;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraInteraction must be attached to a Camera!");
        }

        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, Color.white);
        crosshairTexture.Apply();

        labelStyle = new GUIStyle
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState { textColor = Color.white }
        };
    }

    void Update()
    {
        CheckForInteractable();

        // Left mouse button to interact with whatever is at center of screen
        if (Input.GetMouseButtonDown(0) && currentLookTarget != null)
        {
            InteractWith(currentLookTarget);
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Visualize ray
        Debug.DrawRay(
            ray.origin,
            ray.direction * interactionRange,
            currentLookTarget != null ? Color.green : Color.red
        );

        // Raycast from center of camera
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            currentLookTarget = hit.collider.gameObject;

            // Highlight the new target
            if (enableHighlight && currentLookTarget != lastLookTarget)
            {
                RemoveHighlight(); // Remove from previous object
                AddHighlight(currentLookTarget);
                lastLookTarget = currentLookTarget;
            }
        }
        else
        {
            currentLookTarget = null;

            // Remove highlight when not looking at anything
            if (enableHighlight && lastLookTarget != null)
            {
                RemoveHighlight();
                lastLookTarget = null;
            }
        }
    }

    void AddHighlight(GameObject obj)
    {
        targetRenderer = obj.GetComponent<Renderer>();
        if (targetRenderer == null) return;

        // Store original materials
        originalMaterials = targetRenderer.materials;

        // Create new materials with emission
        Material[] highlightMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            highlightMaterials[i] = new Material(originalMaterials[i]);
            highlightMaterials[i].EnableKeyword("_EMISSION");
            highlightMaterials[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }

        targetRenderer.materials = highlightMaterials;
    }

    void RemoveHighlight()
    {
        if (targetRenderer != null && originalMaterials != null)
        {
            targetRenderer.materials = originalMaterials;
            targetRenderer = null;
            originalMaterials = null;
        }
    }

    void InteractWith(GameObject obj)
    {
        IInteractable interactable =
            obj.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            interactable.Interact();
            Debug.Log("Interacting with: " + obj.name);
        }
    }

    // Draw crosshair in center of screen
    void OnGUI()
    {
        if (!showCrosshair) return;
        if (Event.current.type != EventType.Repaint) return;

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        // Update crosshair color
        crosshairTexture.SetPixel(
            0, 0,
            currentLookTarget != null ? Color.green : Color.white
        );
        crosshairTexture.Apply();

        GUI.DrawTexture(new Rect(centerX - 10f, centerY - 1f, 20f, 2f), crosshairTexture);
        GUI.DrawTexture(new Rect(centerX - 1f, centerY - 10f, 2f, 20f), crosshairTexture);

        if (currentLookTarget != null)
        {
            GUI.Label(
                new Rect(centerX - 100f, centerY + 30f, 200f, 30f),
                "[Click] " + currentLookTarget.name,
                labelStyle
            );
        }
    }

    void OnDestroy()
    {
        // Clean up on destroy
        RemoveHighlight();
    }

    /*void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        Gizmos.color = currentLookTarget != null ? Color.green : Color.red;
        Gizmos.DrawLine(
            cam.transform.position,
            cam.transform.position + cam.transform.forward * interactionRange
        );
    }*/
}
