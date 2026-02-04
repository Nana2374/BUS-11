using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusSeatManager : MonoBehaviour
{
    [Header("Seat Setup")]
    public Transform[] seats;  // Drag all seat transforms here in Inspector

    private Dictionary<Transform, bool> seatOccupancy = new Dictionary<Transform, bool>();

    void Start()
    {
        // Initialize all seats as unoccupied
        foreach (Transform seat in seats)
        {
            if (seat != null)
            {
                seatOccupancy[seat] = false;
            }
        }

        Debug.Log($"Bus has {seats.Length} seats available.");
    }

    // Find the first available seat
    public Transform GetAvailableSeat()
    {
        foreach (Transform seat in seats)
        {
            if (seat != null && !seatOccupancy[seat])
            {
                return seat;
            }
        }

        Debug.Log("No available seats!");
        return null;
    }

    // Mark a seat as occupied
    public void OccupySeat(Transform seat)
    {
        if (seatOccupancy.ContainsKey(seat))
        {
            seatOccupancy[seat] = true;
            Debug.Log($"Seat {seat.name} is now occupied.");
        }
    }

    // Mark a seat as available (for when passenger gets off)
    public void FreeSeat(Transform seat)
    {
        if (seatOccupancy.ContainsKey(seat))
        {
            seatOccupancy[seat] = false;
            Debug.Log($"Seat {seat.name} is now available.");
        }
    }

    // Check if a specific seat is occupied
    public bool IsSeatOccupied(Transform seat)
    {
        if (seatOccupancy.ContainsKey(seat))
        {
            return seatOccupancy[seat];
        }
        return false;
    }

    // Get total number of seats
    public int GetTotalSeats()
    {
        return seats.Length;
    }

    // Get number of occupied seats
    public int GetOccupiedSeats()
    {
        int count = 0;
        foreach (var occupied in seatOccupancy.Values)
        {
            if (occupied) count++;
        }
        return count;
    }

    // Optional: Visualize seats in editor
    void OnDrawGizmos()
    {
        if (seats == null) return;

        foreach (Transform seat in seats)
        {
            if (seat == null) continue;

            // Green = available, Red = occupied (in play mode)
            if (Application.isPlaying && seatOccupancy.ContainsKey(seat))
            {
                Gizmos.color = seatOccupancy[seat] ? Color.red : Color.green;
            }
            else
            {
                Gizmos.color = Color.yellow; // Yellow in edit mode
            }

            Gizmos.DrawWireCube(seat.position, new Vector3(0.5f, 0.5f, 0.5f));

            // Draw seat direction (forward arrow)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(seat.position, seat.forward * 0.5f);
        }
    }
}
