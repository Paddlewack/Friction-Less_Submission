using System.Collections.Generic;
using UnityEngine;

public class ModelController
{
    Dictionary<WheelCollider, (Transform bone, Transform model)> wheelBoneMap = new Dictionary<WheelCollider, (Transform bone, Transform model)>();
    TankController tc;

    public ModelController(TankController ptc)
    {
        this.tc = ptc;
        MapWheelsToBones();
    }

    private void MapWheelsToBones()
    {
        if (tc.wheels.Length != tc.wheelBones.Length || tc.wheels.Length != tc.wheelModels.Length)
        {
            Debug.LogError("ERROR! Wheel bones, models and colliders must be the same length");
            return;
        }
        for (int i = 0; i < tc.wheels.Length; i++)
        {
            Transform closestWheelBone = tc.wheelBones[0];
            for (int x = 0; x < tc.wheelBones.Length; x++)
            {
                if (Vector3.Distance(tc.wheels[i].transform.position, tc.wheelBones[x].position) <
                   Vector3.Distance(tc.wheels[i].transform.position, closestWheelBone.position))
                {
                    closestWheelBone = tc.wheelBones[x];
                }
            }
            Transform closestWheelModel = tc.wheelModels[0];
            for (int x = 0; x < tc.wheelModels.Length; x++)
            {
                if (Vector3.Distance(tc.wheels[i].transform.position, tc.wheelModels[x].position) <
                    Vector3.Distance(tc.wheels[i].transform.position, closestWheelModel.position))
                {
                    closestWheelModel = tc.wheelModels[x];
                }
            }
            wheelBoneMap.Add(tc.wheels[i].wheelCollider, (closestWheelBone, closestWheelModel));
        }
    }

    public void Update()
    {
        if (wheelBoneMap.Count != 0)
        {
            foreach (KeyValuePair<WheelCollider, (Transform bone, Transform model)> pair in wheelBoneMap)
            {
                WheelCollider wheel = pair.Key;
                Vector3 pos;
                Quaternion rot;
                wheel.GetWorldPose(out pos, out rot);

                Vector3 bonePos = pos + tc.transform.TransformDirection(tc.bonePositionOffset);
                pair.Value.bone.position = Vector3.MoveTowards(pair.Value.bone.position, bonePos, tc.maxBoneMovePerFrame);

                Vector3 modelPos = pos + tc.transform.TransformDirection(tc.modelPositionOffset);
                pair.Value.model.position = Vector3.MoveTowards(pair.Value.model.position, modelPos, tc.maxBoneMovePerFrame);
            }
        }
    }
}
