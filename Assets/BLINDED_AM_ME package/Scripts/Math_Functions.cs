//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Youtube stuff: https://www.youtube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

using UnityEngine;
using System.Collections;

namespace BLINDED_AM_ME{

	public class Math_Functions{

		#region Conversion

		/// <summary>
		/// Convert degrees to radians.
		/// </summary>
		/// <returns>radians.</returns>
		/// <param name="degrees">angle</param>
		public static float DegreesToRadians(float degrees)
		{
			return degrees * 0.01745329f;
		}

		/// <summary>
		/// Convert radians to degrees.
		/// </summary>
		/// <returns>degrees.</returns>
		/// <param name="radians">angle</param>
		public static float RadiansToDegrees(float rads)
		{
			return rads * 57.2957795f;
		}

		/// <summary>
		/// Convert a value within a scope into another scope. Example 5 between 0 and 10 equals 0.5 between 0 and 1
		/// </summary>
		/// <returns>.</returns>
		/// <param name="value">The value within a scope.</param>
		/// <param name="oldMin">The minimum of the current scope.</param>
		/// <param name="oldMax">The maximum of the current scope.</param>
		/// <param name="newMin">The minimum of the new scope.</param>
		/// <param name="newMax">The maximum of the new scope.</param>
		public static float Value_from_another_Scope(float value, float oldMin, float oldMax, float newMin, float newMax)
		{
			// one scoped value to another scope
			// values need to be floats
			//  a <= x <= b 
			// 	0 <= (x-a)/(b-a) <=1
			// 	new_value = ( (old_value - old_min) / (old_max - old_min) ) * (new_max - new_min) + newmin

			return ( (value - oldMin) / (oldMax - oldMin) ) * (newMax - newMin) + newMin;
		}

		#endregion
	
		#region 2D

		/// <summary>
		/// Returns a 2D vector direction going counter clockwise from (1,0)
		/// </summary>
		/// <returns> 2D vector </returns>
		/// <param name="angleDegrees">Angle in degrees.</param>
		public static Vector2 AngleToVector2D(float angleDegrees)
		{
			return new Vector2(
				Mathf.Cos(DegreesToRadians(angleDegrees)),
				Mathf.Sin(DegreesToRadians(angleDegrees))
			);

		}

		#endregion

		#region MISC

		/// <summary>
		/// Rotate a vector point or direction around point(0,0,0) by the given axis.
		/// </summary>
		/// <returns>.</returns>
		/// <param name="vector">point or direction.</param>
		/// <param name="axis">roll, pitch, and yaw.</param>
		/// <param name="angleDegrees">Angle degrees.</param>
		public static Vector3 Rotate_Vector(Vector3 vector, Vector3 axis, float angleDegrees){

			// Rodrigues' rotation formula uses radians
			// Vector3 newDir = Mathf.Cos(angle) * dir + Mathf.Sin(angle) * Vector3.Cross(axis, dir) + (1.0f - Mathf.Cos(angle)) * Vector3.Dot(axis,dir) * axis;

			// may also use
			// Quaternion.AngleAxis(angleDegrees, axis) * dir

			return Mathf.Cos(DegreesToRadians(angleDegrees)) * vector
				+ Mathf.Sin(DegreesToRadians(angleDegrees)) * Vector3.Cross(axis, vector)
				+ (1.0f - Mathf.Cos(DegreesToRadians(angleDegrees))) * Vector3.Dot(axis,vector) * axis;

		}


		public static Vector3 Point_In_Triangle(Vector2 uv, Vector3 point1, Vector3 point2, Vector3 point3){

			//point = (1 - sqrt(u)) * A + (sqrt(u) * (1 - v)) * B + (sqrt(u) * v) * C

			Vector3 point =  (1.0f - Mathf.Sqrt(uv.x)) * point1;
			point += (Mathf.Sqrt(uv.x) * (1.0f - uv.y)) * point2;
			point += (Mathf.Sqrt(uv.x) * uv.y) * point3;

			return point;

		}

		public static Vector3 ScaleIndirection(Vector3 origScale, float scaling, Vector3 normal){

			return origScale + (scaling - 1.0f) * Vector3.Project(origScale, normal);

		}

		public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
		{
			// from Standard Assets
			// comments are no use here... it's the catmull-rom equation.
			// Un-magic this, lord vector!
			return 0.5f * ((2f*p1) + (-p0 + p2)*i + (2f*p0 - 5f*p1 + 4f*p2 - p3)*i*i + (-p0 + 3f*p1 - 3f*p2 + p3)*i*i*i);
		}

		#endregion

		#region Trajectory

		// https://en.wikipedia.org/wiki/Trajectory_of_a_projectile

		/// <summary>
		/// if returns false, increase initSpeed
		/// </summary>
		public static bool Trajectory_Can_Hit_Point(Vector2 point, float initSpeed, float gravity = 9.81f)
		{

			if(initSpeed * initSpeed * initSpeed * initSpeed - 
				gravity * (gravity * point.x * point.x + 2.0f * point.y * initSpeed * initSpeed) >= 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// returns the higher angle
		/// </summary>
		public static float Trajectory_Find_Needed_Angle1(Vector2 point, float initSpeed = 1.0f, float gravity = 9.81f)
		{
			return RadiansToDegrees(
				Mathf.Atan(
				(Mathf.Pow(initSpeed, 2.0f) + Mathf.Sqrt( Mathf.Pow(initSpeed, 4.0f) - (gravity * ((gravity * Mathf.Pow(point.x, 2.0f)) + (2.0f * point.y * Mathf.Pow(initSpeed, 2.0f))))))
				/(gravity * point.x))
			);    
		}

		/// <summary>
		/// returns the lower angle
		/// </summary>
		public static float Trajectory_Find_Needed_Angle2(Vector2 point, float initSpeed = 1.0f, float gravity = 9.81f)
		{
			return RadiansToDegrees( 
				Mathf.Atan(
				(Mathf.Pow(initSpeed, 2.0f) - Mathf.Sqrt( Mathf.Pow(initSpeed, 4.0f) - (gravity * ((gravity * Mathf.Pow(point.x, 2.0f)) + (2.0f * point.y * Mathf.Pow(initSpeed, 2.0f))))))
				/(gravity * point.x))
			);
		}
			
		public static float Trajectory_Horizontal_Distance(float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float initHeight = 0.0f){


			if(initHeight < 0){
				initHeight = 0;
				Debug.LogError("initHeight needs to be greater than 0");
				return 0.0f;
			}

			return (initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees))) / gravity
				* (
					(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
					Mathf.Sqrt( 
						(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) * 
						(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
						2.0f * gravity * initHeight
					)
				);
		}

		public static float Trajectory_Time_of_Flight(float initSpeed = 1.0f, float angleDegrees = 45.0f, float horDistance = 1.0f){

			return horDistance/(initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees)));
		}

		public static float Trajectory_Time_of_Flight(float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float initHeight = 0.0f){

			if(initHeight < 0){
				initHeight = 0;
				Debug.LogError("initHeight needs to be greater than 0");
				return 0.0f;
			}

			return (
				(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
				Mathf.Sqrt( 
					(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) * 
					(initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
					2.0f * gravity * initHeight
				)
			) / gravity;
		}

		/// <summary>
		/// angle needed to reach horizontal distance
		/// </summary>
		public static float Trajectory_Angle_of_Reach(float initSpeed = 1.0f, float gravity = 9.81f, float horDistance = 1.0f){

			return 0.5f * Mathf.Asin(gravity * horDistance / (initSpeed * initSpeed));
		}
			

		public static float Trajectory_Height_at_HorDistance( float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float horDistance = 1.0f, float initHeight = 0.0f){

			if(initHeight < 0){
				initHeight = 0;
				Debug.LogError("initHeight needs to be greater than 0");
				return 0.0f;
			}

			return initHeight + horDistance * Mathf.Tan(DegreesToRadians(angleDegrees)) - 
				( (gravity * horDistance * horDistance) / ( 2 * Mathf.Pow(initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees)), 2)) );
		}


		public static Vector3[] Trajectory_Predicted_Path(Vector3 startPoint, Vector3 initVelocity , Vector3 gravity, float initHeight = 10.0f, int numIterations = 10){
		
			if(initHeight < 0){
				initHeight = 0;
				Debug.LogError("initHeight needs to be greater than 0");
				return new Vector3[]{ startPoint };
			}

			Vector3 up = -gravity.normalized;
			Vector3 right = Vector3.Cross(initVelocity.normalized, up).normalized;
			right = Vector3.Cross(up, right);

			float initSpeed = initVelocity.magnitude;
			float angleDegrees = Vector3.Angle(initVelocity, right) * (Vector3.Dot(initVelocity, up) < 0 ? -1:1);
			float gravityAcc = gravity.magnitude;
			float horDistance = Trajectory_Horizontal_Distance(initSpeed, gravityAcc, angleDegrees, initHeight);

			Vector3[] path = new Vector3[numIterations];

			float normX = 0.0f;

			for(int i=1; i<=numIterations; i++){

				normX = (float)i / (float) numIterations;

				path[i-1] = startPoint + right * horDistance * normX;
				path[i-1] += up * (Trajectory_Height_at_HorDistance(initSpeed, gravityAcc, angleDegrees, horDistance * normX, initHeight) - initHeight);
			}

			return path;
		}

		#endregion

	}
}