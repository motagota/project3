using UnityEngine;

// Enum to define the types of connections between conveyors and other components
public enum ConveyorConnectionType
{
    None,       // No connection
    Input,      // Connection receives items (input to the conveyor)
    Output,     // Connection sends items (output from the conveyor)
    Bidirectional // Connection can both send and receive items
    ,
    LeftSide,
    RightSide,
    Previous,
    Next
}