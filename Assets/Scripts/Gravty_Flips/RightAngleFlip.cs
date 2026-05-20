using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightAngleFlip : MonoBehaviour
{
   void OnTriggerEnter(Collider col) 
   {
	   if (col.attachedRigidbody !=null) 
	   {
		   FlipDirection(transform.forward, col.attachedRigidbody.transform);
	   }
   }


   void FlipDirection(Vector3 newUpDirection, Transform tr) 
   {
	   float angleBetweenUpDirections = Vector3.Angle(newUpDirection, tr.up);
	   float angleThreshold = 0.001f;

	   if (angleBetweenUpDirections < angleThreshold)
	   {
		   return;
	   }

	   Quaternion rotationDifference = Quaternion.FromToRotation(tr.up, newUpDirection);
	   tr.rotation = rotationDifference * tr.rotation;
   }
}
