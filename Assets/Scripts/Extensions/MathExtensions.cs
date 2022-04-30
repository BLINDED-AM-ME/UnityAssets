using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME.Extensions
{
	public static class MathExtensions{

		#region Conversion
		/// <summary>
		/// Convert degrees to radians.
		/// </summary>
		/// <returns>radians.</returns>
		/// <param name="degrees">angle</param>
		public static float ToRadians(float degrees)
		{
			return degrees * 0.01745329f;
		}

		/// <summary>
		/// Convert radians to degrees.
		/// </summary>
		/// <returns>degrees.</returns>
		/// <param name="radians">angle</param>
		public static float ToDegrees(float radians)
		{
			return radians * 57.2957795f;
		}

		/// <summary> Convert a value within a range into another range. </summary>
		/// <remarks> Example 0 < 5 < 10 to 0 < 0.5 < 1 </remarks>
		public static float ConvertRange(float value, float oldMin, float oldMax, float newMin, float newMax)
		{
			// values need to not be int
			//  a <= x <= b 
			// 	0 <= (x-a)/(b-a) <=1
			// 	new_value = ( (old_value - old_min) / (old_max - old_min) ) * (new_max - new_min) + newmin

			value = Mathf.Max(oldMin, Mathf.Min(value, oldMax));

			return ((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
		}
		#endregion

		public static class Geometry
		{
			// Geometry is a branch of mathematics
			// that studies the sizes, shapes, positions, angles and dimensions of things.

			/// <summary> Convert degrees to radians. </summary>
			/// <returns> radians </returns>
			/// <param name="degrees">angle</param>
			public static float ToRadians(float degrees)
			{
				return degrees * 0.01745329f;
			}

			/// <summary> Convert radians to degrees. </summary>
			/// <returns> degrees </returns>
			/// <param name="radians">angle</param>
			public static float ToDegrees(float radians)
			{
				return radians * 57.2957795f;
			}

			/// <summary> Returns a 2D vector direction going counter clockwise from (1,0) </summary>
			/// <returns> 2D vector </returns>
			/// <param name="angleDegrees">Angle in degrees.</param>
			public static Vector2 AngleToDir2D(float angleDegrees)
			{
				var radians = ToRadians(angleDegrees);
				return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
			}

			/// <returns> the Z coordinate of the cross product. </returns>
			public static float Cross(Vector2 A, Vector2 B)
			{
				return A.x * B.y - B.x * A.y;
			}

			/// <summary> Rotate a point or direction around pointZero(0,0,0) by the given axis.</summary>
			/// <param name="vector">point or direction.</param>
			/// <param name="axis">roll, pitch, and yaw.</param>
			/// <param name="angleDegrees">Angle degrees.</param>
			public static Vector3 RotateAround(Vector3 vector, Vector3 axis, float angleDegrees)
			{
				// Rodrigues' rotation formula uses radians
				// Vector3 newDir = Mathf.Cos(angle) * dir + Mathf.Sin(angle) * Vector3.Cross(axis, dir) + (1.0f - Mathf.Cos(angle)) * Vector3.Dot(axis,dir) * axis;

				// may also use
				// Quaternion.AngleAxis(angleDegrees, axis) * dir

				var radians = ToRadians(angleDegrees);

				return Mathf.Cos(radians) * vector
					 + Mathf.Sin(radians) * Vector3.Cross(axis, vector)
					 + (1.0f - Mathf.Cos(radians)) * Vector3.Dot(axis, vector) * axis;

			}

			/// <summary> Rotate a point or direction around point by the given axis. </summary>
			/// <param name="vector">point or direction.</param>
			/// <param name="point">center of the universe.</param>
			/// <param name="axis">roll, pitch, and yaw.</param>
			/// <param name="angleDegrees">Angle degrees.</param>
			public static Vector3 RotateAround(Vector3 vector, Vector3 point, Vector3 axis, float angleDegrees)
			{
				// to relative
				Vector3 x = vector - point;
				// rotation
				x = RotateAround(x, axis, angleDegrees);
				// to world
				return x + point;
			}

			public static Vector3 ScaleIndirection(Vector3 origScale, float scaling, Vector3 normal)
			{
				return origScale + (scaling - 1.0f) * Vector3.Project(origScale, normal);
			}

			public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
			{
				// from Unity Standard Assets
				// comments are no use here... it's the catmull-rom equation.
				// Un-magic this, lord vector!
				return 0.5f * ((2f * p1) + (-p0 + p2) * i + (2f * p0 - 5f * p1 + 4f * p2 - p3) * i * i + (-p0 + 3f * p1 - 3f * p2 + p3) * i * i * i);
			}

			/// <summary> 2D Vectors only, try a Plane if 3D </summary>
			/// <param name="intersectionPoint"></param>
			/// <param name="doSegmentsIntersect"></param>
			/// <returns> true if lines intersect, false if lines are parallel </returns>
			public static bool TryGetIntersection(
				Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
				out Vector2 intersectionPoint,
				out bool doSegmentsIntersect)
			{
				// Direction and Magnitude
				Vector2 dir1 = p2 - p1;
				Vector2 dir2 = p4 - p3;

				float crossProduct = Cross(dir1, dir2);

				if (crossProduct == 0)
				{
					// The lines are parallel
					intersectionPoint = Vector2.zero;
					doSegmentsIntersect = false;
					return false;
				}

				float t1  = (p3.x - p1.x) * dir1.y;
					  t1 += (p1.y - p3.y) * dir1.x;
					  t1 /= crossProduct;

				intersectionPoint = new Vector2(p3.x + dir2.x * t1,
												p3.y + dir2.y * t1);

				float t2  = (p1.x - p3.x) * dir2.y;
					  t2 += (p3.y - p1.y) * dir2.x;
					  t2 /= -crossProduct;

				doSegmentsIntersect = t1 >= 0 && t1 <= 1
								   && t2 >= 0 && t2 <= 1;

				return true;
			}

			public static Vector3 GetPointInTriangle(Vector2 uv, Vector3 point1, Vector3 point2, Vector3 point3)
			{
				//point = (1 - sqrt(u)) * A + (sqrt(u) * (1 - v)) * B + (sqrt(u) * v) * C

				Vector3 point = (1.0f - Mathf.Sqrt(uv.x)) * point1;
				point += (Mathf.Sqrt(uv.x) * (1.0f - uv.y)) * point2;
				point += (Mathf.Sqrt(uv.x) * uv.y) * point3;

				return point;
			}

			/// <returns> Return true if the point is in the polygon. </returns>
			public static bool IsPointInPolygon(IEnumerable<Vector2> polygon, Vector2 point)
			{
				float totalAngle = 0.0f;

				var vertexCount = polygon.Count();
				for (int i = 0; i < vertexCount; i++)
				{
					totalAngle += Vector2.SignedAngle(
						polygon.ElementAt(i) - point,
						polygon.ElementAt((i + 1) % vertexCount) - point);
				}

				return Math.Abs(totalAngle) > 1;
			}

			/// <summary>
            /// A strictly convex polygon is a convex polygonal 
            /// such that no line contains two of its edges. 
            /// In a convex polygon, all interior angles are less than or equal to 180 degrees, 
            /// while in a strictly convex polygon all interior angles are strictly less than 180 degrees.
            /// </summary>
            public static bool IsPolygonConvex(IEnumerable<Vector2> polygon)
			{
				Vector2 A, B, C;
				float crossProduct;
				bool? isPositive = null;

				var vertexCount = polygon.Count();
				for (int i = 0; i < vertexCount; i++)
				{
					A = polygon.ElementAt(i);
					B = polygon.ElementAt((i + 1) % vertexCount);
					C = polygon.ElementAt((i + 2) % vertexCount);

					// BA x BC
					var BA = A - B;
					var BC = C - B;
					//BA.Normalize();
					//BC.Normalize();

					crossProduct = Cross(BA, BC);

					// all must have the same sign // 0 is neutral
					if (crossProduct < 0)
                    {
                        if (isPositive == null)
                            isPositive = false;
                        else if (isPositive == true)
                            return false;
                    }
                    else if (crossProduct > 0)
                    {
                        if (isPositive == null)
                            isPositive = true;
                        else if (isPositive == false)
                            return false;
                    }
                }

				// if you made it this far
                return true;
            }

			/// <returns> Return true if the polygon is oriented clockwise. </returns>
			public static bool IsPolygonClockwiseOrientated(IEnumerable<Vector2> polygon)
			{
				return GetPolygonSignedArea(polygon) > 0;
			}

			/// <returns> positive value of SignedArea </returns>
			public static float GetPolygonArea(IEnumerable<Vector2> polygon)
			{
				return Math.Abs(GetPolygonSignedArea(polygon));
			}

			/// <summary> Calculate value of shoelace formula </summary>
			/// <returns> The value will be negative if the polygon is oriented clockwise. </returns>
			public static float GetPolygonSignedArea(IEnumerable<Vector2> polygon)
			{
				Vector2 A, B;
				float area = 0;

				var vertexCount = polygon.Count();
				for (int i = 0; i < vertexCount; i++)
				{
					A = polygon.ElementAt(i);
					B = polygon.ElementAt((i + 1) % vertexCount);

					area += (A.x + B.x) * (A.y - B.y);
				}

				area *= 0.5f;
				return area;
			}

			/// <summary> 
			/// The Rabit hole <see href="https://en.wikipedia.org/wiki/Polygon_triangulation"/>
			/// <see href="https://en.wikipedia.org/wiki/Point-set_triangulation"/>
			/// </summary>
			public static Task<int[]> TriangulatePolygonAsync(IEnumerable<Vector2> polygon, CancellationToken cancellationToken = default)
            {
				cancellationToken.ThrowIfCancellationRequested();
				return Task.Run(() =>
				{
					if (IsPolygonConvex(polygon))
						return TriangulateConvexPolygonAsync(polygon, cancellationToken);
					else
						return TriangulateNonConvexPolygonAsync(polygon, cancellationToken);
                });
			}

			/// <summary> Like Ear clipping method <see href="https://en.wikipedia.org/wiki/Convex_polygon"/> </summary>
			/// <returns> array[*3] of triangle indices </returns>
			public static Task<int[]> TriangulateConvexPolygonAsync(IEnumerable<Vector2> polygon, CancellationToken cancellationToken = default)
			{
				cancellationToken.ThrowIfCancellationRequested();
				return Task.Run(() =>
				{
					// indices should be in order like a closed chain of vertices
					// each new triangle removes 2 edges but creates 1 new edge
					var polygonVertexCount = polygon.Count();

					// the countdown
					var availableIndices = new List<int>(polygonVertexCount);
					for (int i = 0; i < polygonVertexCount; i++)
						availableIndices.Add(i); // {0, 1, 2, 3, ... etc}

					var triangleCount = polygonVertexCount - 2;
					var triangles = new List<int>(triangleCount * 3);

					int iterator_1 = 0, iterator_2, iterator_3;
					for (int i = 0; i < triangleCount; i++)
					{
						cancellationToken.ThrowIfCancellationRequested();

						iterator_1 = i % availableIndices.Count;
						iterator_2 = (i + 1) % availableIndices.Count;
						iterator_3 = (i + 2) % availableIndices.Count;

						triangles.Add(availableIndices[iterator_1]);
						triangles.Add(availableIndices[iterator_2]);
						triangles.Add(availableIndices[iterator_3]);

						// adjust by removing the middle vertex
						availableIndices.RemoveAt(iterator_2);
						// {1, 2, 3} -> {1, 3}
					}

					return triangles.ToArray();
				});
			}

            /// <returns> array[*3] of triangle indices </returns>
            public static Task<int[]> TriangulateNonConvexPolygonAsync(IEnumerable<Vector2> polygon, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.Run(() =>
                {
                    // indices should be in order like a closed chain of vertices
                    // each new triangle removes 2 edges but creates 1 new edge
					
					var polygonVertexCount = polygon.Count();

					// the countdown
                    var availableIndices = new List<int>(polygonVertexCount);
					
					// Orient the polygon clockwise
                    if (IsPolygonClockwiseOrientated(polygon))
						for (int i = 0; i < polygonVertexCount; i++)
						    availableIndices.Add(i); // {0, 1, 2, 3, ... etc}
					else
						for (int i = polygonVertexCount-1; i >= 0; i--)
							availableIndices.Add(i); // {etc..., 3, 2, 1, 0}

					// created
					var triangleCount = polygonVertexCount - 2;
					var triangles = new List<int>(triangleCount * 3);

					int iterator_1, iterator_2, iterator_3;
					int index_1, index_2, index_3;
					var triangle = new Vector2[3];
					bool isEar;
					for(int i = 0; i < triangleCount-1; i++)
                    {
						cancellationToken.ThrowIfCancellationRequested();

						// Find an ear.
						for (iterator_1 = 0; iterator_1 < availableIndices.Count; iterator_1++)
                        {
							cancellationToken.ThrowIfCancellationRequested();

							iterator_2 = (iterator_1 + 1) % availableIndices.Count;
							iterator_3 = (iterator_2 + 1) % availableIndices.Count;

							index_1 = availableIndices[iterator_1];
							index_2 = availableIndices[iterator_2];
							index_3 = availableIndices[iterator_3];

							triangle[0] = polygon.ElementAt(index_1);
							triangle[1] = polygon.ElementAt(index_2);
							triangle[2] = polygon.ElementAt(index_3);

							// if polygon is clockwise and the angle of BA and BC is > 180
							// signed Angle will make it negative so < 0
							// it is not an ear
							isEar = true;
							if (Vector2.SignedAngle(triangle[0] - triangle[1], triangle[2] - triangle[1]) < 0)
							{
								isEar = false;
							}
							else // will this triangle will contain another point?
							{
								foreach (var otherIndex in availableIndices)
								{
									cancellationToken.ThrowIfCancellationRequested();

									if (otherIndex != index_1 // can't be one of the current
									 && otherIndex != index_2
									 && otherIndex != index_3
									 && IsPointInPolygon(triangle, polygon.ElementAt(otherIndex)))
									{
										isEar = false;
										break;
									}
								}
							}

							// Found one
							if (isEar)
							{
								triangles.Add(index_1);
								triangles.Add(index_2);
								triangles.Add(index_3);

								// adjust by removing the middle vertex
								availableIndices.RemoveAt(iterator_2);
								// {1, 2, 3} -> {1, 3}
								break;
							}
						}
                    }

                    // Copy the last three points into their own triangle.
                    triangles.Add(availableIndices[0]);
                    triangles.Add(availableIndices[1]);
                    triangles.Add(availableIndices[2]);

                    return triangles.ToArray();
                });

			}

		}

		public static class Trajectory
		{
			// https://en.wikipedia.org/wiki/Trajectory_of_a_projectile

			// Think 2D Graph
			// x = Distance(Horizontal)
			// y = Height(Vertical)
			// and Speed, Angle, Time, and Gravity

			/// <summary> Get the velocity needed to reach distance </summary>
			public static float GetVeloctiy(float distance, float angleDegrees = 45.0f, float gravity = 9.81f)
			{
				return GetVeloctiy(new Vector2(distance, 0.0f), angleDegrees, gravity);
			}

			/// <summary> Get the velocity needed to reach target </summary>
			public static float GetVeloctiy(Vector2 target, float angleDegrees = 45.0f, float gravity = 9.81f)
			{
				var x = target.y + Mathf.Sqrt(Mathf.Pow(target.x, 2) + Mathf.Pow(target.y, 2));
				return Mathf.Sqrt(x * gravity);
			}

			/// <summary> Time before reaching the distance </summary>
			public static float GetTime(float distance, float initVelocity, float angleDegrees = 45.0f)
			{
				var radians = ToRadians(angleDegrees);
				var vCos = initVelocity * Mathf.Cos(radians);
				return distance / vCos;
			}

			/// <summary> Height at Time </summary>
			public static float GetHeightAtTime(float time, float initVelocity, float angleDegrees = 45.0f, float gravity = 9.81f)
			{
				var radians = ToRadians(angleDegrees);

				var vTimeSin = initVelocity * time * Mathf.Sin(radians);
				var gt = 0.5f * gravity * Mathf.Pow(time, 2);

				return vTimeSin - gt;
			}

			/// <summary> Height at Distance </summary>
			public static float GetHeightAtDistance(float distance, float initVelocity, float angleDegrees = 45.0f, float gravity = 9.81f)
			{
				var radians = ToRadians(angleDegrees);

				var vCos = initVelocity * Mathf.Cos(radians);
				var dTan = distance * Mathf.Tan(radians);
				var gD2 = gravity * Mathf.Pow(distance, 2);

				return dTan - (gD2 / (2.0f * Mathf.Pow(vCos, 2)));
			}

			/// <summary> The "angle of reach" is the angle (θ) 
			/// at which a projectile must be launched in order to go a distance d,
			/// given the initial velocity v </summary>
			public static float GetAngle(float distance, float initVelocity, float gravity = 9.81f, bool isSteep = false)
			{
				var gdv = gravity * distance / Mathf.Pow(initVelocity, 2);
				var radians = isSteep ? 0.5f * Mathf.Acos(gdv)
									  : 0.5f * Mathf.Asin(gdv);

				return ToDegrees(radians);
			}

			/// <summary> The "angle of reach" is the angle (θ) 
			/// at which a projectile must be launched in order to hit target,
			/// given the initial velocity v </summary>
			public static float GetAngle(Vector2 target, float initVelocity, float gravity = 9.81f, bool isSteep = false)
			{
				var x = Mathf.Pow(initVelocity, 2.0f);
				var y = Mathf.Sqrt(Mathf.Pow(initVelocity, 4.0f) - (gravity * ((gravity * Mathf.Pow(target.x, 2.0f)) + (2.0f * target.y * Mathf.Pow(initVelocity, 2.0f)))));

				if (isSteep)
					x += y;
				else
					x -= y;

				var radians = Mathf.Atan(x / gravity * target.x);
				return ToDegrees(radians);
			}

			/// <summary> Flight Time before hitting the ground </summary>
			public static float GetMaxTime(float initVelocity, float angleDegrees = 45.0f, float gravity = 9.81f, float? initHeight = null)
			{
				var radians = ToRadians(angleDegrees);
				var vSin = initVelocity * Mathf.Sin(radians);

				if ((initHeight ?? 0) <= 0)
					return 2.0f * vSin / gravity;

				var distance = vSin + Mathf.Sqrt((float)(Mathf.Pow(vSin, 2) + initHeight));

				return distance / gravity;
			}

			/// <summary> Flight Distance before hitting the ground </summary>
			public static float GetMaxDistance(float initVelocity, float angleDegrees = 45.0f, float gravity = 9.81f, float? initHeight = null)
			{
				var radians = ToRadians(angleDegrees);

				if ((initHeight ?? 0) <= 0)
					return Mathf.Pow(initVelocity, 2) * Mathf.Sin(2 * radians) / gravity;

				var vSin = initVelocity * Mathf.Sin(radians);
				var vCos = initVelocity * Mathf.Cos(radians);

				var x = vCos / gravity;
				var y = vSin + Mathf.Sqrt((float)(Mathf.Pow(vSin, 2) + 2.0f * gravity * initHeight));

				return x * y;
			}

			/// <summary> Height before it starts to fall </summary>
			public static float GetMaxHeight(float initVelocity, float angleDegrees = 90.0f, float gravity = 9.81f)
			{
				if (angleDegrees <= 0)
					return 0.0f;

				var radians = ToRadians(angleDegrees);
				var v2Sin2 = Mathf.Pow(initVelocity, 2) * Mathf.Pow(Mathf.Sin(radians), 2);

				return v2Sin2 / (2.0f * gravity);
			}

			public static float GetTimeToMaxHeight(float initVelocity, float angleDegrees = 45.0f, float gravity = 9.81f)
			{
				var radians = ToRadians(angleDegrees);
				var vSin = initVelocity * Mathf.Sin(radians);
				return vSin / gravity;
			}

		}

	}
}