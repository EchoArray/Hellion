
internal enum CastType
{
    Internal,
    Awake
}


internal enum Priority
{
    Low,
    Medium,
    High
}

#region Vector Related
internal enum ForceDirection
{
    Omni,
    LocalUp,
    LocalForward,
    LocalRight,
    Up,
    Forward,
    Right,
    Custom
}
internal enum VelocityApplication
{
    Set,
    Add
}

internal enum VelocityType
{
    Initial,
    Constant
}

internal enum Direction
{
    // World
    WorldUp, WorldDown, WorldForward, WorldBackward, WorldLeft, WorldRight,
    //Local 
    LocalUp, LocalDown, LocalForward, LocalBackward, LocalLeft, LocalRight
}
internal enum WorldDirection
{
    Up, Down, Forward, Backward, Left, Right,
}
internal enum LocalDirection
{
    Up, Down, Forward, Backward, Left, Right
}
#endregion


internal enum MovementType
{
    Translation,
    Velocity
}

public enum SurfaceType
{
    Generic,

    Water,
    Liquid2,
    Liquid3,

    Dirt,
    Mud,
    Sand,
    Grass,
    GroundA,
    GroundB,
    GroundC,

    Rock1,
    Rock2,
    Rock3,

    Organic1,
    Organic2,
    Organic3,
}

internal enum EquipmentType
{
    None,
    WeaponClassA,
    WeaponClassB,
    WeaponClassC,
    WeaponClassD,
    WeaponClassE,
    WeaponClassF,
    WeaponClassG,
    WeaponClassH,
    GrenadeA,
    GrenadeB,
    GrenadeC,
    GrenadeD,
    Health,
    Shield,

}