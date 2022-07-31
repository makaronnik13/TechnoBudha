using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pose", menuName = "Pose")]
public class PoseAsset : ScriptableObject
{
    public List<BodyPartAngle> Angles;
    public List<BodyPartDistance> Disctances;
}