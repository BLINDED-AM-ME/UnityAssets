using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME;
using BLINDED_AM_ME.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticlePathFlow : MonoBehaviour2
	{
		public Path Path;
		public bool hasRandomStartingPoints = false;

		[Range(0.0f, 5.0f)]
		public float pathWidth = 0.0f;

		private ParticleSystem _particle_system;

		private int _numParticles;
		private ParticleSystem.Particle[] _particles;

		protected override void Start()
		{
			_particle_system = GetComponent<ParticleSystem>();
		}

        private float _dist = 0;
		Matrix4x4 _matrix;
		Vector2 _offset;
		protected override void Update()
		{
			if (Path == null)
				return;

			var totalDistance = Path.TotalDistance;

			if (_particles == null
			 || _particles.Length != _particle_system.main.maxParticles)
				_particles = new ParticleSystem.Particle[_particle_system.main.maxParticles];

			_numParticles = _particle_system.GetParticles(_particles);
			for (int i = 0; i < _numParticles; i++)
			{
				var particle = _particles[i];

				// This made it based on the particle lifetime
				// float normalizedLifetime = (1.0f - obj.remainingLifetime / obj.startLifetime);
				// 
				// if(hasRandomStartingPoints){
				// 	normalizedLifetime += Get_Value_From_Random_Seed_0t1(obj.randomSeed, 100.0f);
				// 	normalizedLifetime = normalizedLifetime % 1.0f;
				// }
				// 
				// Path_Point axis = _path_comp.GetPathPoint(_path_comp.TotalDistance * normalizedLifetime);

				// This made it based on the paritcle speed
				_dist = (particle.startLifetime - particle.remainingLifetime) * particle.velocity.magnitude;

				if (hasRandomStartingPoints)
					_dist += Get_Value_From_Random_Seed_0t1(particle.randomSeed, 100.0f) * totalDistance;

				_dist %= totalDistance;

				if (pathWidth <= 0)
				{
					if (_particle_system.main.simulationSpace == ParticleSystemSimulationSpace.Local)
						_particles[i].position = transform.worldToLocalMatrix.MultiplyPoint3x4(Path.GetPoint(_dist));
					else
						_particles[i].position = Path.GetPoint(_dist);
				}
				else
				{
					_matrix = Path.GetMatrix(_dist);
					_offset = MathExtensions.Geometry.AngleToDir2D(particle.randomSeed % 360.0f)
						* Get_Value_From_Random_Seed_0t1(particle.randomSeed, 150.0f) * pathWidth;

					if (_particle_system.main.simulationSpace == ParticleSystemSimulationSpace.Local)
						_particles[i].position = transform.worldToLocalMatrix.MultiplyPoint3x4(
														_matrix.MultiplyPoint3x4(Vector3.zero + (Vector3)_offset));
					else
						_particles[i].position = _matrix.MultiplyPoint3x4(Vector3.zero + (Vector3)_offset);
				}
			}
			_particle_system.SetParticles(_particles, _numParticles);
		}

		private float Get_Value_From_Random_Seed_0t1(float seed, float converter)
		{
			return (seed % converter) / converter;
		}

	}
}